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

    [SerializeField]
    GameObject PlayModPrefab;

    [SerializeField]
    UnoCardTextures CardTextures = null;

    PlayModMgr playMod = null;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
    }

    void Update()
    {
        if(NetWorkingCSharp.ClientTCP.Tcp != null)
            AnalyseRecievValue();
    }
    public void AnalyseRecievValue()
    {
        if (NetWorkingCSharp.ClientTCP.Tcp.headersReciev.Count > 0)
        {
            NetWorkingCSharp.Header header = NetWorkingCSharp.ClientTCP.Tcp.headersReciev.Dequeue();

            switch (header.TypeData)
            {
                case NetWorkingCSharp.EType.Error:
                    break;
                    // need To put this part in the ServerTCP since it's not gameplay but general feature of Networking
                case NetWorkingCSharp.EType.WELCOME:
                    NetWorkingCSharp.ServerSend.WelcomeToServer ff = (NetWorkingCSharp.ServerSend.WelcomeToServer)header.Data;
                    InitAllClient(ff.clientsData);
                    NetWorkingCSharp.ServerTCP.ClientsGameData.Add(header.clientData.Id, PlayerGameData.CreateUnoGameData(header.clientData));
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
                    SetGamePosPlayers((int[])header.Data);
                    StartCoroutine(StartGame(header.HeaderTime));
                    break;
                case NetWorkingCSharp.EType.DISCONNECT:
                    if (NetWorkingCSharp.ServerTCP.stateGame == NetWorkingCSharp.ServerTCP.EStateGame.RUNNING)
                        playMod.PlayerDisconnected((int)header.Data);
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
            NetWorkingCSharp.ServerTCP.ClientsGameData.Add(ClientData.Id, PlayerGameData.CreateUnoGameData(ClientData));
        }
    }

    public bool CreateClient(string Ip, int port)
    {
        client = new NetWorkingCSharp.ClientTCP();
        return NetWorkingCSharp.ClientTCP.CreateClient(Ip);
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
            header.clientData = NetWorkingCSharp.ClientTCP.Tcp.clientData;
            NetWorkingCSharp.ClientTCP.SendToServer(header);
        }
    }

    public void PlayerIsReady()
    {
        NetWorkingCSharp.ClientTCP.Tcp.clientData.IsReady = !NetWorkingCSharp.ClientTCP.Tcp.clientData.IsReady;
        IsReadyReciev.Invoke(NetWorkingCSharp.ClientTCP.Tcp.clientData.Id, NetWorkingCSharp.ClientTCP.Tcp.clientData.IsReady);
        NetWorkingCSharp.ClientTCP.SendToServer(new NetWorkingCSharp.Header(null, NetWorkingCSharp.EType.PLAYERREADY,
                                                                            NetWorkingCSharp.ClientTCP.Tcp.clientData));
    }

    public void StartAGame()
    {
        Debug.Log("StartGame");
        if(NetWorkingCSharp.ServerTCP.CanStartAGame())
        {
            NetWorkingCSharp.Header header = new NetWorkingCSharp.Header(ChooseGamePosPlayer(), NetWorkingCSharp.EType.BEGINPLAY, 
                                                                            NetWorkingCSharp.ServerTCP.Clients[0].clientData);
            NetWorkingCSharp.ServerSend.SendTCPDataToAll(header);

            StartCoroutine(StartGame(0));
        }
    }

    // only use by the server
    private int[] ChooseGamePosPlayer()
    {
        //Dictionary<int, int> PosPlayers = new Dictionary<int, int>();
        int[] PosPlayers = new int[NetWorkingCSharp.ServerTCP.ClientsGameData.Count];
        List<int> allPlayer = new List<int>();
        foreach(KeyValuePair<int, PlayerGameData> client in NetWorkingCSharp.ServerTCP.ClientsGameData)
        {
            allPlayer.Add(client.Key);
        }

        for (int i = 0; i < NetWorkingCSharp.ServerTCP.ClientsGameData.Count; i++)
        {
            // the position in the list of the player choose for the position on the board
            int player = Random.Range(0, allPlayer.Count - 1);
            PosPlayers[i] = (allPlayer[player]);
            allPlayer.Remove(player);
        }

        SetGamePosPlayers(PosPlayers);

        return PosPlayers;
    }

    private void SetGamePosPlayers(int[] posPlayers)
    {
        for(int i = 0; i < posPlayers.Length; i++)
        {
                NetWorkingCSharp.ServerTCP.ClientsGameData[posPlayers[i]].SetPosOnBoard(i);
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

    private void LoadGame()
    {
        SceneManager.LoadScene("PlayScene");

        playMod = Instantiate(PlayModPrefab).GetComponent<PlayModMgr>();

    }

    public void DisconnectClient()
    {
        NetWorkingCSharp.ClientTCP.SendToServer(new NetWorkingCSharp.Header(null, NetWorkingCSharp.EType.DISCONNECT,
                                                                            NetWorkingCSharp.ClientTCP.Tcp.clientData));
        NetWorkingCSharp.ClientTCP.Disconnect();
        client = null;
    }

    private void OnDestroy()
    {
        if (NetWorkingCSharp.ServerTCP.host)
            NetWorkingCSharp.ServerTCP.CloseListener();
    }
}
