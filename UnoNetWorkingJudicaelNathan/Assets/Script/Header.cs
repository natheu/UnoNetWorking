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
        DISCONNECT
    }

    [ProtoContract]
    class Header
    {
        [ProtoMember(1)]
        public ServerTCP.ClientData clientData;
        [ProtoMember(2)]
        public EType TypeData;

        public object Data;

        public static void SendHeader(System.Net.Sockets.NetworkStream stream, Header header)
        {
            Serializer.SerializeWithLengthPrefix(stream, header, PrefixStyle.Fixed32);

            switch (header.TypeData)
            {
                case EType.Error:
                    break;
                case EType.WELCOME:
                    Serializer.SerializeWithLengthPrefix<ServerSend.WelcomeToServer>(stream, (ServerSend.WelcomeToServer)header.Data, PrefixStyle.Fixed32);
                    break;
                case EType.MSG:
                case EType.UPDATENAME:
                    Debug.Log("Maybe Name");
                    Serializer.SerializeWithLengthPrefix<string>(stream, (string)header.Data, PrefixStyle.Fixed32);
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
