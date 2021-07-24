using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnoPlayer : MonoBehaviour
{
    public enum EController
    {
        DEFAULT = 0,
        PLAYER,
        IA,
        ENEMY
    }


    EController controller = EController.DEFAULT;
    List<List<int>> CardsInHand = new List<List<int>>();
    List<PlayerGameData.CardType> CardsInHandTest = new List<PlayerGameData.CardType>();
    Dictionary<int, PlayerGameData.CardType> CardsInHandTest2 = new Dictionary<int, PlayerGameData.CardType>();
    UnoCardTextures Textures;

    [SerializeField]
    LayerMask cardMask;
    PlayerGameData.CardType currentCard = new PlayerGameData.CardType();
    int indexCurrentCard = 0;
    Transform cardSelected;


    // Start is called before the first frame update
    void Start()
    {
    }

    public void InitPlayer(PlayerGameData.CardType[] beginCard, UnoCardTextures textures, EController c)
    {
        Textures = textures;
        controller = c;

        for (int i = 0; i < 5; i++)
        {
            CardsInHand.Add(new List<int>());
        }
        int j = 0;
        foreach(PlayerGameData.CardType card in beginCard)
        {
            Debug.Log(card.CardColor);
            //CardsInHandTest.Add(card);
            CardsInHand[(int)card.CardColor - 1].Add(card.Effect);
            SpawnCards(card, j, new Vector3(-2 + j, 1, 0));
            //CardsInHandTest2.Add(, card);
            j++;
        }


        // spawn all the card
    }

    public void SpawnCards(PlayerGameData.CardType cardToSpawn, int index, Vector3 pos)
    {

        GameObject gm = Instantiate(Textures.GetPrefab(), Vector3.zero, Quaternion.identity, transform.GetChild((int)cardToSpawn.CardColor));
        gm.transform.localPosition = pos;
        gm.transform.localRotation = Quaternion.identity;
        //gm.transform.parent = transform;
        gm.name = ((int)cardToSpawn.CardColor).ToString() + "_" + cardToSpawn.Effect.ToString();

        if (controller == EController.PLAYER)
            gm.transform.GetChild(0).transform.GetChild(0).gameObject.layer = LayerMask.NameToLayer("PlayerCard");
        else
            gm.transform.GetChild(0).transform.GetChild(0).gameObject.layer = LayerMask.NameToLayer("OtherPCard");
    }

    // Delete the card play by the current player
    // return the card that has been played
    public PlayerGameData.CardType CardPlay(ref UnoNetworkingGameData.GameData data)
    {
        List<PlayerGameData.CardType> cards = new List<PlayerGameData.CardType>(data.CardTypePutOnBoard);
        if (cards.Count > 1)
        {
            PlayerGameData.CardType cardType = data.CardTypePutOnBoard[0];
            CardsInHand[(int)cardType.CardColor - 1].RemoveAt(data.PosInHand);
            // TO DO Animation Play card
            cards.RemoveAt(0);
            data.CardTypePutOnBoard = cards.ToArray();
            return cardType;
        }
        return new PlayerGameData.CardType(PlayerGameData.CardType.Color.DEFAULT, 0);
    }

    public void DrawCards(UnoNetworkingGameData.GameData data, bool AddTextures)
    {
        foreach (PlayerGameData.CardType cardType in data.CardTypePutOnBoard)
        {
            CardsInHand[(int)cardType.CardColor].Add(cardType.Effect);
            // TO DO Animation Draw Cards
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (controller == EController.PLAYER)
        {
            RaycastHit outHit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out outHit, 1000f, cardMask))
            {

                string[] nameCardSplit = outHit.transform.parent.parent.name.Split('_');
                //Debug.Log("Card Touch : " + outHit.transform.name);
                if (cardSelected != outHit.transform && cardSelected != null)
                {
                    cardSelected.position = new Vector3(cardSelected.position.x, 1, cardSelected.position.z);
                    cardSelected = outHit.transform;
                    cardSelected.transform.position = new Vector3(outHit.transform.position.x, 2, outHit.transform.position.z);
                }
                else
                {
                    cardSelected = outHit.transform;
                    cardSelected.transform.position = new Vector3(outHit.transform.position.x, 2, outHit.transform.position.z);
                }
                //outHit.transform.position = new Vector3(outHit.transform.position.x, 0, outHit.transform.position.z) + new Vector3(0, 2, 0);

                if (nameCardSplit.Length >= 2)
                {
                    currentCard.CardColor = (PlayerGameData.CardType.Color)int.Parse(nameCardSplit[0]);

                    currentCard.Effect = int.Parse(nameCardSplit[1]);

                    indexCurrentCard = FindPosCard(outHit.transform);
                    //Debug.Log("It's working");
                }
            }
            else if(cardSelected != null)
            {
                cardSelected.position = new Vector3(cardSelected.position.x, 1, cardSelected.position.z);
                cardSelected = null;
            }

        }
    }

    private void OnDrawGizmos()
    {
        Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
        Gizmos.DrawLine(r.origin, r.origin + r.direction * 100);

        Gizmos.DrawSphere(Camera.main.ScreenToWorldPoint(Input.mousePosition), 1);
    }

    public void UpdatePlayer(PlayerGameData.CardType onBoardCard)
    {
        if (controller == EController.PLAYER)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if(CardsInHand[(int)currentCard.CardColor - 1][indexCurrentCard] == currentCard.Effect)
                {
                    Debug.Log("For the moment everithing is fine");
                }
            }
        }
    }

    private int FindPosCard(Transform cardObject)
    {
        return cardObject.GetSiblingIndex();

    }

    public void ToControllerIA()
    {
        controller = EController.IA;
    }

    /*public void SetController(EController c)
    {
        controller = c;
    }*/
}
