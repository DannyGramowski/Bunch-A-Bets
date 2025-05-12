using System.Text.Json;

namespace Server ;

public class Card {
    public static readonly string[] CARD_VALUES = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" };
    public static readonly string[] SUIT_VALUES = { "C", "S", "D", "H"}; 
    
    public static readonly Dictionary<string, string> CARD_VALUES_MAPPING = new Dictionary<string, string>{ {"2", "2"}, {"3", "3"}, {"4", "4"}, {"5", "5"}, {"6", "6"}, {"7", "7"}, {"8", "8"}, {"9", "9"}, {"10", "10"}, {"J", "Jack"}, {"Q", "Queen"}, {"K", "King"}, {"A", "Ace"} };
    public static readonly Dictionary<string, string> SUIT_VALUES_MAPPING = new Dictionary<string, string>{ {"C", "Clubs"}, {"S", "Spades"}, {"D", "Diamonds"}, {"H", "Hearts"}}; 
    
    public readonly string Value;  
    public readonly string Suit;

    public Card(string value, string suit) {
        Value = value;
        Suit = suit;
    }

    public override string ToString() {
        return "(" + CARD_VALUES_MAPPING[Value] + ":" + SUIT_VALUES_MAPPING[Suit] + ")";
    }

    public Dictionary<string, string> ToDictionary() {
        return new Dictionary<string, string>() {{"value", Value}, {"suit", Suit}};
    }

    public static string SerializeCardList(List<Card> cards) {
        return JsonSerializer.Serialize(cards.Select(card => card.ToDictionary()).ToArray());
    }
}
