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

        public Deck(int shuffle)
        {
            Cards = new List<PlayerGameData.CardType>();
            EndShuffle = false;
            Effects = new List<int>();
        }

        public void CreateDeck(List<List<int>> cards)
        {
            for(int i = 0; i < PlayerGameData.NB_COLOR; i++)
            {
                Effects = cards[i];
                for (int j = 0; j < Effects.Count; j++)
                {
                    int numberInDeck = Effects[j];

                    AddCardinDeck(Effects[j], (PlayerGameData.CardType.Color)i + 1, j);
                }
            }

            Effects = cards[PlayerGameData.NB_COLOR];
            for (int i = 0; i < 2; i++)
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

        public void Shuffle(int numberShuffle)
        {
            EndShuffle = false;
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
                numberShuffle--;
            }

            EndShuffle = true;
        }

        public PlayerGameData.CardType GetNextCard()
        {
            if (Cards.Count < 0)
                return new PlayerGameData.CardType();

            PlayerGameData.CardType tmpCard = Cards[0];

            Cards.RemoveAt(0);

            return tmpCard;
        }

    }
}


