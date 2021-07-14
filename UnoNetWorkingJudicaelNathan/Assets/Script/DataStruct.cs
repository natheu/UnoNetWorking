using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataStruct
{
    public static void Swap<T>(List<T> list, int indexA, int indexB)
    {
        T tmp = list[indexA];
        list[indexA] = list[indexB];
        list[indexB] = tmp;
    }

    public struct Deck
    {
        List<PlayerGameData.CardType> Cards;
        bool EndShuffle;

        List<int> Effects;
        void CreateDeck(List<List<int>> cards)
        {
            for(int i = 0; i < PlayerGameData.NB_COLOR; i++)
            {
                Effects = cards[i];
                for (int j = 0; j < Effects.Count; j++)
                {
                    int numberInDeck = Effects[j];

                    AddCardinDeck(Effects[j], (PlayerGameData.CardType.Color)i, j);
                }
            }

            Effects = cards[PlayerGameData.NB_COLOR + 1];
            for (int i = 0; i <= 2; i++)
            {
                AddCardinDeck(Effects[i], PlayerGameData.CardType.Color.ANY, i + PlayerGameData.PLUS_FOUR);
            }
        }

        private void AddCardinDeck(int numberInDeck, PlayerGameData.CardType.Color color, int effect)
        {
            while (numberInDeck != 0)
            {
                Cards.Add(new PlayerGameData.CardType(color, effect));
                numberInDeck--;
            }
        }

        void Shuffle(int numberShuffle)
        {
            List<PlayerGameData.CardType> shuffleList = new List<PlayerGameData.CardType>();
            while(numberShuffle != 0)
            {
                //shuffleList.Clear();
                for (int i = 0; i < Cards.Count; i++)
                {
                    int j = Random.Range(0, i);
                    Swap(Cards, i, j);
                }
                //Cards = shuffleList;
            }
        }

    }
}


