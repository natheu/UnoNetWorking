using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardSelector", menuName = "ScriptableObjects/CardSelector")]
public class CardSelector : ScriptableObject
{

    [SerializeField]
    private List<int> EffectFactor = new List<int>(15);
    [SerializeField]
    // in the real Game there is 108 cards
    private uint NbCardsInOnePackage = 108;

    private List<List<int>> FactorCards = new List<List<int>>();

    public List<List<int>> GetFactorCards()
    {
        return FactorCards;
    }

    [HideInInspector]
    public int IndexValueUpdate = 0;

    private void OnValidate()
    {
        CreateFactorForAllCards();
    }

    private void CreateFactorForAllCards()
    {
        FactorCards.Clear();

        // add the color ANY
        for (int i = 0; i <= PlayerGameData.NB_COLOR; i++)
        {
            FactorCards.Add(new List<int>());
        }

        for (int i = 0; i <= PlayerGameData.CHOOSE_COLOR; i++)
        {
            if (i <= PlayerGameData.CARD_WITH_COLOR)
            {
                for (int j = 0; j < PlayerGameData.NB_COLOR; j++)
                {
                    FactorCards[j].Add(EffectFactor[i]);
                }
            }
            else
                FactorCards[PlayerGameData.NB_COLOR].Add(EffectFactor[i]);

        }
    }
    /*
    // when a factor is changed for one card change for the other
    private void CalculateFactor()
    {
        for(int i = 0; i < EffectFactor.Count; i++)
        {

        }
    }*/
}
