using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ProtoBuf;

namespace NetWorkingCSharp
{
    class ClientTCP
    {
        static public string Ip = "127.0.0.1";
        static public int port = 50150;

        public TCP Tcp;
        public bool CreateClient(string Ip)
        {
            Tcp = new TCP();
            return Tcp.Connect(Ip);
        }

        public void SendToServer(Header header)
        {
            Tcp.SendToServer(header);
        }

        public void Disconnect()
        {
            Tcp.Disconnect();
        }

        public class TCP
        {
            public TcpClient Socket;

            public ServerTCP.ClientData clientData;
            public NetworkStream stream;
            public Queue<Header> headersReciev = new Queue<Header>();
            public bool connected = false;

            public bool Connect(string Ip)
            {
                Socket = new TcpClient
                {
                    ReceiveBufferSize = ServerTCP.ClientServ.DataBufferSize,
                    SendBufferSize  = ServerTCP.ClientServ.DataBufferSize
                };

                Debug.Log("TryConnect");
                //Socket.BeginConnect(Ip, testInst.port, ConnectCallback, Socket);
                Socket.Connect(Ip, ClientTCP.port);

                if (!Socket.Connected)
                    return false;

                connected = true;
                stream = Socket.GetStream();

                Thread loopRead = new Thread(new ThreadStart(ReceiveCallback));
                loopRead.IsBackground = true;
                loopRead.Start();

                return true;
            }

            private void ConnectCallback(IAsyncResult _result)
            {
                Socket.EndConnect(_result);

                if (!Socket.Connected)
                {
                    return;
                }

                connected = true;
                stream = Socket.GetStream();

                Thread loopRead = new Thread(new ThreadStart(ReceiveCallback));
                loopRead.IsBackground = true;
                loopRead.Start();
            }

            private void ReceiveCallback()
            {
                while(connected)
                {
                    try
                    {
                        Header header = Serializer.DeserializeWithLengthPrefix<Header>(stream, PrefixStyle.Fixed32);

                        if (header == null)
                            break;

                        // don't forget Set ID
                        object data = null;

                        switch (header.TypeData)
                        {
                            case EType.Error:
                                break;
                            case EType.WELCOME:
                                data = Serializer.DeserializeWithLengthPrefix<ServerSend.WelcomeToServer>(stream, PrefixStyle.Fixed32);
                                if (clientData.Equals(default(ServerTCP.ClientData)))
                                    clientData = header.clientData;
                                break;
                            case EType.MSG:
                                data = Serializer.DeserializeWithLengthPrefix<string>(stream, PrefixStyle.Fixed32);
                                break;
                            case EType.UPDATENAME:
                                data = Serializer.DeserializeWithLengthPrefix<string>(stream, PrefixStyle.Fixed32);
                                break;
                            case EType.PLAYERREADY:
                                break;
                        }

                        header.Data = data;
                        headersReciev.Enqueue(header);
                    }
                    catch
                    {
                        //TODO : disconnect
                        Disconnect();
                        break;
                    }
                }

                connected = false;
                Debug.Log("Client ShutDown");
            }

            public void Disconnect()
            {
                Socket.Close();
            }

            public void SendToServer(Header header)
            {
                if (connected)
                {
                    Header.SendHeader(stream, header);
                }
            }
        }
    }
}
