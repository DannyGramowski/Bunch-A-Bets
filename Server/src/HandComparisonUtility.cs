namespace Server;


public enum HandRank {
    HighCard,
    OnePair,
    TwoPair,
    ThreeOfKind,
    Straight,
    Flush,
    FullHouse,
    FourOfKind,
    StraightFlush,
    RoyalFlush
}

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

        //handle if 2 players have equal pair
        //handle if 2 players have equal higher pair in 2 pair

        return -1;
    }

    public static bool HandIsStraight(List<Card> hand) {
        //hand must be sorted in descending order of value with Ace high.
        //will still work with Ace-5 though
        if (hand.Count != 5) return false;

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
        if (hand.Count != 5) return false;

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

    public static (HandRank, int) GetBestOfKind(List<Card> hand) {
        //<numericvalue, count> where numeric value is the value returned by card.GetNumericValue()
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
            } else if (pair.Value == highestCount) {//get the highest value pair in the case of a 2 pair or high card
                cardValueOfHighestCount = Math.Max(cardValueOfHighestCount, pair.Key);
            }
        }

        //I understand this is confusing. The gist of it is it finds the highest count then
        //using the total cards in the hand and the number of different values of cards to determine the HandRank
        if (highestCount == 4) {
            return (HandRank.FourOfKind, cardValueOfHighestCount);
        } else if (highestCount == 3) {
            if (count.Count == 2 && hand.Count == 5) {
                //since we know the hand is 5 cards, and the total number of different card values is 2, we know that it must be a full house.
                return (HandRank.FullHouse, cardValueOfHighestCount);
            }
            return (HandRank.ThreeOfKind, cardValueOfHighestCount);
        } else if (highestCount == 2) {
            if ((hand.Count == 5 && count.Count == 3) || (hand.Count == 4 && count.Count == 2)) {
                return (HandRank.TwoPair, cardValueOfHighestCount);
            }
            return (HandRank.OnePair, cardValueOfHighestCount);
        }
        return (HandRank.HighCard, cardValueOfHighestCount);
    }
}