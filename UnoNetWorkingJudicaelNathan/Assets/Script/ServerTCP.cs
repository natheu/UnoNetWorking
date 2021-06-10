﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using ProtoBuf;
using UnityEngine;

namespace NetWorkingCSharp
{
    public class ServerTCP
    {
        [ProtoContract]
        public struct ClientData
        {
            [ProtoMember(1)]
            public int Id;
            [ProtoMember(2)]
            public string Name;
        }

        public class ClientServ
        {
            public static int DataBufferSize = 4096;

            public TcpClient Socket;
            public NetworkStream stream;
            public bool connected = false;
            public ClientData clientData;

            public ClientServ(int ID)
            {
                clientData.Id = ID;
            }

            public void Connect(TcpClient socket)
            {
                Socket = socket;
                socket.ReceiveBufferSize = DataBufferSize;
                socket.SendBufferSize = DataBufferSize;

                stream = socket.GetStream();

                clientData.Name = "Client" + clientData.Id;
                Debug.Log(clientData.Id);

                connected = true;

                List<ServerTCP.ClientData> clients = new List<ClientData>();
                foreach(KeyValuePair<int, ClientServ> p in Clients)
                {
                    Debug.Log("Yooo");
                    clients.Add(p.Value.clientData);
                }

                ServerSend.Welcome(socket, clientData, "Welcome to the Server", clients);
            }

            public void Disconnect()
            {
                Socket.Close();
                stream = null;
                connected = false;
            }

            public void SendData(byte[] arraySerialized)
            {
                try
                {
                    if (Socket != null)
                        stream.BeginWrite(arraySerialized, 0, arraySerialized.Length, null, null);
                }
                catch (Exception ex)
                {
                    Debug.Log($"Error sending data to player {clientData.Id} via TCP {ex}");
                }
            }
        }

        public static int MaxPlayers { get; private set; }
        public static int Port { get; private set; }
        public static bool host { get; private set; }

        public static Dictionary<int, ClientServ> Clients = new Dictionary<int, ClientServ>();

        public enum EStateGame
        {
            DEFAULT = 0,
            NEWCONNECTION,
            RUNNING,
            START,
            FINISH,
            CLOSED

        }

        public static EStateGame stateGame { get; private set; }

        // Thread signal.  
        public static ManualResetEvent allDone = new ManualResetEvent(false);

        private static TcpListener _TcpListener;
        //private static Socket _TcpListener;
        public static void CreateServer(int maxPlayer, int port, bool isHost)
        {
            MaxPlayers = maxPlayer;
            Port = port;

            Debug.Log("Starting server ...");
            // Init the server client
            //InitializeServerData(isHost);
            if (isHost)
                Clients.Add(0, new ClientServ(0));
            host = isHost;
            _TcpListener = new TcpListener(IPAddress.Any, Port);
            _TcpListener.Start();
            _TcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPAcceptCallback), null);

            stateGame = EStateGame.START;

            Thread loopRead = new Thread(new ThreadStart(ReceiveCallback));
            loopRead.IsBackground = true;
            loopRead.Start();

            Debug.Log($"The Server Start on {Port} ...");
        }

        public static void TCPAcceptCallback(IAsyncResult ar)
        {
            TcpClient client = _TcpListener.EndAcceptTcpClient(ar);
            _TcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPAcceptCallback), null);


            if(Clients.Count < MaxPlayers)
            {
                Debug.Log($"Incoming connection from {client.Client.RemoteEndPoint}...");
                ClientServ newClient = new ClientServ(Clients.Count);
                newClient.Connect(client);
                Clients.Add(Clients.Count, newClient);
            }
        }

        public static void CloseListener()
        {
            _TcpListener.Stop();
        }

        private static void InitializeServerData(bool isHost)
        {
            int nbPlayers = MaxPlayers;
            for (int i = 1; i <= MaxPlayers; i++)
            {
                Clients.Add(i, new ClientServ(i));
            }
        }

        private static void ReceiveCallback()
        {
            while (stateGame != EStateGame.CLOSED)
            {
                for (int i = 0; i < Clients.Count; i++)
                { 
                    //ClientServ currClient = pair.Value;
                    ClientServ currClient = Clients[i];
                    if (currClient.connected)
                    {
                        try
                        {
                            Header header = Serializer.DeserializeWithLengthPrefix<Header>(currClient.stream, PrefixStyle.Fixed32);

                            if (header == null)
                                currClient.connected = false; 

                            switch (header.TypeData)
                            {
                                case EType.Error:
                                    break;
                                case EType.WELCOME:
                                    ServerSend.WelcomeToServer ff = Serializer.DeserializeWithLengthPrefix<ServerSend.WelcomeToServer>(currClient.stream, PrefixStyle.Fixed32);
                                    header.Data = ff;
                                    break;
                                case EType.MSG:
                                    string msg = Serializer.DeserializeWithLengthPrefix<string>(currClient.stream, PrefixStyle.Fixed32);
                                    header.Data = msg;
                                    Debug.Log(msg);
                                    break;
                                case EType.DISCONNECT:
                                    currClient.Disconnect();
                                    break;
                            }

                            header.clientData = currClient.clientData;
                            ServerSend.SendTCPDataToAllExept(currClient.clientData.Id, header);
                        }
                        catch(SocketException ex)
                        {
                            //TODO : disconnect
                            Debug.Log(ex.Message);
                            Debug.Log(currClient.clientData.Id);
                            currClient.connected = false;
                        }
                    }
                }
            }
        }
    }
}
