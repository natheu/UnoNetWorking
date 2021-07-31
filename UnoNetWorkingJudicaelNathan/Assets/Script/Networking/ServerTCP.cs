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
    public class ServerTCP
    {
        [ProtoContract]
        public struct ClientData
        {
            [ProtoMember(1)]
            public int Id;
            [ProtoMember(2)]
            public string Name;
            [ProtoMember(3)]
            public bool IsReady;
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
                clientData.Name = "Client" + ID;
            }

            public void Connect(TcpClient socket)
            {
                Socket = socket;
                socket.ReceiveBufferSize = DataBufferSize;
                socket.SendBufferSize = DataBufferSize;

                stream = socket.GetStream();

                clientData.Name = "Client" + clientData.Id;
                clientData.IsReady = false;

                connected = true;

                List<ServerTCP.ClientData> clients = new List<ClientData>();
                foreach(KeyValuePair<int, ClientServ> p in Clients)
                {
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
        //public static ClientTCP ListenClient { get; private set; }

        public static Dictionary<int, ClientServ> Clients = new Dictionary<int, ClientServ>();
        public static Dictionary<int, PlayerGameData> ClientsGameData = new Dictionary<int, PlayerGameData>();

        public static Mutex mutexClient = new Mutex();
        private static int IdClient = 1;

        public enum EStateGame
        {
            DEFAULT = 0,
            NEWCONNECTION,
            LOBBY,
            RUNNING,
            START,
            STARTNEW,
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
            {
                Clients.Add(0, new ClientServ(0));
                ClientsGameData.Add(0, new PlayerGameData(new PlayerGameData.GameData(5), Clients[0].clientData));

                ClientTCP.Tcp = new ClientTCP.TCP();
                //ListenClient = new ClientTCP();
                //ListenClient.Tcp = new ClientTCP.TCP();
            }
            host = isHost;
            _TcpListener = new TcpListener(IPAddress.Any, Port);
            _TcpListener.Start();
            _TcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPAcceptCallback), null);

            stateGame = EStateGame.START;

            /*Thread loopRead = new Thread(new ThreadStart(ReceiveCallback));
            loopRead.IsBackground = true;
            loopRead.Start();*/

            Debug.Log($"The Server Start on {Port} ...");
        }

        public static void TCPAcceptCallback(IAsyncResult ar)
        {
            TcpClient client = _TcpListener.EndAcceptTcpClient(ar);
            _TcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPAcceptCallback), null);

            mutexClient.WaitOne();
            if (Clients.Count < MaxPlayers)
            {
                Debug.Log($"Incoming connection from {client.Client.RemoteEndPoint}...");
                ClientServ newClient = new ClientServ(Clients.Count);
                newClient.Connect(client);
                Thread loopRead = new Thread(new ParameterizedThreadStart(ReceiveCallback));
                loopRead.IsBackground = true;
                loopRead.Start(IdClient);
                Clients.Add(IdClient, newClient);
                ClientsGameData.Add(IdClient, new PlayerGameData(new PlayerGameData.GameData(5), newClient.clientData));
                IdClient++;
            }
            mutexClient.ReleaseMutex();
        }

        public static void CloseListener()
        {
            ServerTCP.mutexClient.Dispose();
            _TcpListener.Stop();
            _TcpListener = null;
            stateGame = EStateGame.CLOSED;
            Clients.Clear();
            // Need more work on the Destroy of the TCP Server
            //ServerSend.SendTCPDataToAll(null, EType.DISCONNECT,)
        }

        private static void InitializeServerData(bool isHost)
        {
            int nbPlayers = MaxPlayers;
            for (int i = 1; i <= MaxPlayers; i++)
            {
                Clients.Add(i, new ClientServ(i));
            }
        }
        private static void ReceiveCallback(object keyClient)
        {
            ClientServ currClient = Clients[(int)keyClient];
            while (stateGame != EStateGame.CLOSED && currClient.connected)
            {
                /*for (int i = 0; i < Clients.Count; i++)
                {
                    //ClientServ currClient = pair.Value;
                    if (!Clients.ContainsKey(i))
                        continue;
                    ClientServ currClient = Clients[i];*/
                if (currClient.connected)
                {
                    try
                    {
                        Header header = Serializer.DeserializeWithLengthPrefix<Header>(currClient.stream, PrefixStyle.Fixed32);

                        if (header == null)
                        {
                            Debug.Log("Header null : " + currClient.clientData.Id);
                            currClient.connected = false;
                            continue;
                        }

                        object data = null;
                        bool ToSend = true;
                        switch (header.TypeData)
                        {
                            case EType.Error:
                                break;
                            case EType.WELCOME:
                                data = Serializer.DeserializeWithLengthPrefix<ServerSend.WelcomeToServer>(currClient.stream, PrefixStyle.Fixed32);
                                break;
                            case EType.MSG:
                                data = Serializer.DeserializeWithLengthPrefix<string>(currClient.stream, PrefixStyle.Fixed32);
                                Debug.Log((string)data);
                                break;
                            case EType.UPDATENAME:
                                currClient.clientData.Name = Serializer.DeserializeWithLengthPrefix<string>(currClient.stream, PrefixStyle.Fixed32);
                                Debug.Log("Update Name :" + currClient.clientData.Name);
                                break;
                            case EType.PLAYERREADY:
                                Debug.Log("Ready ?");
                                currClient.clientData.IsReady = !currClient.clientData.IsReady;
                                break;
                            case EType.PLAYERACTION:
                                data  = Serializer.DeserializeWithLengthPrefix<UnoNetworkingGameData.GameData>(currClient.stream, PrefixStyle.Fixed32);
                                ToSend = false;
                                break;
                            case EType.DISCONNECT:
                                DisconnectClient(currClient);
                                break;
                        }

                        header.clientData = currClient.clientData;
                        header.Data = data;
                        ClientTCP.Tcp.headersReciev.Enqueue(header);
                        if(ToSend)
                            ServerSend.SendTCPDataToAllExept(currClient.clientData.Id, header);
                    }
                    catch(SocketException ex)
                    {
                        //TODO : disconnect
                        Debug.Log(ex.Message);
                        Debug.Log(currClient.clientData.Id);
                        DisconnectClient(currClient);
                    }
                }
                //}
            }
        }

        private static void  DisconnectClient(ClientServ clientToDisconnect)
        {
            //int i = clientToDisconnect.clientData.Id + 1;
            ServerSend.SendTCPDataToAllExept(clientToDisconnect.clientData.Id, new Header(null, EType.DISCONNECT, clientToDisconnect.clientData));
            clientToDisconnect.Disconnect();
            Clients.Remove(clientToDisconnect.clientData.Id);
            ClientsGameData.Remove(clientToDisconnect.clientData.Id);
            /*for(; i < Clients.Count; i++)
            {
                Clients.
            }*/
        }

        public static bool CanStartAGame()
        {
            if (ClientsGameData.Count <= 1)
            {
                Debug.LogError("Don't start a Game alone It's sad");
                return false;
            }
            foreach(KeyValuePair<int, PlayerGameData> client in ClientsGameData)
            {
                if(Clients[client.Value.ClientData.Id].connected)
                {
                    if (!client.Value.ClientData.IsReady)
                    {
                        Debug.LogError("Player : " + client.Value.ClientData.Name + " isn't Ready");
                        return false;
                    }
                }
            }
            return true;
        }

        public static void GameRunning()
        {
            stateGame = EStateGame.RUNNING;
        }

        public static void SetState(EStateGame state)
        {
            stateGame = state;
        }
    }
}
