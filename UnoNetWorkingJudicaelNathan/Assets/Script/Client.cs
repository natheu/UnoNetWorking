using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ProtoBuf;
using UnityEngine;

namespace NetWorkingCSharp
{
    class Client
    {
        public static int DataBufferSize = 4096;

        public int Id;
        public TCP tcp;
        public Client(int clientId)
        {
            Id = clientId;
            tcp = new TCP(Id); 
        }

        public class TCP
        {
            public TcpClient Socket;

            public int Id;
            public NetworkStream stream;
            public bool connected = false;

            public TCP(int id)
            {
                Id = id;
            }

            public void Connect(TcpClient socket)
            {
                Socket = socket;
                socket.ReceiveBufferSize = DataBufferSize;
                socket.SendBufferSize = DataBufferSize;

                stream = socket.GetStream();

                connected = true;

                /*Thread loopRead = new Thread(new ThreadStart(ReceiveCallback));
                loopRead.IsBackground = true;
                loopRead.Start();*/
                ServerSend.Welcome(Id, "Welcome to the Server");
            }

            public void SendData(byte[] arraySerialized)
            {
                try
                {
                    if(Socket != null)
                        stream.BeginWrite(arraySerialized, 0, arraySerialized.Length, null, null);
                }
                catch(Exception ex)
                {
                    Debug.Log($"Error sending data to player {Id} via TCP {ex}");
                }
            }

            private void ReceiveCallback()
            {
                while (connected)
                {
                    try
                    {
                        Header test = Serializer.DeserializeWithLengthPrefix<Header>(stream, PrefixStyle.Fixed32);

                        if (test == null)
                            break;

                        switch (test.TypeData)
                        {
                            case EType.Error:
                                break;
                            case EType.FUCK:
                                ServerSend.WelcomeToServer ff = Serializer.DeserializeWithLengthPrefix<ServerSend.WelcomeToServer>(stream, PrefixStyle.Fixed32);
                                Debug.Log(ff.msg);
                                test.Data = ff;
                                break;
                        }

                        test.SerializeMessageId = Id;


                        ServerSend.SendTCPDataToAllExept(Id, test);
                    }
                    catch
                    {
                        //TODO : disconnect
                    }
                }
                connected = false;
            }
        }
    }
}
