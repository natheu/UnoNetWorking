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
    class ClientTest
    {
        public string Ip = "127.0.0.1";
        public int port = 50150;

        public static ClientTest testInst; 

        public int Id = 0;
        public TCP Tcp;
        public bool CreateClient(string Ip)
        {
            testInst = this;
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

            public readonly int Id;
            public NetworkStream stream;
            public byte[] receiveBuffer;
            public bool connected = false;

            public bool Connect(string Ip)
            {
                Socket = new TcpClient
                {
                    ReceiveBufferSize = Client.DataBufferSize,
                    SendBufferSize  = Client.DataBufferSize
                };

                receiveBuffer = new byte[Client.DataBufferSize];
                Debug.Log("TryConnect");
                //Socket.BeginConnect(Ip, testInst.port, ConnectCallback, Socket);
                Socket.Connect(Ip, testInst.port);

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
                        Header test = Serializer.DeserializeWithLengthPrefix<Header>(stream, PrefixStyle.Fixed32);

                        if (test == null)
                            break;

                        // don't forget Set ID

                        switch (test.TypeData)
                        {
                            case EType.Error:
                                break;
                            case EType.FUCK:
                                ServerSend.WelcomeToServer ff = Serializer.DeserializeWithLengthPrefix<ServerSend.WelcomeToServer>(stream, PrefixStyle.Fixed32);
                                Debug.Log(ff.msg);
                                test.Data = ff;
                                break;
                            case EType.MSG:
                                string msg = Serializer.DeserializeWithLengthPrefix<string>(stream, PrefixStyle.Fixed32);
                                Debug.Log(msg);
                                break;
                        }
                    }
                    catch
                    {
                        //TODO : disconnect
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
                    //Serializer.SerializeWithLengthPrefix<Header>(stream, header, PrefixStyle.Fixed32);
                }
            }
        }
    }
}
