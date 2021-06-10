using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using ProtoBuf;
using UnityEngine;

namespace NetWorkingCSharp
{
    class Server
    {
        public static int MaxPlayers { get; private set; }
        public static int Port { get; private set; }

        public static Dictionary<int, Client> Clients = new Dictionary<int, Client>();

        public enum EStateGame
        {
            DEFAULT = 0,
            NEWCONNECTION,
            RUNNING,
            START,
            FINISH

        }

        public static EStateGame stateGame { get; private set; }

    // Thread signal.  
    public static ManualResetEvent allDone = new ManualResetEvent(false);

        private static TcpListener _TcpListener;
        //private static Socket _TcpListener;
        public static void CreateServer(int maxPlayer, int port)
        {
            MaxPlayers = maxPlayer;
            Port = port;

            Debug.Log("Starting server ...");
            InitializeServerData();

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

            Debug.Log($"Incoming connection from {client.Client.RemoteEndPoint}...");

            for (int i = 1; i <= MaxPlayers; i++)
            {
                if(Clients[i].tcp.Socket == null)
                {
                    Clients[i].tcp.Connect(client);
                    return;
                }
            }
        }

        public static void CloseListener()
        {
            _TcpListener.Stop();
        }

        private static void InitializeServerData()
        {
            for(int i = 1; i <= MaxPlayers; i++)
            {
                Clients.Add(i, new Client(i));
            }
        }

        private static void ReceiveCallback()
        {
            while (stateGame != EStateGame.FINISH)
            {
                for (int i = 1; i <= Clients.Count; i++)
                {
                    Client.TCP currClient = Clients[i].tcp;
                    if (currClient.connected)
                    {
                        try
                        {
                            Header test = Serializer.DeserializeWithLengthPrefix<Header>(currClient.stream, PrefixStyle.Fixed32);

                            if (test == null)
                                currClient.connected = false; 

                            switch (test.TypeData)
                            {
                                case EType.Error:
                                    break;
                                case EType.FUCK:
                                    ServerSend.WelcomeToServer ff = Serializer.DeserializeWithLengthPrefix<ServerSend.WelcomeToServer>(currClient.stream, PrefixStyle.Fixed32);
                                    //Debug.Log(ff.msg);
                                    test.Data = ff;
                                    break;
                                case EType.MSG:
                                    string msg = Serializer.DeserializeWithLengthPrefix<string>(currClient.stream, PrefixStyle.Fixed32);
                                    Debug.Log(msg);
                                    break;
                            }

                            test.SerializeMessageId = currClient.Id;
                            ServerSend.SendTCPDataToAllExept(currClient.Id, test);
                        }
                        catch(SocketException ex)
                        {
                            //TODO : disconnect
                            Debug.Log(ex.Message);
                            Debug.Log(currClient.Id);
                            currClient.connected = false;
                        }
                    }
                }
            }
        }
    }
}
