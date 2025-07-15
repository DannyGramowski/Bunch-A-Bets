using System.Text.Json;

namespace Server;

public class Card {
    public static readonly char[] CARD_VALUES = { '2', '3', '4', '5', '6', '7', '8', '9', 'T', 'J', 'Q', 'K', 'A' };
    public static readonly char[] SUIT_VALUES = { 'C', 'S', 'D', 'H' };

    public static readonly Dictionary<char, string> CARD_VALUES_MAPPING = new Dictionary<char, string> { { '2', "2" }, { '3', "3" }, { '4', "4"}, { '5', "5" }, { '6', "6" }, { '7', "7" }, { '8', "8" }, { '9', "9" }, { 'T', "10" }, { 'J', "Jack" }, { 'Q', "Queen" }, { 'K', "King" }, { 'A', "Ace" } };
    public static readonly Dictionary<char, string> SUIT_VALUES_MAPPING = new Dictionary<char, string> { { 'C', "Clubs" }, { 'S', "Spades" }, { 'D', "Diamonds" }, { 'H', "Hearts" } };

    public readonly char Value;
    public readonly char Suit;

    public Card(char value, char suit) {
        Value = value;
        Suit = suit;
    }

    public override string ToString() {
        return "(" + CARD_VALUES_MAPPING[Value] + ":" + SUIT_VALUES_MAPPING[Suit] + ")";
    }

    public override bool Equals(object? obj) {
        Card? other = obj as Card;
        return other != null && this.Value == other.Value && this.Suit == other.Suit;
    }

    public Dictionary<string, object> ToDictionary() {
        return new Dictionary<string, object>() { { "value", Value }, { "suit", Suit } };
    }

    public int GetNumericValue() {
        return Array.IndexOf(CARD_VALUES, Value) + 2;
    }

    public static List<Dictionary<string, object>> SerializeCardList(List<Card> cards) {
        if (cards == null)
        {
            return new List<Dictionary<string, object>>();
        }
        return cards.Select(card => card.ToDictionary()).ToList();
    }
}
