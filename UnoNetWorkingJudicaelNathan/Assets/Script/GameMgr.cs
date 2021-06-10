using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMgr : MonoBehaviour
{

    NetWorkingCSharp.ClientTCP client;
    Dictionary<int, NetWorkingCSharp.ServerTCP.ClientData> localClients = new Dictionary<int, NetWorkingCSharp.ServerTCP.ClientData>();

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        //NetWorkingCSharp.Server.CreateServer(10, 50150);
        //client = new NetWorkingCSharp.ClientTCP();
    }

    void Update()
    {
        if(NetWorkingCSharp.ServerTCP.host)
        {

        }
        if(client != null)
        {
            AnalyseRecievValue();
        }
    }

    public void AnalyseRecievValue()
    {
        if (client.Tcp.headersReciev.Count > 0)
        {
            Debug.Log("Yo");
            NetWorkingCSharp.Header header = client.Tcp.headersReciev.Dequeue();

            switch (header.TypeData)
            {
                case NetWorkingCSharp.EType.Error:
                    break;
                case NetWorkingCSharp.EType.WELCOME:
                    NetWorkingCSharp.ServerSend.WelcomeToServer ff = (NetWorkingCSharp.ServerSend.WelcomeToServer)header.Data;
                    InitAllClient(ff.clientsData);
                    localClients.Add(header.clientData.Id, header.clientData);
                    Debug.Log(ff.msg + " : " + header.clientData.Name);
                    break;
                case NetWorkingCSharp.EType.MSG:
                    string msg = (string)header.Data;
                    Debug.Log(header.clientData.Name + " : " + msg);
                    break;
            }
        }
    }

    public void InitAllClient(List<NetWorkingCSharp.ServerTCP.ClientData> clients)
    {
        if (clients.Count == 0)
            return;
        foreach(NetWorkingCSharp.ServerTCP.ClientData ClientData in clients)
        {
            localClients.Add(ClientData.Id, ClientData);
            Debug.Log("New Comer");
        }
    }

    public bool CreateClient(string Ip, int port)
    {
        client = new NetWorkingCSharp.ClientTCP();
        return client.CreateClient(Ip);
    }

    public void SendMsg(string msg)
    {
        NetWorkingCSharp.Header header = new NetWorkingCSharp.Header(msg, NetWorkingCSharp.EType.MSG, localClients[client.Tcp.clientData.Id]);
        if(NetWorkingCSharp.ServerTCP.host)
            NetWorkingCSharp.ServerSend.SendTCPDataToAll(header);
        else
            client.SendToServer(header);
    }


    private void OnDestroy()
    {
        if (NetWorkingCSharp.ServerTCP.host)
            NetWorkingCSharp.ServerTCP.CloseListener();
    }
}
