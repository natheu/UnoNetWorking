using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;
using UnityEngine;

namespace NetWorkingCSharp
{
    public enum EType : byte 
    {
        Error = 0,
        FUCK,
        MSG
    }

    [ProtoContract]
    class Header
    {
        [ProtoMember(1)]
        public int SerializeMessageId;
        [ProtoMember(2)]
        public EType TypeData;
        [ProtoMember(3)]
        public string Name = "";

        public object Data;

        //private static int _HeaderSerialId = 0;

        public static void SendHeader(System.Net.Sockets.NetworkStream stream, Header header)
        {
            Serializer.SerializeWithLengthPrefix<Header>(stream, header, PrefixStyle.Fixed32);

            switch (header.TypeData)
            {
                case EType.Error:
                    break;
                case EType.FUCK:
                    Serializer.SerializeWithLengthPrefix<ServerSend.WelcomeToServer>(stream, (ServerSend.WelcomeToServer)header.Data, PrefixStyle.Fixed32);
                    break;
                case EType.MSG:
                    Serializer.SerializeWithLengthPrefix<string>(stream, (string)header.Data, PrefixStyle.Fixed32);
                    break;
            }
        }

        public Header(object data, EType type, int id)
        {
            Data = data;
            TypeData = type;
            SerializeMessageId = id;
        }

        // default constructor for the serialisation
        public Header()
        {

        }
    }
}
