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

    public void InitPlayer(List<PlayerGameData.CardType> beginCard, UnoCardTextures textures)
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

    public void SpawnCards(List<PlayerGameData.CardType> cardToSpawn)
    {

    }

    // Delete the card play by the current player
    public PlayerGameData.CardType CardPlay(UnoNetworkingGameData.GameData data)
    {
        if (data.CardTypePutOnBoard.Count > 1)
        {
            PlayerGameData.CardType cardType = data.CardTypePutOnBoard[0];
            CardsInHand[(int)cardType.CardColor].RemoveAt(data.PosInHand);
            // TO DO Animation Play card
            data.CardTypePutOnBoard.RemoveAt(0);
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
