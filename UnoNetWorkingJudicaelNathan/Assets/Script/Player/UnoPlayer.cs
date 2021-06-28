using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnoPlayer : MonoBehaviour
{
    List<UnoNetworkingGameData.CardType> CardsInHand = new List<UnoNetworkingGameData.CardType>(); 



    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void CreatePlayer(List<UnoNetworkingGameData.CardType> beginCard)
    {
        CardsInHand = beginCard;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
