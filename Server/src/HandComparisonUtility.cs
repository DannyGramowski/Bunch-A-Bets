namespace Server;


public static class HandComparisonUtility {
    public static int CompareBotHands(Bot b1, Bot b2, List<Card> centerCards) {

        return -1;
    }

    public static int CompareHands(List<Card> hand1, List<Card> hand2) {
        hand1 = hand1.OrderByDescending(c => c.GetNumericValue()).ToList();
        hand2 = hand2.OrderByDescending(c => c.GetNumericValue()).ToList();

        if (hand1.Count != 5 || hand2.Count != 5) {
            throw new Exception("hands must be 5 cards");
        }

        //TODO handle full house

        return -1;
    }

    public static bool HandIsStraight(List<Card> hand) {
        //hand must be sorted in descending order of value with Ace high.
        //will still work with Ace-5 though
        Card? previous = null;

        foreach (Card c in hand) {
            if (previous == null) {
                previous = c;
                continue;
            }

            if (previous.GetNumericValue() - 1 != c.GetNumericValue()) {
                if (previous.Value == "A" && c.Value == "5") continue; //Given the sorted precondition, doing this allows A-5 to be valid. 
                return false;
            }
        }

        return true;
    }

    public static bool HandIsFlush(List<Card> hand) {
        Card? previous = null;

        foreach (Card c in hand) {
            if (previous == null) {
                previous = c;
                continue;
            }

            if (previous.Suit != c.Suit) return false;
        }

        return true;
    }

    //(int, int) (numericvalue, count) where numeric value is the value returned by card.GetNumericValue()
    public static (int, int) GetBestOfKind(List<Card> hand) {
        Dictionary<int, int> count = new();

        foreach (Card c in hand) {
            var value = c.GetNumericValue();
            if (count.ContainsKey(value)) {
                count[value]++;
            } else {
                count[value] = 1;
            }
        }

        int highestCount = 0;
        int cardValueOfHighestCount = 0;
        foreach (var pair in count) {
            if (pair.Value > highestCount) {
                highestCount = pair.Value;
                cardValueOfHighestCount = pair.Key;
            }
        }

        return (cardValueOfHighestCount, highestCount);
    }
}