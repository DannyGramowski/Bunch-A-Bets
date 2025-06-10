namespace Server;

public class BotGameData {
    public List<Card> Cards;
    public BotRoundState RoundState;
    public int PotValue;
    public int PotValueOfHand; //Can only earn up to this amount per player in all ins

    public BotGameData() { }

    public BotGameData(List<Card> cards, BotRoundState roundState, int potValue, int potValueOfHand) {
        Cards = cards;
        RoundState = roundState;
        PotValue = potValue;
        PotValueOfHand = potValueOfHand;
    }

    public void NewHand(List<Card> cards)
    {
        if (cards.Count != 2) Console.Error.WriteLine("Invalid number of cards");
        Cards = cards;
        PotValueOfHand = 0;
        NewRound();
    }

    public void NewRound() {
        if (!StillBidding()) return;
        RoundState = BotRoundState.NotPlayed;
        PotValue = 0;
    }

    public bool StillBidding() {
        return RoundState != BotRoundState.Folded && RoundState != BotRoundState.AllIn;
    }
}