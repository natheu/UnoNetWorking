using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayModMgr : MonoBehaviour
{ 
    [SerializeField]
    private float DistBetweenPlayer = 2f;
    [SerializeField]
    private GameObject PrefabPlayer = null;

    private int DirectionBoard = -1;
    private int CurrentPlayer = 0;
    private float TimerCurrPlayer = 0f;

    PlayerGameData.CardType CardOnBoard;
    GameObject CardOnBoardObject;

    GameObject PrefabCard;

    List<UnoPlayer> players = new List<UnoPlayer>();

    UnityEvent<PlayerGameData.CardType> CardPlayEvent = new UnityEvent<PlayerGameData.CardType>();

    [HideInInspector]
    public DataStruct.Deck deck;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

        TimerCurrPlayer -= Time.deltaTime;
        if (TimerCurrPlayer < 0)
        {
            TimerCurrPlayer = 0;
            /*
            UpdateCurrentPlayer();
            if (NetWorkingCSharp.ServerTCP.host)
            {
                
            }
            */
        }
        //Debug.Log("trun : " + CurrentPlayer);
        UnoNetworkingGameData.GameData gameData = players[CurrentPlayer].UpdatePlayer(CardOnBoard);

        if (!gameData.Equals(default(UnoNetworkingGameData.GameData)))
        {
            // Update the board of the player host
            if (NetWorkingCSharp.ServerTCP.host)
            {
                gameData = AnalyseGameData(ref gameData);
                NetWorkingCSharp.Header header = new NetWorkingCSharp.Header(gameData, NetWorkingCSharp.EType.PLAYERACTION,
                                                                                            new NetWorkingCSharp.ServerTCP.ClientData());
                Debug.Log("Host PLay");
                GameMgr.SendNetWorkingData(header);
            }
            // Only the GameData to the server
            else
            {
                NetWorkingCSharp.Header header = new NetWorkingCSharp.Header(gameData, NetWorkingCSharp.EType.PLAYERACTION,
                                                                                            new NetWorkingCSharp.ServerTCP.ClientData());

                GameMgr.SendNetWorkingData(header);
            }
        }


        //if()

    }

    private int GetNextPlayer(int posPlayer)
    {
        int next = posPlayer + DirectionBoard;
        if (next < 0)
            next = players.Count - 1;
        else if (next >= players.Count)
            next = 0;

        Debug.Log(next);
        return next;
    }

    public UnoNetworkingGameData.GameData AnalyseGameData(ref UnoNetworkingGameData.GameData data)
    {
        switch (data.type)
        {
            case UnoNetworkingGameData.GameData.TypeData.DEFAULT:
                break;
            case UnoNetworkingGameData.GameData.TypeData.CARDPLAY:            
                AnalyseEffect(ref data);
                break;
            case UnoNetworkingGameData.GameData.TypeData.DRAWCARDS:
                ChooseDrawCard(ref data);
                break;
        }

        UpdateActionPlayer(0, data); 

        return data;
    }

    void AnalyseEffect(ref UnoNetworkingGameData.GameData data)
    {
        PlayerGameData.CardType cardType = data.CardTypePutOnBoard[0];
        // the card is a number no special effect
        if (cardType.Effect < 10)
            return;

        // the card plays is a +2
        if (cardType.Effect == 10)
        {
            List<PlayerGameData.CardType> list = new List<PlayerGameData.CardType>();
            list.Add(data.CardTypePutOnBoard[0]);
            for (int i = 0; i < 2; i++)
            {
                list.Add(deck.GetNextCard());
            }

            data.CardTypePutOnBoard = list.ToArray();
        }

        /*
        // the card plays is Turn Pass
        else if(cardType.Effect == 11)
        {

        }
        // the card plays is Invert Direction 
        else if (cardType.Effect == 12)
        {

        }
        */
    }

    public void ChooseDrawCard(ref UnoNetworkingGameData.GameData data)
    {
        data.CardTypePutOnBoard = new PlayerGameData.CardType[] { deck.GetNextCard() };
    }

    public void UpdateActionPlayer(int IdPLayerAction, UnoNetworkingGameData.GameData data)
    {
        Debug.Log("Update Player");
        switch(data.type)
        {
            case UnoNetworkingGameData.GameData.TypeData.DEFAULT:
                break;
            case UnoNetworkingGameData.GameData.TypeData.CARDPLAY:
                PlayerGameData.CardType cardType = players[CurrentPlayer].CardPlay(ref data);
                CardPlayEvent.Invoke(cardType);
                CardOnBoard = cardType;
                CardOnBoardObject.name = ((int)cardType.CardColor).ToString() + "_" + cardType.Effect.ToString();
                EffectPlayCard(cardType, data);
                break;
            case UnoNetworkingGameData.GameData.TypeData.DRAWCARDS:
                players[CurrentPlayer].DrawCards(data);
                break;
        }
    }

    private void EffectPlayCard(PlayerGameData.CardType cardType, UnoNetworkingGameData.GameData gameData)
    {
        // the card is a number no special effect
        if (cardType.Effect < 10)
        {
            CurrentPlayer = GetNextPlayer(CurrentPlayer);
            return;
        }

        // the card plays is a +2
        if(cardType.Effect == 10)
        {
            int nextPlayer = GetNextPlayer(CurrentPlayer);

            players[nextPlayer].DrawCards(gameData);

            CurrentPlayer = GetNextPlayer(nextPlayer);
            return;
        }
        
        // the card plays is Turn Pass
        if(cardType.Effect == 11)
        {
            CurrentPlayer = GetNextPlayer(GetNextPlayer(CurrentPlayer));
            // Add anim or something to tell your turn is passed
            return;
        }
        
        // the card plays is Invert Direction 
        if (cardType.Effect == 12)
        {
            DirectionBoard *= -1;
            CurrentPlayer = GetNextPlayer(CurrentPlayer);
            // animation tu show that the direction of the board changed
            return;
        }

        CurrentPlayer = GetNextPlayer(CurrentPlayer);
        return;
    }

    public void CreateBoard(UnoNetworkingGameData.GameData[] playerHandData, UnoCardTextures textures, int keyPlayer)
    {
        PrefabCard = textures.GetPrefab();

        List<Vector3> AllPos = CreateAllPos(NetWorkingCSharp.ServerTCP.ClientsGameData.Count);
        for(int i = 0; i < playerHandData.Length - 1; i++)
        {
            UnoNetworkingGameData.GameData data = playerHandData[i];

            Vector3 PosToCenter = (Vector3.zero - AllPos[i]).normalized;
            GameObject Player = Instantiate(PrefabPlayer, AllPos[i], Quaternion.FromToRotation(Vector3.forward, PosToCenter));
            players.Insert(i, Player.GetComponent<UnoPlayer>());

            UnoPlayer.EController conrollerP = UnoPlayer.EController.DEFAULT;
            //DontDestroyOnLoad(Player);
            if (NetWorkingCSharp.ServerTCP.ClientsGameData[keyPlayer].GetPosOnBoard() == i)
            {
                Camera.main.transform.position = Player.transform.Find("PosPlayer").position;
                Camera.main.transform.rotation = Quaternion.FromToRotation(Vector3.forward, PosToCenter);
                conrollerP = UnoPlayer.EController.PLAYER;
            }
            else if (NetWorkingCSharp.ServerTCP.ClientsGameData.ContainsKey(data.PosInHand))
            {
                conrollerP = UnoPlayer.EController.ENEMY;
            }
            else
                conrollerP = UnoPlayer.EController.IA;

            Player.GetComponent<UnoPlayer>().InitPlayer(data.CardTypePutOnBoard, textures, conrollerP);

        }

        CardOnBoard = playerHandData[playerHandData.Length - 1].CardTypePutOnBoard[0];

        CardOnBoardObject = Instantiate(PrefabCard, this.transform);

        CardOnBoardObject.transform.rotation = Quaternion.Euler(80, 0, 0);
        CardOnBoardObject.name = ((int)CardOnBoard.CardColor).ToString() + "_" + CardOnBoard.Effect.ToString();
        /*
        foreach(UnoNetworkingGameData.GameData data in playerHandData)
        {
            Vector3 PosToCenter = (Vector3.zero - AllPos[i]).normalized;
            GameObject Player = Instantiate(PrefabPlayer, AllPos[i], Quaternion.FromToRotation(Vector3.forward, PosToCenter));
            players.Insert(i, Player.GetComponent<UnoPlayer>());

            UnoPlayer.EController conrollerP = UnoPlayer.EController.DEFAULT;
            //DontDestroyOnLoad(Player);
            if (NetWorkingCSharp.ServerTCP.ClientsGameData[keyPlayer].GetPosOnBoard() == i)
            {
                Camera.main.transform.position = Player.transform.Find("PosPlayer").position;
                Camera.main.transform.rotation = Quaternion.FromToRotation(Vector3.forward, PosToCenter);
                conrollerP = UnoPlayer.EController.PLAYER;
            }
            else if(NetWorkingCSharp.ServerTCP.ClientsGameData.ContainsKey(data.PosInHand))
            {
                conrollerP = UnoPlayer.EController.ENEMY;
            }
            else
                conrollerP = UnoPlayer.EController.IA;

            Player.GetComponent<UnoPlayer>().InitPlayer(data.CardTypePutOnBoard, textures, conrollerP);

            i++;
        }
        */
    }

    private List<Vector3> CreateAllPos(int numberPlayer)
    {
        List<Vector3> localPositions = new List<Vector3>(numberPlayer);
        // get the angle of the first point to get the angle of the isoscel triangle 
        float Tmpangle = (1 * (Mathf.PI * 2 / numberPlayer));
        // get distance between the center (considered as the vertex of the two legs) and one of the point on base of the isoscel triangle
        // to get the radius our circle
        float radius = (float)DistBetweenPlayer / Mathf.Sin((Tmpangle / 2f) * 2f);
        if (numberPlayer == 2)
            radius = DistBetweenPlayer;
        //Debug.Log((Tmpangle / 2f) * 2f);

        for (int i = 0; i < numberPlayer; i++)
        {
            // the angle on the Unit circle between 0 - 360
            float angle = (i * (Mathf.PI * 2 / numberPlayer));
            Vector3 pos = Vector3.zero;
            pos.x = Mathf.Cos(angle) * radius;
            pos.z = Mathf.Sin(angle) * radius;

            //Debug.Log(pos);

            localPositions.Add(pos);
        }
        
        return localPositions;
    }


    public void PlayerDisconnected(int posOnBoard)
    {
        players[posOnBoard].ToControllerIA();
    }
}
