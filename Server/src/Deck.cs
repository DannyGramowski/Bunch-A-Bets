namespace Server;


public class Deck {
    private Stack<Card> _cards = new Stack<Card>();

    public Deck() {
        foreach (string suit in Card.SUIT_VALUES) {
            foreach (string value in Card.CARD_VALUES) {
                _cards.Push(new Card(value, suit));
            }
        }
    }

    public void ShuffleDeck() {
        var tempDeck = _cards.ToList();
        _cards.Clear();
        while (tempDeck.Count > 0) {
            int randomIndex = Random.Shared.Next(0, tempDeck.Count);
            _cards.Push(tempDeck[randomIndex]);
            tempDeck.RemoveAt(randomIndex);
        }
    }

    public Card DrawCard() {
        return _cards.Pop();
    }

    public override string ToString() {
        return $"Deck: [{string.Join(", ", _cards)}]";
    }
}