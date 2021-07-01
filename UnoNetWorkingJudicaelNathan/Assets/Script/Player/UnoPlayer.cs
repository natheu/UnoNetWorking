using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnoPlayer : MonoBehaviour
{
    List<PlayerGameData.CardType> CardsInHand = new List<PlayerGameData.CardType>();
    UnoCardTextures Textures;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void InitPlayer(List<PlayerGameData.CardType> beginCard, UnoCardTextures textures)
    {
        CardsInHand = beginCard;
        Textures = textures;
        // spawn all the card
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
