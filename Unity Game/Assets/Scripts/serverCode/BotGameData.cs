using System;
using System.Collections.Generic;

namespace Server
{

    public class BotGameData
    {
        public List<Card> Cards;
        public BotRoundState RoundState;
        public int PotValue;
        public int PotValueOfHand; //Can only earn up to this amount per player in all ins
        public int PotValueOfHandCache = 0;

        public BotGameData() { }

        public BotGameData(List<Card> cards, BotRoundState roundState, int potValue, int potValueOfHand)
        {
            Cards = cards;
            RoundState = roundState;
            PotValue = potValue;
            PotValueOfHand = potValueOfHand;
        }

        public void NewHand(List<Card> cards)
        {
            if (cards.Count != 2) Console.Error.WriteLine("Invalid number of cards");
            RoundState = BotRoundState.NotPlayed; // Necessary since RoundState is not always reset in NewRound
            Cards = cards;
            PotValueOfHand = 0;
            NewRound();
        }

        public void NewRound()
        {
            PotValue = 0;
            if (StillBidding())
            {
                RoundState = BotRoundState.NotPlayed;
            }
        }

        public bool StillBidding()
        {
            return RoundState != BotRoundState.Folded && RoundState != BotRoundState.AllIn;
        }
    }

}