using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayModMgr : MonoBehaviour
{ 
    public enum EPlayModState
    {
        PLAY,
        WAITCOLOR,
        WAITCHOOSEPLAYERCARDS
    };

    [SerializeField]
    private float DistBetweenPlayer = 2f;
    [SerializeField]
    private GameObject PrefabPlayer = null;
    [SerializeField]
    private float PlayTimePlayer = 15f;

    private int DirectionBoard = -1;
    private int CurrentPlayer = 0;
    private float TimerCurrPlayer = 100f;

    UnoCardTextures TexturesCards;
    PlayerGameData.CardType CardOnBoard;
    GameObject CardOnBoardObject;

    GameObject PrefabCard;
    [SerializeField]
    GameObject PrefabChooseColor;

    List<UnoPlayer> players = new List<UnoPlayer>();

    UnityEvent<PlayerGameData.CardType> CardPlayEvent = new UnityEvent<PlayerGameData.CardType>();
    UnityEvent<int> UpdateDirBoard = new UnityEvent<int>();

    [HideInInspector]
    public DataStruct.Deck deck;

    EPlayModState state;
    bool FirstCard = false;

    // Start is called before the first frame update
    void Start()
    {
        CardPlayEvent.AddListener(card => UpdateOnBoardCard(card));
        UpdateDirBoard.AddListener(dir =>
        {
            transform.GetChild(0).GetComponent<AnimDirBoard>().UpdateDirection(dir);
        });
    }

    private void UpdateOnBoardCard(PlayerGameData.CardType toCard)
    {
        CardOnBoard = toCard;
        CardOnBoardObject.name = ((int)toCard.CardColor).ToString() + "_" + toCard.Effect.ToString();
        //Debug.Log(CardOnBoardObject.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material.mainTexture);
        CardOnBoardObject.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material.mainTexture = 
                                                            TexturesCards.GetSprite((int)toCard.CardColor - 1, toCard.Effect);
    }

    // Update is called once per frame
    void Update()
    {

        TimerCurrPlayer -= Time.deltaTime;
        if (TimerCurrPlayer <= 0)
        {
            TimerCurrPlayer = 0;
            if(NetWorkingCSharp.ServerTCP.host && state == EPlayModState.PLAY)
            {
                List<PlayerGameData.CardType> list = new List<PlayerGameData.CardType>();
                for (int i = 0; i < 4; i++)
                {
                    list.Add(deck.GetNextCard());
                }
                UnoNetworkingGameData.GameData unoGameData = new UnoNetworkingGameData.GameData();
                unoGameData.PosInHand = 1;
                unoGameData.CardTypePutOnBoard = list.ToArray();
                unoGameData.type = UnoNetworkingGameData.GameData.TypeData.DRAWCARDS;
                NetWorkingCSharp.HeaderGameData gameData = new NetWorkingCSharp.HeaderGameData(
                                                            NetWorkingCSharp.HeaderGameData.EDataType.CARD, unoGameData);

                NetWorkingCSharp.Header header = new NetWorkingCSharp.Header(gameData, NetWorkingCSharp.EType.PLAYERACTION,
                                                                                            new NetWorkingCSharp.ServerTCP.ClientData());
                UpdateActionPlayer(0, header);
                TimerCurrPlayer = PlayTimePlayer;
                GameMgr.SendNetWorkingData(header);
            }


            return;
        }

        //Debug.Log("trun : " + CurrentPlayer);
        NetWorkingCSharp.HeaderGameData headerGame = players[CurrentPlayer].UpdatePlayer(CardOnBoard, state, TimerCurrPlayer);

        if (headerGame.dataType != NetWorkingCSharp.HeaderGameData.EDataType.DEFAULT)
        {
            // Update the board of the player host
            if (NetWorkingCSharp.ServerTCP.host)
            {
                AnalyseGameData(ref headerGame);
                NetWorkingCSharp.Header header = new NetWorkingCSharp.Header(headerGame, NetWorkingCSharp.EType.PLAYERACTION,
                                                                                            new NetWorkingCSharp.ServerTCP.ClientData());
                UpdateActionPlayer(0, header);
                Debug.Log("Host PLay");
                GameMgr.SendNetWorkingData(header);
            }
            // Only the GameData to the server
            else
            {
                NetWorkingCSharp.Header header = new NetWorkingCSharp.Header(headerGame, NetWorkingCSharp.EType.PLAYERACTION,
                                                                                            NetWorkingCSharp.ClientTCP.Tcp.clientData);

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

    public void AnalyseGameData(ref NetWorkingCSharp.HeaderGameData header)
    {
        switch (header.dataType)
        {
            case NetWorkingCSharp.HeaderGameData.EDataType.CARD:
            case NetWorkingCSharp.HeaderGameData.EDataType.CHOOSECOLOR:
                if (state == EPlayModState.PLAY)
                {
                    UnoNetworkingGameData.GameData data = (UnoNetworkingGameData.GameData)header.GameData;
                    switch (data.type)
                    {
                        case UnoNetworkingGameData.GameData.TypeData.DEFAULT:
                            break;
                        case UnoNetworkingGameData.GameData.TypeData.CARDPLAY:
                            AnalysePlayerNumberCards(ref data);
                            AnalyseEffect(ref deck, ref data);
                            break;
                        case UnoNetworkingGameData.GameData.TypeData.DRAWCARDS:
                            ChooseDrawCard(ref data);
                            break;
                    }

                    header.GameData = data;
                }
                else if (state == EPlayModState.WAITCOLOR)
                {
                    AnalyseANYCards(ref header);
                }
                break;
            /*case NetWorkingCSharp.HeaderGameData.EDataType.UNO:
                break;
            case NetWorkingCSharp.HeaderGameData.EDataType.COUNTERUNO:
                break;*/
        }
    }

    void AnalysePlayerNumberCards(ref UnoNetworkingGameData.GameData data)
    {
        if (!players[CurrentPlayer].IsPlayerInUNO())
            return;
        else if (players[CurrentPlayer].IsUno())
            return;

        List <PlayerGameData.CardType> list = new List<PlayerGameData.CardType>(data.CardTypePutOnBoard);
        for (int i = 0; i < 2; i++)
        {
            list.Add(deck.GetNextCard());
        }
        Debug.Log("YOU DIDN'T SAY UNO");
        data.CardTypePutOnBoard = list.ToArray();
    }

    static public void AnalyseEffect(ref DataStruct.Deck deck, ref UnoNetworkingGameData.GameData data)
    {
        PlayerGameData.CardType cardType = data.CardTypePutOnBoard[0];
        // the card is a number no special effect
        if (cardType.Effect < 10)
            return;

        // the card plays is a +2
        if (cardType.Effect == 10)
        {
            List<PlayerGameData.CardType> list = new List<PlayerGameData.CardType>(data.CardTypePutOnBoard);
            
            //list.Add(data.CardTypePutOnBoard[0]);
            for (int i = 0; i < 2; i++)
            {
                list.Add(deck.GetNextCard());
            }

            data.CardTypePutOnBoard = list.ToArray();
        }
    }

    private void AnalyseANYCards(ref NetWorkingCSharp.HeaderGameData header)
    {
        if(CardOnBoard.Effect == PlayerGameData.PLUS_FOUR)
        {
            UnoNetworkingGameData.GameData data = new UnoNetworkingGameData.GameData();
            List<PlayerGameData.CardType> list = new List<PlayerGameData.CardType>();
            for (int i = 0; i < 4; i++)
            {
                list.Add(deck.GetNextCard());
            }

            data.CardTypePutOnBoard = list.ToArray();
            data.PosInHand = (int)header.GameData;
            header.dataType = NetWorkingCSharp.HeaderGameData.EDataType.CARD;
            header.GameData = data;
        }
    }

    public void ChooseDrawCard(ref UnoNetworkingGameData.GameData data)
    {
        data.CardTypePutOnBoard = new PlayerGameData.CardType[] { deck.GetNextCard() };
    }

    public void UpdateActionPlayer(int IdPLayerAction, NetWorkingCSharp.Header header)
    {
        NetWorkingCSharp.HeaderGameData headerGameData = (NetWorkingCSharp.HeaderGameData)header.Data;
        Debug.Log("Update Player");
        int indexLastPlayer = CurrentPlayer;
        switch (headerGameData.dataType)
        {
            case NetWorkingCSharp.HeaderGameData.EDataType.CARD:
            case NetWorkingCSharp.HeaderGameData.EDataType.CHOOSECOLOR:
                if (state == EPlayModState.PLAY)
                {
                    UnoNetworkingGameData.GameData data = (UnoNetworkingGameData.GameData)headerGameData.GameData;
                    switch (data.type)
                    {
                        case UnoNetworkingGameData.GameData.TypeData.DEFAULT:
                            break;
                        case UnoNetworkingGameData.GameData.TypeData.CARDPLAY:
                            PlayerGameData.CardType cardType = players[CurrentPlayer].CardPlay(ref data);
                            CardPlayEvent.Invoke(cardType);
                            deck.AddNewCard(cardType);
                            EffectPlayCard(cardType, data, (System.DateTime.Now.Millisecond - header.HeaderTime) / 1000f);
                            break;
                        case UnoNetworkingGameData.GameData.TypeData.DRAWCARDS:
                            players[CurrentPlayer].DrawCards(data);
                            if (data.PosInHand == 1)
                                CurrentPlayer = GetNextPlayer(CurrentPlayer);
                            TimerCurrPlayer = PlayTimePlayer - (System.DateTime.Now.Millisecond - header.HeaderTime) / 1000f;
                            break;
                    }
                }
                else if (state == EPlayModState.WAITCOLOR)
                {
                    Debug.Log("UpdateColor");

                    if (headerGameData.dataType == NetWorkingCSharp.HeaderGameData.EDataType.CHOOSECOLOR)
                        UpdateANYColor((PlayerGameData.CardType.Color)headerGameData.GameData, PlayerGameData.CHOOSE_COLOR);
                    else
                    {
                        UnoNetworkingGameData.GameData data = (UnoNetworkingGameData.GameData)headerGameData.GameData;
                        UpdateANYColor((PlayerGameData.CardType.Color)data.PosInHand, PlayerGameData.PLUS_FOUR);
                        players[GetNextPlayer(CurrentPlayer)].DrawCards(data);
                        players[CurrentPlayer].ToChooseCard();
                        CurrentPlayer = GetNextPlayer(CurrentPlayer);
                    }
                    CurrentPlayer = GetNextPlayer(CurrentPlayer);

                    TimerCurrPlayer = PlayTimePlayer - (System.DateTime.Now.Millisecond - header.HeaderTime) / 1000f;

                    state = EPlayModState.PLAY;
                }
                players[indexLastPlayer].ToNextPlayer(false, CardOnBoard);
                players[CurrentPlayer].ToNextPlayer(true, CardOnBoard);
                break;
            case NetWorkingCSharp.HeaderGameData.EDataType.UNO:
                players[CurrentPlayer].SetIsUno(true);
                break;
        }

    }

    private void UpdateANYColor(PlayerGameData.CardType.Color color, int anyEffect)
    {
        CardOnBoardObject.name = ((int)color).ToString() + "_" + CardOnBoardObject.name.Split('_')[1];
        CardOnBoard.CardColor = color;
        // TO DO : change the textures to the right texture
        int i = (anyEffect - PlayerGameData.PLUS_FOUR) * 4 + (int)color - 1;
        CardOnBoardObject.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material.mainTexture = TexturesCards.GetANYColorSprite(i);
    }

    private void EffectPlayCard(PlayerGameData.CardType cardType, UnoNetworkingGameData.GameData gameData, float packetTime)
    {
        // the card is a number no special effect
        if (cardType.Effect < 10)
        {
            CurrentPlayer = GetNextPlayer(CurrentPlayer);
            TimerCurrPlayer = PlayTimePlayer - packetTime;
            return;
        }

        // the card plays is a +2
        if(cardType.Effect == 10)
        {
            int nextPlayer = GetNextPlayer(CurrentPlayer);

            players[nextPlayer].DrawCards(gameData);

            CurrentPlayer = GetNextPlayer(nextPlayer);

            TimerCurrPlayer = PlayTimePlayer - packetTime;
            return;
        }
        
        // the card plays is Turn Pass
        if(cardType.Effect == 11)
        {
            CurrentPlayer = GetNextPlayer(GetNextPlayer(CurrentPlayer));
            TimerCurrPlayer = PlayTimePlayer - packetTime;
            // Add anim or something to tell your turn is passed
            return;
        }
        
        // the card plays is Invert Direction 
        if (cardType.Effect == 12)
        {
            DirectionBoard *= -1;
            UpdateDirBoard.Invoke(DirectionBoard);
            CurrentPlayer = GetNextPlayer(CurrentPlayer);
            TimerCurrPlayer = PlayTimePlayer - packetTime;
            // animation tu show that the direction of the board changed
            return;
        }

        if(cardType.Effect >= PlayerGameData.PLUS_FOUR)
        {
            if (state != EPlayModState.WAITCOLOR)
            {
                state = EPlayModState.WAITCOLOR;
                players[CurrentPlayer].ToChooseColor();
            }
            if (NetWorkingCSharp.ServerTCP.ClientsGameData[NetWorkingCSharp.ClientTCP.Tcp.clientData.Id].GetPosOnBoard() == CurrentPlayer)
                Instantiate(PrefabChooseColor, new Vector3(0, 1, 0), players[CurrentPlayer].transform.rotation);
        }

        //CurrentPlayer = GetNextPlayer(CurrentPlayer);
        return;
    }

    public void CreateBoard(UnoNetworkingGameData.GameData[] playerHandData, UnoCardTextures textures, int keyPlayer)
    {
        PrefabCard = textures.GetPrefab();
        TexturesCards = textures;
        List<Vector3> AllPos = CreateAllPos(NetWorkingCSharp.ServerTCP.ClientsGameData.Count);
        for(int i = 0; i < playerHandData.Length - 1; i++)
        {
            UnoNetworkingGameData.GameData data = playerHandData[i];

            Vector3 PosToCenter = (Vector3.zero - AllPos[i]).normalized;
            GameObject Player = Instantiate(PrefabPlayer, AllPos[i], Quaternion.FromToRotation(Vector3.forward, PosToCenter));
            players.Insert(i, Player.GetComponent<UnoPlayer>());

            UnoPlayer.EController conrollerP = UnoPlayer.EController.DEFAULT;

            if (NetWorkingCSharp.ServerTCP.ClientsGameData[keyPlayer].GetPosOnBoard() == i)
            {
                Camera.main.transform.position = Player.transform.Find("PosPlayer").position;
                Camera.main.transform.forward = Vector3.zero - Camera.main.transform.position;
                conrollerP = UnoPlayer.EController.PLAYER;
            }
            else if (NetWorkingCSharp.ServerTCP.ClientsGameData.ContainsKey(data.PosInHand))
            {
                conrollerP = UnoPlayer.EController.ENEMY;
            }
            else
                conrollerP = UnoPlayer.EController.IA;

            Player.GetComponent<UnoPlayer>().InitPlayer(NetWorkingCSharp.ServerTCP.ClientsGameData[data.PosInHand].ClientData.Name, 
                                                        data.CardTypePutOnBoard, textures, conrollerP);

        }

        InitOnBoardCard(playerHandData[playerHandData.Length - 1]);
    }

    private void InitOnBoardCard(UnoNetworkingGameData.GameData gameData)
    {
        CardOnBoard = gameData.CardTypePutOnBoard[0];

        CardOnBoardObject = Instantiate(PrefabCard, this.transform);

        CardOnBoardObject.transform.rotation = Quaternion.Euler(80, 0, 0);
        //CardOnBoardObject.name = ((int)CardOnBoard.CardColor).ToString() + "_" + CardOnBoard.Effect.ToString();
        UpdateOnBoardCard(CardOnBoard);
        EffectFirstCard(CardOnBoard, gameData);
        players[CurrentPlayer].ToNextPlayer(true, CardOnBoard);
        TimerCurrPlayer = PlayTimePlayer;
    }
    private void EffectFirstCard(PlayerGameData.CardType cardType, UnoNetworkingGameData.GameData gameData)
    {
        // the card plays is a +2
        if (cardType.Effect == 10)
        {
            List<PlayerGameData.CardType> cards = new List<PlayerGameData.CardType>(gameData.CardTypePutOnBoard);
            cards.RemoveAt(0);
            gameData.CardTypePutOnBoard = cards.ToArray();
            players[CurrentPlayer].DrawCards(gameData);

            CurrentPlayer = GetNextPlayer(CurrentPlayer);
        }

        // the card plays is Turn Pass
        if (cardType.Effect == 11)
        {
            CurrentPlayer = GetNextPlayer(CurrentPlayer);
            // Add anim or something to tell your turn is passed
            return;
        }

        // the card plays is Invert Direction 
        if (cardType.Effect == 12)
        {
            DirectionBoard *= -1;
            UpdateDirBoard.Invoke(DirectionBoard);
            CurrentPlayer = GetNextPlayer(CurrentPlayer);
            // animation tu show that the direction of the board changed
            return;
        }
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
