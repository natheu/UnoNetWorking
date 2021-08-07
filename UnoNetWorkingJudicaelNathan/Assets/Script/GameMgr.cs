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
    GameObject SaveDataPrefab;

    [SerializeField]
    UnoCardTextures CardTextures = null;

    PlayModMgr PlayMod = null;

    [SerializeField]
    CardSelector selector;
    DataStruct.Deck deckGame;

    [SerializeField]
    uint nbCardBeginning = 5;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        deckGame = new DataStruct.Deck(0);
        deckGame.CreateDeck(selector.GetFactorCards());
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
                    SetGameDataPlayer(header.HeaderTime, (UnoNetworkingGameData.GameData[])header.Data);
                    break;
                case NetWorkingCSharp.EType.PLAYERACTION:
                    if (NetWorkingCSharp.ServerTCP.host)
                        HostPlayerAction(header);
                    else
                    {
                        NetWorkingCSharp.HeaderGameData headerGame = (NetWorkingCSharp.HeaderGameData)header.Data;
                        PlayMod.UpdateActionPlayer(header.clientData.Id, headerGame);
                    }
                    break;
                case NetWorkingCSharp.EType.DISCONNECT:
                    if (header.clientData.Id != NetWorkingCSharp.ClientTCP.Tcp.clientData.Id)
                    {
                        if (NetWorkingCSharp.ServerTCP.stateGame == NetWorkingCSharp.ServerTCP.EStateGame.RUNNING)
                            PlayMod.PlayerDisconnected((int)header.Data);
                    }
                    else
                    {
                        if(PlayMod != null)
                        {
                            DisconnectLoadScene();
                        }
                        else if(NetWorkingCSharp.ServerTCP.stateGame == NetWorkingCSharp.ServerTCP.EStateGame.LOBBY)
                        {
                            Debug.Log("Not fully implemented");
                        }
                    }
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
            Debug.Log(ClientData.Name);
        }
    }

    public bool CreateServer(int maxPlayer, int port, bool isHost = true)
    {
        // nbMaxPlayer and port 50150
        NetWorkingCSharp.ServerTCP.CreateServer(maxPlayer, port, isHost);

        deckGame.Shuffle(2);

        return true;
    }

    public bool CreateClient(string Ip, int port)
    {
        client = new NetWorkingCSharp.ClientTCP();
        bool isSucces = NetWorkingCSharp.ClientTCP.CreateClient(Ip);

        if (isSucces)
            NetWorkingCSharp.ServerTCP.SetState(NetWorkingCSharp.ServerTCP.EStateGame.LOBBY);

        return isSucces;
    }

    public void SendMsg(string msg, NetWorkingCSharp.EType messageType = NetWorkingCSharp.EType.MSG)
    {
        NetWorkingCSharp.Header header = new NetWorkingCSharp.Header(msg, messageType, new NetWorkingCSharp.ServerTCP.ClientData());
        if (NetWorkingCSharp.ServerTCP.host)
        {
            if (messageType == NetWorkingCSharp.EType.UPDATENAME)
                NetWorkingCSharp.ServerTCP.ClientsGameData[0].Updatename(msg);
            header.clientData = NetWorkingCSharp.ServerTCP.Clients[0].clientData;
            NetWorkingCSharp.ServerSend.SendTCPDataToAll(header);
        }
        else
        {
            NetWorkingCSharp.ServerTCP.ClientData clientData = NetWorkingCSharp.ClientTCP.Tcp.clientData;
            if (messageType == NetWorkingCSharp.EType.UPDATENAME)
            {
                NetWorkingCSharp.ServerTCP.ClientsGameData[clientData.Id].Updatename(msg);
                NetWorkingCSharp.ClientTCP.Tcp.clientData.Name = msg;
            }
            header.clientData = clientData;
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
            UnoNetworkingGameData.GameData[] startGameData = ChooseCardAndPosPlayers();
            NetWorkingCSharp.Header header = new NetWorkingCSharp.Header(startGameData, NetWorkingCSharp.EType.BEGINPLAY, 
                                                                            NetWorkingCSharp.ServerTCP.Clients[0].clientData);
            NetWorkingCSharp.ServerSend.SendTCPDataToAll(header);

            for (int i = 0; i < startGameData.Length - 1; i++)
            {
                NetWorkingCSharp.ServerTCP.ClientsGameData[startGameData[i].PosInHand].SetStartGameData(startGameData[i].CardTypePutOnBoard, i);
            }


            StartCoroutine(StartGame(0, startGameData));
        }
    }

    // only use by the server
    // to optimize with the choixe of all the card
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

    private UnoNetworkingGameData.GameData[] ChooseCardAndPosPlayers()
    {
        int[] pos = ChooseGamePosPlayer();

        UnoNetworkingGameData.GameData[] dataplayers = new UnoNetworkingGameData.GameData[pos.Length + 1];

        for (int i = 0; i < pos.Length; i++)
        {
            dataplayers[i].PosInHand = pos[i];

            dataplayers[i].CardTypePutOnBoard = ChooseCardPlayer(nbCardBeginning);
        }
        // choose the beginning card on board
        dataplayers[pos.Length].CardTypePutOnBoard = ChooseCardPlayer(1); 

        return dataplayers;
    }

    private PlayerGameData.CardType[] ChooseCardPlayer(uint nbCardStart)
    {
        List<PlayerGameData.CardType> cards = new List<PlayerGameData.CardType>();

        for(int i = 0; i < nbCardStart; i++)
        {
            cards.Add(deckGame.GetNextCard());
        }

        return cards.ToArray();
    }

    private void SetGamePosPlayers(int[] posPlayers)
    {
        for(int i = 0; i < posPlayers.Length - 1; i++)
        {
                NetWorkingCSharp.ServerTCP.ClientsGameData[posPlayers[i]].SetPosOnBoard(i);
        }
    }

    private void SetGameDataPlayer(int time, UnoNetworkingGameData.GameData[] dataPlayers)
    {
        for(int i = 0; i < dataPlayers.Length; i++)
        {
            NetWorkingCSharp.ServerTCP.ClientsGameData[dataPlayers[i].PosInHand].SetStartGameData(dataPlayers[i].CardTypePutOnBoard, i);
        }
        StartCoroutine(StartGame(time, dataPlayers));
    }

    private IEnumerator StartGame(int date, UnoNetworkingGameData.GameData[] gameData)
    {
        float timeToWait = 3f - (System.DateTime.Now.Millisecond - date) / 1000f;
        if (date == 0)
            timeToWait = 3f;
        NetWorkingCSharp.ServerTCP.GameRunning();
        if (timeToWait > 0f)
        {
            Debug.Log("Wait : " + timeToWait);
            yield return new WaitForSeconds(timeToWait);
        }

        LoadGame(gameData);
    }
    
    private void LoadGame(UnoNetworkingGameData.GameData[] gameData)
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        Instantiate(SaveDataPrefab).GetComponent<SaveData>().beginPlayGameData = gameData;

        SceneManager.LoadScene("PlayScene");

    }

    void DisconnectLoadScene()
    {
        SceneManager.LoadScene("BeginScene");
        NetWorkingCSharp.ServerTCP.SetState(NetWorkingCSharp.ServerTCP.EStateGame.DEFAULT);
        PlayMod = null;
    }

    // called second
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if(scene.name == "PlayScene")
        {
            SaveData data = FindObjectOfType<SaveData>();

            PlayMod = Instantiate(PlayModPrefab).GetComponent<PlayModMgr>();

            int key = -1;
            if (NetWorkingCSharp.ClientTCP.Tcp != null)
                key = NetWorkingCSharp.ClientTCP.Tcp.clientData.Id;
            else if (NetWorkingCSharp.ServerTCP.host)
                key = 0;
            PlayMod.CreateBoard(data.beginPlayGameData, CardTextures, key);
            PlayMod.deck = deckGame;

        }
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void HostPlayerAction(NetWorkingCSharp.Header header)
    {
        //UnoNetworkingGameData.GameData gameData = (UnoNetworkingGameData.GameData)header.Data;
        NetWorkingCSharp.HeaderGameData headerData = (NetWorkingCSharp.HeaderGameData)header.Data;
        if (headerData.dataType == NetWorkingCSharp.HeaderGameData.EDataType.CARD)
        {
            //UnoNetworkingGameData.GameData gameData = (UnoNetworkingGameData.GameData)headerData.GameData;
            //gameData = PlayMod.AnalyseGameData(ref headerData);
            PlayMod.AnalyseGameData(ref headerData);

            //headerData.GameData = gameData;
            header.Data = headerData;

        }
        NetWorkingCSharp.ServerSend.SendTCPDataToAll(header);
    }

    public void DisconnectClient()
    {
        NetWorkingCSharp.ClientTCP.Disconnect();
        client = null;
    }

    private void OnDestroy()
    {
        if (NetWorkingCSharp.ServerTCP.host)
            NetWorkingCSharp.ServerTCP.CloseListener();
    }

    // send data in case you are the host or the client
    public static void SendNetWorkingData(NetWorkingCSharp.Header header)
    {
        if(NetWorkingCSharp.ServerTCP.host)
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
}
