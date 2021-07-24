using System.Collections;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class PlayerGameData
{
    public static int CARD_WITH_COLOR { get; } = 12;
    public static int NB_COLOR { get; } = 4;
    public static int PLUS_FOUR = 13;
    public static int CHOOSE_COLOR = 14;

    [ProtoContract]
    public struct CardType
    {
        public enum Color
        {
            DEFAULT = 0,
            RED,
            BLUE,
            GREEN,
            YELLOW,
            ANY
        }
        [ProtoMember(1)]
        public int Effect;
        [ProtoMember(2)]
        public Color CardColor;

        public CardType(Color color, int effect)
        {
            Effect = effect;
            CardColor = color;
        }
    }

    [ProtoContract]
    public struct GameData
    {
        public int NumberOfCard;
        public List<List<int>> CardsInHand;
        public int PosOnBoard;

        public GameData(int numberOfCard = 5)
        {
            NumberOfCard = numberOfCard;
            CardsInHand = new List<List<int>>();
            PosOnBoard = 0;
        }
    }

    GameData DataUnoPlayer;
    public NetWorkingCSharp.ServerTCP.ClientData ClientData;

    static public PlayerGameData CreateUnoGameData(NetWorkingCSharp.ServerTCP.ClientData client)
    {
        return new PlayerGameData(new GameData(5), client);
    }

    public PlayerGameData(GameData data, NetWorkingCSharp.ServerTCP.ClientData client)
    {
        DataUnoPlayer = data;
        ClientData = client;
    }

    // function only use by the server to check if the data send by the player are the same as the server
    public bool IsDataOk(GameData data)
    {
        if (data.NumberOfCard != DataUnoPlayer.NumberOfCard)
            return false;

        return true;
    }

    public void Updatename(string name)
    {
        ClientData.Name = name;
    }

    public void SetIsReady(bool ready)
    {
        ClientData.IsReady = ready;
    }

    public void SetPosOnBoard(int pos)
    {
        DataUnoPlayer.PosOnBoard = pos;
    }

    public int GetPosOnBoard()
    {
        return DataUnoPlayer.PosOnBoard;
    }

    public void SetStartGameData(CardType[] dataStart, int posOnBoard)
    {
        /*foreach (CardType cardType in dataStart)
        {
            DataUnoPlayer.CardsInHand[(int)cardType.CardColor].Add(cardType.Effect);
        }*/

        DataUnoPlayer.PosOnBoard = posOnBoard;
    }

    /*public void DrawCards(UnoNetworkingGameData.GameData data)
    {
        foreach(CardType cardType in data.CardTypePutOnBoard)
        {
            DataUnoPlayer.CardsInHand[(int)cardType.CardColor].Add(cardType.Effect);
        }
    }

    // Delete the card play by the current player
    public CardType CardPlay(UnoNetworkingGameData.GameData data)
    {
        if (data.CardTypePutOnBoard.Count > 1)
        {
            CardType cardType = data.CardTypePutOnBoard[0];
            DataUnoPlayer.CardsInHand[(int)cardType.CardColor].RemoveAt(data.PosInHand);
            data.CardTypePutOnBoard.RemoveAt(0);
            return cardType;
        }
        return new CardType(CardType.Color.DEFAULT, 0);
    }*/
}

public class UnoNetworkingGameData
{
    [ProtoContract]
    public struct GameData
    {
        public enum TypeData
        {
            DEFAULT,
            DRAWCARDS,
            CARDPLAY
        }

        [ProtoMember(1)]
        public PlayerGameData.CardType[] CardTypePutOnBoard;
        [ProtoMember(2)]
        // became in the BeginPlay state the ID of the Client
        public int PosInHand;
        [ProtoMember(3)]
        // the type of data send When Card is play
        public TypeData type;

        public GameData(int numberOfCard = 5)
        {
            CardTypePutOnBoard = new PlayerGameData.CardType[1];
            PosInHand = 0;
            type = TypeData.DEFAULT;
        }
    }

    //[ProtoMember(1)]
    public NetWorkingCSharp.LocalClientDataGame<GameData> DataUnoPlayer;
}


// only for the test NEED TO BE DELETED OR UPDATE IN THE FUTURE
[ProtoContract]
public struct GameDataArray
{
    [ProtoMember(1)]
    public UnoNetworkingGameData.GameData[] gameDatas;
}
