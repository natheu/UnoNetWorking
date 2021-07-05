using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnoPlayer : MonoBehaviour
{
    List<List<int>> CardsInHand = new List<List<int>>();
    UnoCardTextures Textures;
    public bool ControlledByAI = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void InitPlayer(PlayerGameData.CardType[] beginCard, UnoCardTextures textures)
    {
        for(int i = 0; i < 5; i++)
        {
            CardsInHand.Add(new List<int>());
        }

        foreach(PlayerGameData.CardType card in beginCard)
        {
            CardsInHand[(int)card.CardColor].Add(card.Effect);
        }

        SpawnCards(beginCard);

        // spawn all the card

        Textures = textures;
    }

    public void SpawnCards(PlayerGameData.CardType[] cardToSpawn)
    {

    }

    // Delete the card play by the current player
    // return the card that has been played
    public PlayerGameData.CardType CardPlay(ref UnoNetworkingGameData.GameData data)
    {
        List<PlayerGameData.CardType> cards = new List<PlayerGameData.CardType>(data.CardTypePutOnBoard);
        if (cards.Count > 1)
        {
            PlayerGameData.CardType cardType = data.CardTypePutOnBoard[0];
            CardsInHand[(int)cardType.CardColor].RemoveAt(data.PosInHand);
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
        
    }
}
