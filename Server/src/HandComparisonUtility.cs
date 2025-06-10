using System.Diagnostics;
using System.Linq;

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

public enum HandWinner {
    Player1 = -1,
    Tie = 0,
    Player2 = 1
}

public static class HandComparisonUtility {
    public const int HAND_SIZE = 5;

    public static HandWinner CompareBotHands(IBot b1, IBot b2, List<Card> centerCards) {

        return CompareHands(GetBestHand(b1.GameData.Cards, centerCards), GetBestHand(b2.GameData.Cards, centerCards));
    }

    internal static List<Card> GetBestHand(List<Card> botHand, List<Card> centerCards) {
        var combinations = GetCombinations(botHand.Concat(centerCards).ToList(), HAND_SIZE);
        List<Card> bestHand = new ();

        foreach (var hand in combinations) {
            if (bestHand.Count == 0) {
                bestHand = hand;
                continue;
            }
            var comparedResult = CompareHands(bestHand, hand);
            if (comparedResult == HandWinner.Player2) {
                bestHand = hand;
            }
        }

        return bestHand;
    }

    internal static IEnumerable<List<Card>> GetCombinations(List<Card> list, int k) {
        if (k == 0) {
            yield return new ();
        } else {
            for (int i = 0; i <= list.Count - k; i++) {
                foreach (var tail in GetCombinations(list.Skip(i + 1).ToList(), k - 1)) {
                    var combination = new List<Card> { list[i] };
                    combination.AddRange(tail);
                    yield return OrderCards(combination);
                }
            }
        }
    }

    internal static List<Card> OrderCards(List<Card> cards) {
        return cards.OrderByDescending(c => c.GetNumericValue()).ToList();
    }

    internal static HandWinner CompareHands(List<Card> h1, List<Card> h2) {
        if (h1.Count != 5 || h2.Count != 5) {
            throw new Exception("hands must be 5 cards");
        }

        bool h1straight = HandIsStraight(h1);
        bool h1flush = HandIsFlush(h1);
        bool h1StraightFlush = h1straight & h1flush;
        var h1BestKind = GetBestOfKind(h1);

        bool h2straight = HandIsStraight(h2);
        bool h2flush = HandIsFlush(h2);
        bool h2StraightFlush = h2straight & h2flush;
        var h2BestKind = GetBestOfKind(h2);


        if (h1StraightFlush && h2StraightFlush) {
            return HandleTie(h1, h2);
        } else if (h1StraightFlush) {
            return HandWinner.Player1;
        } else if (h2StraightFlush) {
            return HandWinner.Player2;
        }

        if (h1BestKind.Item1 == HandRank.FourOfKind && h2BestKind.Item1 == HandRank.FourOfKind) {
            return HandleTie(h1, h2);
        } else if (h1BestKind.Item1 == HandRank.FourOfKind) {
            return HandWinner.Player1;
        } else if (h2BestKind.Item1 == HandRank.FourOfKind) {
            return HandWinner.Player2;
        }

        if (h1BestKind.Item1 == HandRank.FullHouse && h2BestKind.Item1 == HandRank.FullHouse) {
            return HandleTie(h1, h2);
        } else if (h1BestKind.Item1 == HandRank.FullHouse) {
            return HandWinner.Player1;
        } else if (h2BestKind.Item1 == HandRank.FullHouse) {
            return HandWinner.Player2;
        }

        if (h1flush && h2flush) {
            return HandleTie(h1, h2);
        } else if (h1flush) {
            return HandWinner.Player1;
        } else if (h2flush) {
            return HandWinner.Player2;
        }

        if (h1straight && h2straight) {
            return HandleTie(h1, h2);
        } else if (h1straight) {
            return HandWinner.Player1;
        } else if (h2straight) {
            return HandWinner.Player2;
        }


        return HandleTie(h1, h2);
    }

    internal static HandWinner HandleTie(List<Card> h1, List<Card> h2) {
        Debug.Assert(h1.Count == h2.Count, $"hand counts must be equal. {h1.Count} != {h2.Count}");
    
        if (h1.Count == 0) {
            return HandWinner.Tie; //A tie
        }

        var h1Best = GetBestOfKind(h1);
        var h2Best = GetBestOfKind(h2);

        if (h1Best.Item1 == h2Best.Item1) {
            if (h1Best.Item2 == h2Best.Item2) {
                //removes the equivilent cards then perform recursion to find next highest card
                var newH1 = h1.Where((Card c) => c.GetNumericValue() != h1Best.Item2).ToList();
                var newH2 = h2.Where((Card c) => c.GetNumericValue() != h2Best.Item2).ToList();
                return HandleTie(newH1, newH2);
            }

            return h1Best.Item2 > h2Best.Item2 ? HandWinner.Player1 : HandWinner.Player2;
        }
        if (h1Best.Item1 > h2Best.Item1) {
            return HandWinner.Player1;
        } else {
            return HandWinner.Player2;
        }

    }

    internal static bool HandIsStraight(List<Card> hand) {
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
                if (previous.Value == 'A' && c.Value == '5') {
                    previous = c;
                    continue; //Given the sorted precondition, doing this allows A-5 to be valid. 
                }
                return false;
            }
            previous = c;

        }

        return true;
    }


    internal static bool HandIsFlush(List<Card> hand) {
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

    internal static (HandRank, int) GetBestOfKind(List<Card> hand) {
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

        //The gist of it is it finds the highest count then
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