using System;
using System.Collections.Generic;
using System.Text;


namespace NetWorkingCSharp
{
    public struct LocalClientDataGame<T>
    {
        public ServerTCP.ClientData ClientData;
        public T GameData;

        public LocalClientDataGame(ServerTCP.ClientData client, T gameData)
        {
            ClientData = client;
            GameData = gameData;
        }
    }
}