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
            public List<ServerTCP.ClientData> clientsData = new List<ServerTCP.ClientData>();

        }

        private static void SendTCPData(int toClient, Header headerToSend)
        {
            if (ServerTCP.Clients[toClient].connected)
                Header.SendHeader(ServerTCP.Clients[toClient].stream, headerToSend);
        }

        public static void SendTCPDataToAll(Header headerToSend)
        {
            foreach (KeyValuePair<int, ServerTCP.ClientServ> client in ServerTCP.Clients)
            {
                if (client.Value.connected)
                    Header.SendHeader(client.Value.stream, headerToSend);
            }
            /*
            for (int i = 0; i < ServerTCP.Clients.Count; i++)
            {
                if (ServerTCP.Clients[i].connected)
                {
                    Header.SendHeader(ServerTCP.Clients[i].stream, headerToSend);
                }
            }
            */
        }

        public static void SendTCPDataToAllExept(int clientExeption, Header headerToSend)
        {
            Debug.Log("Exeption : " + clientExeption);
            foreach(KeyValuePair<int, ServerTCP.ClientServ> client in ServerTCP.Clients)
            {
                if (clientExeption != client.Value.clientData.Id && client.Value.connected)
                    Header.SendHeader(client.Value.stream, headerToSend);
            }
            /*
            for (int i = 0; i < ServerTCP.Clients.Count; i++)
            {
                if (clientExeption != i && ServerTCP.Clients[i].connected)
                    Header.SendHeader(ServerTCP.Clients[i].stream, headerToSend);
            }
            */
        }

        public static void Welcome(TcpClient newClient, ServerTCP.ClientData toClient, string msg, List<ServerTCP.ClientData> clients)
        {
            WelcomeToServer welcome = new WelcomeToServer();
            welcome.msg = msg;
            welcome.clientsData = clients;
            Header H = new Header(welcome, EType.WELCOME, toClient);
            //H.TypeData = EType.WELCOME;

            Header.SendHeader(newClient.GetStream(), H);

            welcome.clientsData.Clear();
            H.Data = welcome;
            SendTCPDataToAllExept(toClient.Id, H);
        }

    }
}
