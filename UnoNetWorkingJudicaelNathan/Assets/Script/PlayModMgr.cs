using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayModMgr : MonoBehaviour
{
    [SerializeField]
    private float DistBetweenPlayer = 2f;
    [SerializeField]
    private GameObject PrefabPlayer = null;

    private int DirectionBoard = -1;

    List<UnoPlayer> players = new List<UnoPlayer>();

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void CreateBoard(int numberOfCard, List<UnoNetworkingGameData.CardType> beginCard)
    {
        List<Vector3> AllPos = CreateAllPos(NetWorkingCSharp.ServerTCP.ClientsGameData.Count);
        int i = 0;
        foreach (KeyValuePair<int, UnoNetworkingGameData> client in NetWorkingCSharp.ServerTCP.ClientsGameData)
        {
            Vector3 PosToCenter = (Vector3.zero - AllPos[i]).normalized;
            GameObject Player = Instantiate(PrefabPlayer, AllPos[i], Quaternion.FromToRotation(Vector3.forward, PosToCenter));
            players.Insert(i, Player.GetComponent<UnoPlayer>());

            Player.GetComponent<UnoPlayer>().CreatePlayer(beginCard);

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
}
