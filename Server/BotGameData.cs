namespace Server;

public struct BotGameData {
    public List<Card> Cards;
    public BotRoundState RoundState;
    public float PotValue;

    public void NewHand(List<Card> cards) {
        if(cards.Count != 2) Console.Error.WriteLine("Invalid number of cards");
        Cards = cards;
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