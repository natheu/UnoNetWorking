using System.Collections;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class UnoNetworkingGameData
{
    [ProtoContract]
    public struct CardType
    {
        public enum Color
        {
            DEFAULT = 0,
            ANY,
            RED,
            BLUE,
            GREEN,
            YELLOW
        }
        [ProtoMember(1)]
        public int Effect;
        [ProtoMember(2)]
        public Color CardColor;

        public CardType(CardType.Color color, int effect)
        {
            Effect = effect;
            CardColor = color;
        }
    }

    [ProtoContract]
    public struct UnoGameData
    {
        [ProtoMember(1)]
        public int NumberOfCard;
        [ProtoMember(2)]
        public List<CardType> CardTypePutOnBoard;
        [ProtoMember(3)]
        public int PosOnBoard;

        public UnoGameData(int numberOfCard = 5)
        {
            NumberOfCard = numberOfCard;
            CardTypePutOnBoard = new List<CardType>();
            PosOnBoard = 0;
        }
    }

    [ProtoMember(1)]
    NetWorkingCSharp.LocalClientDataGame<UnoGameData> DataUnoPlayer;

    static public UnoNetworkingGameData CreateUnoGameData(NetWorkingCSharp.ServerTCP.ClientData client)
    {
        return new UnoNetworkingGameData(new UnoGameData(5), client);
    }

    public UnoNetworkingGameData(UnoGameData data, NetWorkingCSharp.ServerTCP.ClientData client)
    {
        DataUnoPlayer = new NetWorkingCSharp.LocalClientDataGame<UnoGameData>(client, data);
    }

    // function only use by the server to check if the data send by the player are the same as the server
    public bool IsDataOk(UnoGameData data)
    {
        if (data.NumberOfCard != DataUnoPlayer.GameData.NumberOfCard)
            return false;

        return true;
    }

    public void Updatename(string name)
    {
        DataUnoPlayer.ClientData.Name = name;
    }

    public void SetIsReady(bool ready)
    {
        DataUnoPlayer.ClientData.IsReady = ready;
    }

    public void SetPosition(int pos)
    {
        DataUnoPlayer.GameData.PosOnBoard = pos;
    }

    public int GetPosition()
    {
        return DataUnoPlayer.GameData.PosOnBoard;
    }
}
