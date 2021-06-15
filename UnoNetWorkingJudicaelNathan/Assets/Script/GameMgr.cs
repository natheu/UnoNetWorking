using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameMgr : MonoBehaviour
{

    NetWorkingCSharp.ClientTCP client;
    Dictionary<int, NetWorkingCSharp.ServerTCP.ClientData> localClients = new Dictionary<int, NetWorkingCSharp.ServerTCP.ClientData>();

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
        /*if(NetWorkingCSharp.ServerTCP.host)
        {

        }*/
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
                case NetWorkingCSharp.EType.WELCOME:
                    NetWorkingCSharp.ServerSend.WelcomeToServer ff = (NetWorkingCSharp.ServerSend.WelcomeToServer)header.Data;
                    InitAllClient(ff.clientsData);
                    localClients.Add(header.clientData.Id, header.clientData);
                    Debug.Log(ff.msg + " : " + header.clientData.Name);
                    break;
                case NetWorkingCSharp.EType.MSG:
                    string msg = (string)header.Data;
                    Debug.Log(header.clientData.Name + " : " + msg);
                    OnMessageReciev.Invoke(header.clientData.Name + " : " + msg);
                    break;
                case NetWorkingCSharp.EType.UPDATENAME:
                    string name = (string)header.Data;
                    OnMessageReciev.Invoke("Player " + header.clientData.Id + " change his name to " + name);
                    localClients[header.clientData.Id] = header.clientData;
                    break;
                case NetWorkingCSharp.EType.BEGINPLAY:
                    localClients[header.clientData.Id] = header.clientData;
                    IsReadyReciev.Invoke(header.clientData.Id, header.clientData.IsReady);
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
            localClients.Add(ClientData.Id, ClientData);
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
        NetWorkingCSharp.ServerSend.SendTCPDataToAll(new NetWorkingCSharp.Header(null, NetWorkingCSharp.EType.PLAYERREADY,
                                                                            client.Tcp.clientData));
    }

    public void StartAGame()
    {
        if(NetWorkingCSharp.ServerTCP.CanStartAGame())
        {
            SceneManager.LoadScene("PlayScene");
            NetWorkingCSharp.Header header = new NetWorkingCSharp.Header(null, NetWorkingCSharp.EType.BEGINPLAY, 
                                                                            NetWorkingCSharp.ServerTCP.Clients[0].clientData);
            NetWorkingCSharp.ServerSend.SendTCPDataToAll(header);
        }
    }


    private void OnDestroy()
    {
        if (NetWorkingCSharp.ServerTCP.host)
            NetWorkingCSharp.ServerTCP.CloseListener();
    }
}
