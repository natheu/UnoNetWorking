using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnoPlayer : MonoBehaviour
{
    public enum EController
    {
        DEFAULT = 0,
        PLAYER,
        IA,
        ENEMY
    }
    static int indexCardsChild = 1;

    EController controller = EController.DEFAULT;
    //List<List<int>> CardsInHand = new List<List<int>>();
    List<PlayerGameData.CardType> CardsInHand = new List<PlayerGameData.CardType>();
    /*List<PlayerGameData.CardType> CardsInHandTest = new List<PlayerGameData.CardType>();*/
    UnoCardTextures Textures;

    [SerializeField]
    float DistBetweenCards = 0.5f;

    [SerializeField]
    LayerMask deckMask;
    [SerializeField]
    LayerMask CardMask;
    [SerializeField]
    LayerMask ChooseColor;

    LayerMask currentLayer;

    PlayerGameData.CardType currentCard = new PlayerGameData.CardType();
    int indexCurrentCard = -1;
    Transform cardSelected;


    // Start is called before the first frame update
    void Start()
    {
        currentLayer = CardMask;
    }

    public void InitPlayer(string name, PlayerGameData.CardType[] beginCard, UnoCardTextures textures, EController c)
    {
        Textures = textures;
        controller = c;
        //---------------
        // spawn all the card
        foreach (PlayerGameData.CardType card in beginCard)
        {
            //Debug.Log(card.CardColor);
            //CardsInHandTest.Add(card);
            /*CardsInHand[(int)card.CardColor - 1].Add(card.Effect);
            SpawnCards(card, j, new Vector3(-1 + j, 2, j * 0.0001f));*/
            AddCard(card);
            //CardsInHandTest2.Add(, card);
        }

        Transform canvasPlayer = transform.GetChild(2);
        canvasPlayer.GetComponent<Canvas>().worldCamera = Camera.main;
        canvasPlayer.GetChild(0).GetChild(0).GetComponent<Text>().text = name;
    }

    public void SpawnCards(PlayerGameData.CardType cardToSpawn, int index, Vector3 pos)
    {

        GameObject gm = Instantiate(Textures.GetPrefab(), Vector3.zero, Quaternion.identity, transform.GetChild(indexCardsChild));
        gm.transform.localPosition = pos;
        gm.transform.localRotation = Quaternion.identity;

        gm.name = ((int)cardToSpawn.CardColor).ToString() + "_" + cardToSpawn.Effect.ToString();

        if (controller == EController.PLAYER)
            gm.transform.GetChild(0).transform.GetChild(0).gameObject.layer = LayerMask.NameToLayer("PlayerCard");
        else
            gm.transform.GetChild(0).transform.GetChild(0).gameObject.layer = LayerMask.NameToLayer("OtherPCard");

        if (controller == EController.PLAYER)
        {
            //if (cardToSpawn.CardColor != PlayerGameData.CardType.Color.ANY)
            gm.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material.mainTexture =
                                Textures.GetSprite((int)cardToSpawn.CardColor - 1, cardToSpawn.Effect);
            /*else
                gm.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material.mainTexture =
                                Textures.GetSprite((int)cardToSpawn.CardColor - 1, cardToSpawn.Effect - PlayerGameData.PLUS_FOUR);*/
        }
        else
            gm.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material.mainTexture = null;

        gm.transform.SetSiblingIndex(index);
    }

    public void AddCard(PlayerGameData.CardType cardToAdd)
    {
        Vector3 direction = new Vector3(DistBetweenCards, 0, 0);
        int newNbCard = CardsInHand.Count + 1;

        float middleCard = (int)(newNbCard / 2f);
        if (newNbCard % 2 == 0)
            middleCard = middleCard -1 + DistBetweenCards / 2f;

        //Debug.Log("card midd" + middleCard);
        bool cardAdd = false;

        for (int i = 0; i < CardsInHand.Count; i++)
        {

            //if (controller == EController.PLAYER)
            SortCardForControllerPlayer(cardToAdd, i, middleCard, direction, ref cardAdd);
            //else
        }

        if(!cardAdd)
        {
            int nbCardBefore = CardsInHand.Count;
            CardsInHand.Add(cardToAdd);
            SpawnCards(cardToAdd, nbCardBefore, new Vector3(middleCard * -DistBetweenCards, 0f, nbCardBefore * 0.001f) + direction * nbCardBefore);
        }

    }

    void SortCardForControllerPlayer(PlayerGameData.CardType cardToAdd, int index, float middleCard, Vector3 direction, ref bool cardAdded)
    {
        if (CardsInHand[index].CardColor == cardToAdd.CardColor && !cardAdded && CardsInHand[index].Effect > cardToAdd.Effect)
        {
            CardsInHand.Insert(index, cardToAdd);
            SpawnCards(cardToAdd, index, new Vector3(middleCard * -DistBetweenCards, 0f, index * 0.001f) + direction * index);
            cardAdded = true;
        }
        else if (CardsInHand[index].CardColor > cardToAdd.CardColor && !cardAdded)
        {
            CardsInHand.Insert(index, cardToAdd);
            SpawnCards(cardToAdd, index, new Vector3(middleCard * -DistBetweenCards, 0f, index * 0.001f) + direction * index);
            cardAdded = true;
        }
        else
            transform.GetChild(indexCardsChild).GetChild(index).localPosition = new Vector3(middleCard * -DistBetweenCards, 0f, index * 0.001f) + direction * index;
    }

    void RemoveCard(PlayerGameData.CardType cardToRemove, int index)
    {
        Vector3 direction = new Vector3(DistBetweenCards, 0, 0);

        int newNbCard = CardsInHand.Count - 1;
        float middleCard = (int)(newNbCard / 2f);
        if (newNbCard % 2 == 0)
            middleCard = middleCard - 1 + DistBetweenCards / 2f;

        // the index future index of the card
        int tmpi = 0;
        for (int i = 0; i < CardsInHand.Count; i++)
        {
            if (i != index)
            {
                transform.GetChild(indexCardsChild).GetChild(i).localPosition = new Vector3(middleCard * -DistBetweenCards, 0f, tmpi * 0.001f) + direction * tmpi;
                tmpi++;
            }
        }
        CardsInHand.RemoveAt(index);
        Destroy(transform.GetChild(indexCardsChild).GetChild(index).gameObject);
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
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out outHit, 1000f, currentLayer))
            {
                /*Transform TNameCard = outHit.transform.parent.parent;
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

                    //currentCard = CardsInHand[FindPosCard(TNameCard)];

                    indexCurrentCard = FindPosCard(TNameCard);
                    //Debug.Log("It's working");
                }*/
                if (outHit.transform.gameObject.layer == LayerMask.NameToLayer("PlayerCard"))
                    UpdatePosCardSelected(outHit);
                else if (outHit.transform.gameObject.layer == LayerMask.NameToLayer("ChooseColor"))
                {
                    currentCard.CardColor = (PlayerGameData.CardType.Color)int.Parse(outHit.transform.name);
                    cardSelected = outHit.transform;
                    //Debug.Log("ChooseColor");
                }
            }
            else if(cardSelected != null)
            {
                currentCard = new PlayerGameData.CardType();
                indexCurrentCard = -1;
            }

        }
    }

    private void UpdatePosCardSelected(RaycastHit outHit)
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

            //currentCard = CardsInHand[FindPosCard(TNameCard)];

            indexCurrentCard = FindPosCard(TNameCard);
            //Debug.Log("It's working");
        }
    }

    private void OnDrawGizmos()
    {
        Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
        Gizmos.DrawLine(r.origin, r.origin + r.direction * 100);

        Gizmos.DrawSphere(Camera.main.ScreenToWorldPoint(Input.mousePosition), 1);
    }

    public NetWorkingCSharp.HeaderGameData UpdatePlayer(PlayerGameData.CardType onBoardCard, PlayModMgr.EPlayModState playState)
    {
        NetWorkingCSharp.HeaderGameData header = new NetWorkingCSharp.HeaderGameData();
        header.dataType = NetWorkingCSharp.HeaderGameData.EDataType.DEFAULT;
        if (controller == EController.PLAYER)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (playState == PlayModMgr.EPlayModState.PLAY)
                {
                    currentLayer = CardMask;
                    if (indexCurrentCard != -1)
                    {
                        UnoNetworkingGameData.GameData data = CardChoose(onBoardCard);
                        if(data.type != UnoNetworkingGameData.GameData.TypeData.DEFAULT)
                        {
                            header.dataType = NetWorkingCSharp.HeaderGameData.EDataType.CARD;
                            header.GameData = data;
                            return header;
                        }
                    }
                    RaycastHit outHit;
                    if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out outHit, 1000f, deckMask))
                    {
                        UnoNetworkingGameData.GameData gameData = new UnoNetworkingGameData.GameData();
                        gameData.type = UnoNetworkingGameData.GameData.TypeData.DRAWCARDS;
                        header.dataType = NetWorkingCSharp.HeaderGameData.EDataType.CARD;
                        header.GameData = gameData;
                        return header;
                    }
                }
                else if (playState == PlayModMgr.EPlayModState.WAITCOLOR)
                {
                    Debug.Log("CHOOSE COLOR");
                    header.dataType = NetWorkingCSharp.HeaderGameData.EDataType.CHOOSECOLOR;
                    header.GameData = (int)currentCard.CardColor;

                    GameObject toDestroy = cardSelected.parent.gameObject;
                    cardSelected = null;
                    Destroy(toDestroy);
                }

            }
        }

        return header;
    }

    public void ToChooseColor()
    {
        currentLayer = ChooseColor;
    }

    public void ToChooseCard()
    {
        currentLayer = CardMask;
    }

    private UnoNetworkingGameData.GameData CardChoose(PlayerGameData.CardType onBoardCard)
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
            }
        }

        return new UnoNetworkingGameData.GameData();
    }
    /*if (CardsInHand[indexCurrentCard].Effect == currentCard.Effect)
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

        }
    }
    else
    {
        Debug.Log("You need To think again color : " + currentCard.CardColor + "effect : " + currentCard.Effect);
    }*/

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
