using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;
using UnityEngine;

namespace NetWorkingCSharp
{
    public enum EType : Int32 
    {
        Error = 0,
        WELCOME,
        MSG,
        UPDATENAME,
        BEGINPLAY,
        PLAYERREADY,
        DISCONNECT,
        PLAYERACTION
    }

    [ProtoContract]
    public class Header
    {
        [ProtoMember(1)]
        public ServerTCP.ClientData clientData;
        [ProtoMember(2)]
        public EType TypeData;
        [ProtoMember(3)]
        public int HeaderTime;

        public object Data;

        public static void SendHeader(System.Net.Sockets.NetworkStream stream, Header header)
        {
            header.HeaderTime = System.DateTime.Now.Millisecond;
            Serializer.SerializeWithLengthPrefix<Header>(stream, header, PrefixStyle.Fixed32);

            switch (header.TypeData)
            {
                case EType.Error:
                    break;
                case EType.WELCOME:
                    Serializer.SerializeWithLengthPrefix<ServerSend.WelcomeToServer>(stream, (ServerSend.WelcomeToServer)header.Data, PrefixStyle.Fixed32);
                    break;
                case EType.MSG:
                    Serializer.SerializeWithLengthPrefix<string>(stream, (string)header.Data, PrefixStyle.Fixed32);
                    break;
                case EType.UPDATENAME:
                    Serializer.SerializeWithLengthPrefix<string>(stream, (string)header.Data, PrefixStyle.Fixed32);
                    break;
                case EType.BEGINPLAY:
                    //Serializer.
                    GameDataArray arr = new GameDataArray();
                    arr.gameDatas = (UnoNetworkingGameData.GameData[])header.Data;
                    Serializer.SerializeWithLengthPrefix(stream, arr, PrefixStyle.Fixed32);
                    //Debug.LogError("Server header begin Play");
                    break;
                case EType.PLAYERACTION:
                    //Serializer.SerializeWithLengthPrefix(stream, (UnoNetworkingGameData.GameData)header.Data, PrefixStyle.Fixed32);
                    ((HeaderGameData)header.Data).SendData(stream);
                    break;
            }
        }

        public Header(object data, EType type, ServerTCP.ClientData id)
        {
            Data = data;
            TypeData = type;
            clientData = id;
        }

        // default constructor for the serialisation
        public Header()
        {

        }
    }
}
