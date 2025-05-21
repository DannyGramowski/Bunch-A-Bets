namespace Server.Tests;

using Xunit;
using Server;

public class HandComparisonTes {

    private List<Card> make((string, string)[] values) {
        //(value, suit)
        List<Card> result = new();
        foreach (var pair in values)
        {
            result.Add(new Card(pair.Item1, pair.Item2));
        }

        return result;
    }

    [Fact]
    public void TestFlush() {
        List<Card> flush = make([("2", "C"), ("3", "C"), ("4", "C"), ("5", "C"), ("10", "C")]);
        List<Card> notFlush = make([("2", "C"), ("3", "S"), ("4", "C"), ("5", "C"), ("10", "C")]);
        Assert.True(HandComparisonUtility.HandIsFlush(flush));
        Assert.True(!HandComparisonUtility.HandIsFlush(notFlush));
    }

    [Fact]
    public void TestStraight() {
        //test Ace on both sides

        List<Card> straight = make([("2", "C"), ("3", "C"), ("4", "C"), ("5", "C"), ("6", "C")]);
        List<Card> straightAceFront = make([("A", "C"), ("2", "C"), ("3", "C"), ("4", "C"), ("5", "C")]);
        List<Card> straightAceBack = make([("10", "C"), ("J", "C"), ("Q", "C"), ("K", "C"), ("A", "C")]);
        List<Card> notStraight = make([("2", "C"), ("3", "S"), ("4", "C"), ("5", "C"), ("10", "C")]);
        List<Card> notStraightSameValue = make([("2", "C"), ("3", "S"), ("3", "C"), ("4", "C"), ("5", "C")]);

        Assert.True(HandComparisonUtility.HandIsFlush(straight));
        Assert.True(HandComparisonUtility.HandIsFlush(straightAceFront));
        Assert.True(HandComparisonUtility.HandIsFlush(straightAceBack));
        Assert.True(!HandComparisonUtility.HandIsFlush(notStraight));
        Assert.True(!HandComparisonUtility.HandIsFlush(notStraightSameValue));
    }

    [Fact]
    public void TestGetBestOfKind() {
        List<Card> four = make([("6", "C"), ("2", "C"), ("2", "C"), ("2", "C"), ("2", "C")]);
        List<Card> fullHouse = make([("3", "C"), ("3", "C"), ("2", "C"), ("2", "C"), ("2", "C")]);
        List<Card> threeKind = make([("2", "C"), ("2", "C"), ("2", "C"), ("3", "C"), ("4", "C")]);
        List<Card> twoPair = make([("6", "C"), ("6", "C"), ("3", "C"), ("3", "C"), ("4", "C")]);
        List<Card> pair = make([("2", "C"), ("2", "C"), ("6", "C"), ("3", "C"), ("4", "C")]);
        List<Card> highCard = make([("7", "C"), ("2", "C"), ("6", "C"), ("3", "C"), ("4", "C")]);

        Assert.Equal(HandComparisonUtility.GetBestOfKind(four), (HandRank.FourOfKind, 2));
        Assert.Equal(HandComparisonUtility.GetBestOfKind(fullHouse), (HandRank.FullHouse, 2));
        Assert.Equal(HandComparisonUtility.GetBestOfKind(threeKind), (HandRank.ThreeOfKind, 2));
        Assert.Equal(HandComparisonUtility.GetBestOfKind(twoPair), (HandRank.TwoPair, 6));
        Assert.Equal(HandComparisonUtility.GetBestOfKind(pair), (HandRank.OnePair, 2));
        Assert.Equal(HandComparisonUtility.GetBestOfKind(highCard), (HandRank.HighCard, 7));
    }
}