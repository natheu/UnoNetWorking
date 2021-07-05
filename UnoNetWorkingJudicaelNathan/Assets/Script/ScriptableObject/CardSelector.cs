using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardSelector", menuName = "ScriptableObjects/CardSelector")]
public class CardSelector : ScriptableObject
{
    [SerializeField]
    private List<float> EffectFactor = new List<float>(15);
    [SerializeField]
    // in the real Game there is 108 cards
    private uint NbCardsInOnePackage = 108;

    private List<List<float>> FactorCards = new List<List<float>>();

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
        for (int i = 0; i < PlayerGameData.NB_COLOR + 1; i++)
        {
            FactorCards.Add(new List<float>());
        }

        for (int i = 0; i < EffectFactor.Count; i++)
        {
            if (i < PlayerGameData.CARD_WITH_COLOR)
            {
                for (int j = 0; j < PlayerGameData.NB_COLOR; i++)
                {
                    FactorCards[j].Add(EffectFactor[i]);
                }
            }
            else
                FactorCards[PlayerGameData.NB_COLOR + 1].Add(EffectFactor[i]);

        }
    }
    /*
    // when a factor is changed for one card change for the other
    private void CalculateFactor()
    {
        for(int i = 0; i < EffectFactor.Count; i++)
        {

        }
    }
    */
}
