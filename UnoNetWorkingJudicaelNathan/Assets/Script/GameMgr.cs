using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMgr : MonoBehaviour
{
    public bool host;

    NetWorkingCSharp.ClientTest client;

    void Start()
    {
        //NetWorkingCSharp.Server.CreateServer(10, 50150);
        client = new NetWorkingCSharp.ClientTest();
    }

    void Update()
    {
        
    }

    public bool CreateClient(string Ip, int port)
    {
        return client.CreateClient(Ip);
    }

    public void SendMsg(string msg)
    {
        NetWorkingCSharp.Header header = new NetWorkingCSharp.Header(msg, NetWorkingCSharp.EType.MSG, client.Id);

        client.SendToServer(header);
    }


    private void OnDestroy()
    {
        if (NetWorkingCSharp.Server.stateGame == NetWorkingCSharp.Server.EStateGame.START)
            NetWorkingCSharp.Server.CloseListener();
    }
}
