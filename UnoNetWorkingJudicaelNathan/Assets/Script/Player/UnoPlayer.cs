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
    LayerMask deckMask;
    [SerializeField]
    LayerMask cardMask;
    PlayerGameData.CardType currentCard = new PlayerGameData.CardType();
    int indexCurrentCard = -1;
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
            //Debug.Log(card.CardColor);
            //CardsInHandTest.Add(card);
            CardsInHand[(int)card.CardColor - 1].Add(card.Effect);
            SpawnCards(card, j, new Vector3(-1 + j, 2, j * 0.0001f));
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

    /*public void AddCards()
    {

    }*/

    // Delete the card play by the current player
    // return the card that has been played
    public PlayerGameData.CardType CardPlay(ref UnoNetworkingGameData.GameData data)
    {
        List<PlayerGameData.CardType> cards = new List<PlayerGameData.CardType>(data.CardTypePutOnBoard);
        if (cards.Count >= 1)
        {
            PlayerGameData.CardType cardType = data.CardTypePutOnBoard[0];
            CardsInHand[(int)cardType.CardColor - 1].RemoveAt(data.PosInHand);
            Destroy(transform.GetChild((int)cardType.CardColor).GetChild(data.PosInHand).gameObject);
            // TO DO Animation Play card
            cards.RemoveAt(0);
            data.CardTypePutOnBoard = cards.ToArray();
            Debug.Log("CardPlay detected");
            return cardType;
        }
        return new PlayerGameData.CardType(PlayerGameData.CardType.Color.DEFAULT, 0);
    }

    public void DrawCards(UnoNetworkingGameData.GameData data)
    {
        foreach (PlayerGameData.CardType cardType in data.CardTypePutOnBoard)
        {
            CardsInHand[(int)cardType.CardColor].Add(cardType.Effect);
            SpawnCards(cardType, 0, Vector3.zero);
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
                Transform TNameCard = outHit.transform.parent.parent;
                string[] nameCardSplit = TNameCard.name.Split('_');
                //Debug.Log("Card Touch : " + outHit.transform.name);
                if (cardSelected != outHit.transform && cardSelected != null)
                {
                    cardSelected.position = new Vector3(cardSelected.position.x, 1, cardSelected.position.z);
                    cardSelected = TNameCard;
                    cardSelected.transform.position = new Vector3(TNameCard.position.x, 1f, TNameCard.position.z);
                }
                else
                {
                    cardSelected = outHit.transform;
                    cardSelected.transform.position = new Vector3(TNameCard.position.x, 1f, TNameCard.position.z);
                }

                if (nameCardSplit.Length >= 2)
                {
                    currentCard.CardColor = (PlayerGameData.CardType.Color)int.Parse(nameCardSplit[0]);

                    currentCard.Effect = int.Parse(nameCardSplit[1]);

                    indexCurrentCard = FindPosCard(TNameCard);
                    //Debug.Log("It's working");
                }
            }
            else if(cardSelected != null)
            {
                cardSelected.position = new Vector3(cardSelected.position.x, 0, cardSelected.position.z);
                cardSelected = null;
                currentCard = new PlayerGameData.CardType();
                indexCurrentCard = -1;
            }

        }
    }

    private void OnDrawGizmos()
    {
        Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
        Gizmos.DrawLine(r.origin, r.origin + r.direction * 100);

        Gizmos.DrawSphere(Camera.main.ScreenToWorldPoint(Input.mousePosition), 1);
    }

    public UnoNetworkingGameData.GameData UpdatePlayer(PlayerGameData.CardType onBoardCard)
    {
        if (controller == EController.PLAYER)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (indexCurrentCard != -1)
                {
                    if (CardsInHand[(int)currentCard.CardColor - 1][indexCurrentCard] == currentCard.Effect)
                    {
                        Debug.Log("color : " + currentCard.CardColor + "effect : " + currentCard.Effect);

                        if (PlayerGameData.CanPutCardOnBoard(onBoardCard, currentCard))
                        {
                            Debug.Log("You can Play this card");

                            UnoNetworkingGameData.GameData gameData = new UnoNetworkingGameData.GameData();
                            gameData.PosInHand = indexCurrentCard;
                            gameData.CardTypePutOnBoard = new PlayerGameData.CardType[] { currentCard };
                            gameData.type = UnoNetworkingGameData.GameData.TypeData.CARDPLAY;

                            return gameData;

                            /*NetWorkingCSharp.Header header = new NetWorkingCSharp.Header(gameData, NetWorkingCSharp.EType.PLAYERACTION, 
                                                                                            new NetWorkingCSharp.ServerTCP.ClientData());

                            GameMgr.SendNetWorkingData(header);*/
                        }
                    }
                    else
                    {
                        Debug.Log("You need To think again color : " + currentCard.CardColor + "effect : " + currentCard.Effect);
                    }
                }

                RaycastHit outHit;
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out outHit, 1000f, deckMask))
                {
                    UnoNetworkingGameData.GameData gameData = new UnoNetworkingGameData.GameData();
                    gameData.type = UnoNetworkingGameData.GameData.TypeData.DRAWCARDS;
                    return gameData;
                }

            }
        }

        return new UnoNetworkingGameData.GameData();
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
