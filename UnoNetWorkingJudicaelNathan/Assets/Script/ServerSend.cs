using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using ProtoBuf;
using UnityEngine;

namespace NetWorkingCSharp
{
    class ServerSend
    {
        [ProtoContract]
        public class WelcomeToServer
        {
            [ProtoMember(1)]
            public string msg;
            [ProtoMember(2)]
            public int Id;
        }

        private static void SendTCPData(int toClient, byte[] arraySerialized)
        {
            Server.Clients[toClient].tcp.SendData(arraySerialized);
        }

        public static void SendTCPDataToAll(byte[] arraySerialized)
        {
            for(int i = 1; i <= Server.MaxPlayers; i++)
            {
                Server.Clients[i].tcp.SendData(arraySerialized);
            }
        }

        public static void SendTCPDataToAllExept(int clientExeption, Header headerToSend)
        {
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                if (clientExeption != i && Server.Clients[i].tcp.connected)
                    Header.SendHeader(Server.Clients[i].tcp.stream, headerToSend);
                    //Serializer.SerializeWithLengthPrefix<Header>(Server.Clients[i].tcp.stream, headerToSend, PrefixStyle.Fixed32);
            }
        }

        public static void Welcome(int toClient, string msg)
        {
            WelcomeToServer welcome = new WelcomeToServer();
            welcome.msg = msg;
            welcome.Id = toClient;
            Header H = new Header(welcome, EType.FUCK, toClient);
            H.TypeData = EType.FUCK;
            Header.SendHeader(Server.Clients[toClient].tcp.stream, H);

            //Serializer.SerializeWithLengthPrefix<Header>(Server.Clients[toClient].tcp.stream, H, PrefixStyle.Fixed32);
        }

    }
}
