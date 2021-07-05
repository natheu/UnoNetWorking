using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayModMgr : MonoBehaviour
{ 
    [SerializeField]
    private float DistBetweenPlayer = 2f;
    [SerializeField]
    private GameObject PrefabPlayer = null;

    private int DirectionBoard = -1;
    private int CurrentPlayer = 0;
    private float TimerCurrPlayer = 0f;

    private

    List<UnoPlayer> players = new List<UnoPlayer>();

    UnityEvent<PlayerGameData.CardType> CardPlayEvent = new UnityEvent<PlayerGameData.CardType>();

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

        TimerCurrPlayer -= Time.deltaTime;
        if (TimerCurrPlayer < 0)
        {
            TimerCurrPlayer = 0;
            /*
            UpdateCurrentPlayer();
            if (NetWorkingCSharp.ServerTCP.host)
            {
                
            }
            */
        }

        //if()

    }

    private int GetNextPlayer(int posPlayer)
    {
        int next = posPlayer + DirectionBoard;
        if (next < 0)
            next = players.Count - 1;
        else if (next >= players.Count)
            next = 0;

        return next;
    }

    public void UpdateActionPlayer(int IdPLayerAction, NetWorkingCSharp.LocalClientDataGame<UnoNetworkingGameData.GameData> data)
    {
        switch(data.GameData.type)
        {
            case UnoNetworkingGameData.GameData.TypeData.DEFAULT:
                break;
            case UnoNetworkingGameData.GameData.TypeData.CARDPLAY:
                PlayerGameData.CardType cardType = players[CurrentPlayer].CardPlay(ref data.GameData);
                CardPlayEvent.Invoke(cardType);
                EffectPlayCard(cardType, data.GameData);
                break;
            case UnoNetworkingGameData.GameData.TypeData.DRAWCARDS:
                break;
        }
    }

    private void EffectPlayCard(PlayerGameData.CardType cardType, UnoNetworkingGameData.GameData gameData)
    {
        // the card is a number no special effect
        if (cardType.Effect < 10)
            return;

        // the card plays is a +2
        if(cardType.Effect == 10)
        {
            int nextPlayer = GetNextPlayer(CurrentPlayer);
            foreach (KeyValuePair<int, PlayerGameData> client in NetWorkingCSharp.ServerTCP.ClientsGameData)
            {
                if(client.Value.GetPosOnBoard() == nextPlayer)
                {
                    players[CurrentPlayer].DrawCards(gameData, true);
                }
                else
                    players[CurrentPlayer].DrawCards(gameData, false);
            }

            CurrentPlayer = GetNextPlayer(nextPlayer);
        }
        /*
        // the card plays is Turn Pass
        else if(cardType.Effect == 11)
        {

        }
        // the card plays is Invert Direction 
        else if (cardType.Effect == 12)
        {

        }
        */
    }

    public void CreateBoard(int numberOfCard, List<UnoNetworkingGameData.GameData> playerHandData, UnoCardTextures textures)
    {
        List<Vector3> AllPos = CreateAllPos(NetWorkingCSharp.ServerTCP.ClientsGameData.Count);
        int i = 0;
        foreach (KeyValuePair<int, PlayerGameData> client in NetWorkingCSharp.ServerTCP.ClientsGameData)
        {
            Vector3 PosToCenter = (Vector3.zero - AllPos[i]).normalized;
            GameObject Player = Instantiate(PrefabPlayer, AllPos[i], Quaternion.FromToRotation(Vector3.forward, PosToCenter));
            players.Insert(i, Player.GetComponent<UnoPlayer>());

            Player.GetComponent<UnoPlayer>().InitPlayer(playerHandData[i].CardTypePutOnBoard, textures);

            if (client.Value.GetPosOnBoard() == i)
            {
                Camera.main.transform.position = Player.transform.Find("PosPlayer").position;
                Camera.main.transform.rotation = Quaternion.FromToRotation(Vector3.forward, PosToCenter);
            }

            i++;
        }
    }

    private List<Vector3> CreateAllPos(int numberPlayer)
    {
        List<Vector3> localPositions = new List<Vector3>(numberPlayer);
        // get the angle of the first point to get the angle of the isoscel triangle 
        float Tmpangle = (1 * (Mathf.PI * 2 / numberPlayer));
        // get distance between the center (considered as the vertex of the two legs) and one of the point on base of the isoscel triangle
        // to get the radius our circle
        float radius = (float)DistBetweenPlayer / Mathf.Sin((Tmpangle / 2f) * 2f);
        if (numberPlayer == 2)
            radius = DistBetweenPlayer;
        Debug.Log((Tmpangle / 2f) * 2f);

        for (int i = 0; i < numberPlayer; i++)
        {
            // the angle on the Unit circle between 0 - 360
            float angle = (i * (Mathf.PI * 2 / numberPlayer));
            Vector3 pos = Vector3.zero;
            pos.x = Mathf.Cos(angle) * radius;
            pos.z = Mathf.Sin(angle) * radius;

            Debug.Log(pos);

            localPositions.Add(pos);
        }
        
        return localPositions;
    }


    public void PlayerDisconnected(int posOnBoard)
    {
        players[posOnBoard].ControlledByAI = true;
    }
}
