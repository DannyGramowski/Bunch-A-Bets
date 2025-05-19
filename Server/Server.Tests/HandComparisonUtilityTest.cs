namespace Server.Tests;

using Xunit;
using Server;

public class HandComparisonTest {

    private List<Card> make((string, string)[] values) {
        //(value, suit)
        List<Card> result = new();
        foreach (var pair in values) {
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
}