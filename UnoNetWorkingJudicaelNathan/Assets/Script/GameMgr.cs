using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameMgr : MonoBehaviour
{

    NetWorkingCSharp.ClientTCP client;

    UnityEvent<string> OnMessageReciev = new UnityEvent<string>();
    UnityEvent<int, bool> IsReadyReciev = new UnityEvent<int, bool>();
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
    }

    void Update()
    {
        if (NetWorkingCSharp.ServerTCP.ListenClient != null && client == null)
        {
            client = NetWorkingCSharp.ServerTCP.ListenClient;
        }
        if(client != null)
        {
            AnalyseRecievValue();
        }
    }
    public void AnalyseRecievValue()
    {
        if (client.Tcp.headersReciev.Count > 0)
        {
            NetWorkingCSharp.Header header = client.Tcp.headersReciev.Dequeue();

            switch (header.TypeData)
            {
                case NetWorkingCSharp.EType.Error:
                    break;
                    // need To put this part in the ServerTCP since it's not gameplay but general feature of Networking
                case NetWorkingCSharp.EType.WELCOME:
                    NetWorkingCSharp.ServerSend.WelcomeToServer ff = (NetWorkingCSharp.ServerSend.WelcomeToServer)header.Data;
                    InitAllClient(ff.clientsData);
                    NetWorkingCSharp.ServerTCP.ClientsGameData.Add(header.clientData.Id, UnoNetworkingGameData.CreateUnoGameData(header.clientData));
                    Debug.Log(ff.msg + " : " + header.clientData.Name);
                    break;
                case NetWorkingCSharp.EType.MSG:
                    string msg = (string)header.Data;
                    Debug.Log(header.clientData.Name + " : " + msg);
                    OnMessageReciev.Invoke(header.clientData.Name + " : " + msg);
                    break;
                // need To put this part in the ServerTCP since it's not gameplay but general feature of Networking
                case NetWorkingCSharp.EType.UPDATENAME:
                    string name = (string)header.Data;
                    OnMessageReciev.Invoke("Player " + header.clientData.Id + " change his name to " + name);
                    NetWorkingCSharp.ServerTCP.ClientsGameData[header.clientData.Id].Updatename(header.clientData.Name);
                    break;
                // need To put this part in the ServerTCP since it's not gameplay but general feature of Networking
                case NetWorkingCSharp.EType.PLAYERREADY:
                    NetWorkingCSharp.ServerTCP.ClientsGameData[header.clientData.Id].SetIsReady(header.clientData.IsReady);
                    IsReadyReciev.Invoke(header.clientData.Id, header.clientData.IsReady);
                    break;
                // need To put this part in the ServerTCP since it's not gameplay but general feature of Networking
                case NetWorkingCSharp.EType.BEGINPLAY:
                    StartCoroutine(StartGame(header.HeaderTime));
                    break;
            }
        }
    }

    public void InitAllClient(List<NetWorkingCSharp.ServerTCP.ClientData> clients)
    {
        if (clients.Count == 0)
            return;
        foreach(NetWorkingCSharp.ServerTCP.ClientData ClientData in clients)
        {
            NetWorkingCSharp.ServerTCP.ClientsGameData.Add(ClientData.Id, UnoNetworkingGameData.CreateUnoGameData(ClientData));
        }
    }

    public bool CreateClient(string Ip, int port)
    {
        client = new NetWorkingCSharp.ClientTCP();
        return client.CreateClient(Ip);
    }

    public void SendMsg(string msg, NetWorkingCSharp.EType messageType = NetWorkingCSharp.EType.MSG)
    {
        NetWorkingCSharp.Header header = new NetWorkingCSharp.Header(msg, messageType, new NetWorkingCSharp.ServerTCP.ClientData());
        if (NetWorkingCSharp.ServerTCP.host)
        {
            header.clientData = NetWorkingCSharp.ServerTCP.Clients[0].clientData;
            NetWorkingCSharp.ServerSend.SendTCPDataToAll(header);
        }
        else
        {
            header.clientData = client.Tcp.clientData;
            client.SendToServer(header);
        }
    }

    public void PlayerIsReady()
    {
        client.Tcp.clientData.IsReady = !client.Tcp.clientData.IsReady;
        IsReadyReciev.Invoke(client.Tcp.clientData.Id, client.Tcp.clientData.IsReady);
        client.SendToServer(new NetWorkingCSharp.Header(null, NetWorkingCSharp.EType.PLAYERREADY,
                                                                            client.Tcp.clientData));
    }

    public void StartAGame()
    {
        Debug.Log("StartGame");
        if(NetWorkingCSharp.ServerTCP.CanStartAGame())
        {
            NetWorkingCSharp.Header header = new NetWorkingCSharp.Header(null, NetWorkingCSharp.EType.BEGINPLAY, 
                                                                            NetWorkingCSharp.ServerTCP.Clients[0].clientData);
            NetWorkingCSharp.ServerSend.SendTCPDataToAll(header);

            StartCoroutine(StartGame(0));
        }
    }

    private IEnumerator StartGame(int date)
    {
        float timeToWait = 3f - (System.DateTime.Now.Millisecond - date) / 1000f;
        if (date == 0)
            timeToWait = 3f;
        if (timeToWait > 0f)
        {
            Debug.Log("Wait : " + timeToWait);
            yield return new WaitForSeconds(timeToWait);
        }

        SceneManager.LoadScene("PlayScene");
    }

    public void DisconnectClient()
    {
        client.SendToServer(new NetWorkingCSharp.Header(null, NetWorkingCSharp.EType.DISCONNECT,
                                                                            client.Tcp.clientData));
        client.Disconnect();
        client = null;
    }

    private void OnDestroy()
    {
        if (NetWorkingCSharp.ServerTCP.host)
            NetWorkingCSharp.ServerTCP.CloseListener();
    }
}
