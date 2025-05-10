namespace Server ;

public class Card {
    public static readonly string[] CARD_VALUES = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" };
    public static readonly string[] SUIT_VALUES = { "C", "S", "D", "H"}; 
    
    public static readonly Dictionary<string, string> CARD_VALUES_MAPPING = new Dictionary<string, string>{ {"2", "2"}, {"3", "3"}, {"4", "4"}, {"5", "5"}, {"6", "6"}, {"7", "7"}, {"8", "8"}, {"9", "9"}, {"10", "10"}, {"J", "Jack"}, {"Q", "Queen"}, {"K", "King"}, {"A", "Ace"} };
    public static readonly Dictionary<string, string> SUIT_VALUES_MAPPING = new Dictionary<string, string>{ {"C", "Clubs"}, {"S", "Spades"}, {"D", "Diamonds"}, {"H", "Hearts"}}; 
    
    public string Value => _value;  
    public string Suit => _suit;

    private string _value;
    private string _suit;

    public Card(string value, string suit) {
        this._value = value;
        this._suit = suit;
    }

    public override string ToString() {
        return "(" + CARD_VALUES_MAPPING[_value] + ":" + SUIT_VALUES_MAPPING[_suit] + ")";
    }
}
