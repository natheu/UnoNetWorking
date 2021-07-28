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
    //List<List<int>> CardsInHand = new List<List<int>>();
    List<PlayerGameData.CardType> CardsInHand = new List<PlayerGameData.CardType>();
    /*List<PlayerGameData.CardType> CardsInHandTest = new List<PlayerGameData.CardType>();
    Dictionary<int, PlayerGameData.CardType> CardsInHandTest2 = new Dictionary<int, PlayerGameData.CardType>();*/
    UnoCardTextures Textures;

    [SerializeField]
    float DistBetweenCards = 0.5f;
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

        // OLD-----------
        /*for (int i = 0; i < 5; i++)
        {
            CardsInHand.Add(new List<int>());
        }*/
        //---------------

        int j = 0;
        foreach (PlayerGameData.CardType card in beginCard)
        {
            //Debug.Log(card.CardColor);
            //CardsInHandTest.Add(card);
            /*CardsInHand[(int)card.CardColor - 1].Add(card.Effect);
            SpawnCards(card, j, new Vector3(-1 + j, 2, j * 0.0001f));*/
            AddCard(card);
            //CardsInHandTest2.Add(, card);
            j++;
        }


        // spawn all the card
    }

    public void SpawnCards(PlayerGameData.CardType cardToSpawn, int index, Vector3 pos)
    {

        GameObject gm = Instantiate(Textures.GetPrefab(), Vector3.zero, Quaternion.identity, transform.GetChild(6));
        gm.transform.localPosition = pos;
        gm.transform.localRotation = Quaternion.identity;
        //gm.transform.parent = transform;
        gm.name = ((int)cardToSpawn.CardColor).ToString() + "_" + cardToSpawn.Effect.ToString();

        if (controller == EController.PLAYER)
            gm.transform.GetChild(0).transform.GetChild(0).gameObject.layer = LayerMask.NameToLayer("PlayerCard");
        else
            gm.transform.GetChild(0).transform.GetChild(0).gameObject.layer = LayerMask.NameToLayer("OtherPCard");

        gm.transform.SetSiblingIndex(index);
    }

    public void AddCard(PlayerGameData.CardType cardToAdd)
    {
        Vector3 direction = new Vector3(DistBetweenCards, 0, 0);
        int newNbCard = CardsInHand.Count + 1;
        float offsetMiddle = 0f;
        if (newNbCard % 2 == 0)
            offsetMiddle = 0.5f;

        float middleCard = (int)(newNbCard / 2f) + offsetMiddle;
        bool cardAdd = false;
        for (int i = 0; i < CardsInHand.Count; i++)
        {
            if (CardsInHand[i].CardColor == cardToAdd.CardColor && !cardAdd)
            {
                if (CardsInHand[i].Effect > cardToAdd.Effect)
                {
                    CardsInHand.Insert(i, cardToAdd);
                    Debug.Log("index : " + i);
                    SpawnCards(cardToAdd, i, new Vector3(middleCard * -DistBetweenCards, 0f, i * 0.001f) + direction * i);
                    cardAdd = true;
                }
            }
            else if (CardsInHand[i].CardColor > cardToAdd.CardColor && !cardAdd)
            {
                CardsInHand.Insert(i, cardToAdd);
                Debug.Log("index : " + i);
                SpawnCards(cardToAdd, i, new Vector3(middleCard * -DistBetweenCards, 0f, i * 0.001f) + direction * i);
                cardAdd = true;
            }
            else
            {
                Debug.Log("index : " + i);
                transform.GetChild(6).GetChild(i).localPosition = new Vector3(middleCard * -DistBetweenCards, 0f, i * 0.001f) + direction * i;
            }

        }

        if(!cardAdd)
        {
            int nbCardBefore = CardsInHand.Count;
            CardsInHand.Add(cardToAdd);
            SpawnCards(cardToAdd, nbCardBefore, new Vector3(middleCard * -DistBetweenCards, 0f, nbCardBefore * 0.001f) + direction * nbCardBefore);
            //cardAdd = true;
        }

    }

    void RemoveCard(PlayerGameData.CardType cardToRemove, int index)
    {
        Vector3 direction = new Vector3(DistBetweenCards, 0, 0);
        int newNbCard = CardsInHand.Count + 1;
        float offsetMiddle = 0f;
        if (newNbCard % 2 == 0)
            offsetMiddle = 0.5f;

        float middleCard = (int)(newNbCard / 2f) + offsetMiddle;

        CardsInHand.RemoveAt(index);
        Destroy(transform.GetChild(6).GetChild(index).gameObject);

        for (int i = 0; i < CardsInHand.Count; i++)
        {
            transform.GetChild(6).GetChild(i).localPosition = new Vector3(middleCard * -DistBetweenCards, 0f, i * 0.001f) + direction * i;
        }
    }
    /*
    public void AddCard(PlayerGameData.CardType cardToAdd)
    {
        float disCards = 0.5f;
        Vector3 direction = new Vector3(disCards, 0, 0);
        NbCard++;
        //int lasEffect = 0;
        int count = CardsInHand[(int)cardToAdd.CardColor - 1].Count;

        float offsetMiddle = 0f;
        if (NbCard % 2 == 0)
            offsetMiddle = 0.5f;

        float middleCard = (int)(NbCard / 2f) + offsetMiddle;

        float countOffset = 0;
        for (int i = 1; i <= 4; i++)
        {
            int countCurrentColor = CardsInHand[i - 1].Count;
            if ((PlayerGameData.CardType.Color)i == cardToAdd.CardColor)
                countCurrentColor++;
            float offsetCurrent = 0f;
            if (countCurrentColor % 2f == 0)
                offsetCurrent = 0.5f;
             transform.GetChild(i).localPosition = new Vector3(middleCard * -disCards, 0f, i * 0.0001f) + direction * (countOffset + (int)(countCurrentColor / 2f) + offsetCurrent);

            countOffset += countCurrentColor;

        }

        bool cardAdd = false;

        offsetMiddle = 0f;
        if ((CardsInHand[(int)cardToAdd.CardColor - 1].Count + 1) % 2 == 0)
            offsetMiddle = 0.5f;
        middleCard = (int)((CardsInHand[(int)cardToAdd.CardColor - 1].Count + 1) / 2f) + offsetMiddle;

        for (int i = 0; i < CardsInHand[(int)cardToAdd.CardColor - 1].Count; i++)
        {
            if (CardsInHand[(int)cardToAdd.CardColor - 1][i] > cardToAdd.Effect && !cardAdd)
            {
                CardsInHand[(int)cardToAdd.CardColor - 1].Insert(i, cardToAdd.Effect);
                SpawnCards(cardToAdd, i, 
                            new Vector3(-disCards * middleCard, 0, i * 0.0001f) + direction * i);
                cardAdd = true;
            }
            else
            {
                transform.GetChild((int)cardToAdd.CardColor).GetChild(i).localPosition = 
                                            new Vector3(-disCards * middleCard, 0, i * 0.0001f) + direction * i;
            }
        }
        if (!cardAdd)
        {
            CardsInHand[(int)cardToAdd.CardColor - 1].Add(cardToAdd.Effect);
            SpawnCards(cardToAdd, CardsInHand[(int)cardToAdd.CardColor - 1].Count - 1, new Vector3(disCards * (CardsInHand[(int)cardToAdd.CardColor - 1].Count + offsetMiddle), 0f, 
                                                    CardsInHand[(int)cardToAdd.CardColor - 1].Count * 0.0001f));
        }
    }
    */
    // Delete the card play by the current player
    // return the card that has been played
    public PlayerGameData.CardType CardPlay(ref UnoNetworkingGameData.GameData data)
    {
        List<PlayerGameData.CardType> cards = new List<PlayerGameData.CardType>(data.CardTypePutOnBoard);
        if (cards.Count >= 1)
        {
            PlayerGameData.CardType cardType = data.CardTypePutOnBoard[0];
            //CardsInHand[(int)cardType.CardColor - 1].RemoveAt(data.PosInHand);
            RemoveCard(cardType, data.PosInHand);
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
        Debug.Log("number to draw : " + data.CardTypePutOnBoard.Length);
        foreach (PlayerGameData.CardType cardType in data.CardTypePutOnBoard)
        {
            AddCard(cardType);
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
                    cardSelected.position = new Vector3(cardSelected.position.x, 0f, cardSelected.position.z);
                    cardSelected = TNameCard;
                    cardSelected.transform.position = new Vector3(TNameCard.position.x, 0.5f, TNameCard.position.z);
                }
                else
                {
                    cardSelected = outHit.transform;
                    cardSelected.transform.position = new Vector3(TNameCard.position.x, 0.5f, TNameCard.position.z);
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
                    if (CardsInHand[indexCurrentCard].Effect == currentCard.Effect)
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
