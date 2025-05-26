namespace Server.Tests;

using Xunit;
using Server;

public class HandComparisonTes {

    private List<Card> make((string, string)[] values) {
        //(value, suit)
        List<Card> result = new();
        foreach (var pair in values) {
            result.Add(new Card(pair.Item1[0], pair.Item2[0]));
        }

        return HandComparisonUtility.OrderCards(result);
    }

    [Fact]
    public void TestFlush() {
        List<Card> flush = make([("2", "C"), ("3", "C"), ("4", "C"), ("5", "C"), ("T", "C")]);
        List<Card> notFlush = make([("2", "C"), ("3", "S"), ("4", "C"), ("5", "C"), ("T", "C")]);
        Assert.True(HandComparisonUtility.HandIsFlush(flush));
        Assert.True(!HandComparisonUtility.HandIsFlush(notFlush));
    }

    [Fact]
    public void TestStraight() {
        //test Ace on both sides

        List<Card> straight = make([("2", "C"), ("3", "C"), ("4", "C"), ("5", "C"), ("6", "C")]);
        List<Card> straightAceFront = make([("A", "C"), ("2", "C"), ("3", "C"), ("4", "C"), ("5", "C")]);
        List<Card> straightAceBack = make([("T", "C"), ("J", "C"), ("Q", "C"), ("K", "C"), ("A", "C")]);
        List<Card> notStraight = make([("2", "C"), ("3", "S"), ("4", "C"), ("5", "C"), ("T", "C")]);
        List<Card> notStraightSameValue = make([("2", "C"), ("3", "S"), ("3", "C"), ("4", "C"), ("5", "C")]);

        Assert.True(HandComparisonUtility.HandIsStraight(straight));
        Assert.True(HandComparisonUtility.HandIsStraight(straightAceFront));
        Assert.True(HandComparisonUtility.HandIsStraight(straightAceBack));
        Assert.True(!HandComparisonUtility.HandIsStraight(notStraight));
        Assert.True(!HandComparisonUtility.HandIsStraight(notStraightSameValue));
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

    [Fact]
    public void TestHandleTie() {
        List<Card> pair1 = make([("6", "C"), ("6", "C"), ("2", "C"), ("4", "C"), ("5", "C")]);
        List<Card> pair2 = make([("6", "C"), ("6", "C"), ("3", "C"), ("7", "C"), ("9", "C")]);
        List<Card> twoPair1 = make([("6", "C"), ("6", "C"), ("2", "C"), ("2", "C"), ("9", "C")]);
        List<Card> twoPair2 = make([("6", "C"), ("6", "C"), ("2", "C"), ("2", "C"), ("8", "C")]);
        List<Card> allButLast1 = make([("2", "C"), ("2", "C"), ("2", "C"), ("2", "C"), ("3", "C")]);
        List<Card> allButLast2 = make([("2", "C"), ("2", "C"), ("2", "C"), ("2", "C"), ("4", "C")]);
        List<Card> tie1 = make([("6", "C"), ("2", "C"), ("2", "C"), ("2", "C"), ("2", "C")]);
        List<Card> tie2 = make([("6", "C"), ("2", "C"), ("2", "C"), ("2", "C"), ("2", "C")]);

        Assert.Equal(HandWinner.Player2, HandComparisonUtility.HandleTie(pair1, pair2));
        Assert.Equal(HandWinner.Player1, HandComparisonUtility.HandleTie(twoPair1, twoPair2));
        Assert.Equal(HandWinner.Player2, HandComparisonUtility.HandleTie(allButLast1, allButLast2));
        Assert.Equal(HandWinner.Tie, HandComparisonUtility.HandleTie(tie1, tie2));
        
    }

    [Fact]
    public void TestCompareHands() {
        List<Card> royalflush = make([("A", "C"), ("K", "C"), ("Q", "C"), ("J", "C"), ("T", "C")]);
        List<Card> straightflush = make([("T", "C"), ("9", "C"), ("8", "C"), ("7", "C"), ("6", "C")]);
        List<Card> straightflushhigher = make([("T", "C"), ("9", "C"), ("8", "C"), ("7", "C"), ("J", "C")]);
        List<Card> four = make([("T", "C"), ("T", "C"), ("T", "C"), ("T", "C"), ("4", "C")]);
        List<Card> fourhigher = make([("T", "C"), ("T", "C"), ("T", "C"), ("T", "C"), ("8", "C")]);
        List<Card> fullhouse = make([("T", "C"), ("T", "C"), ("T", "C"), ("8", "C"), ("8", "C")]);
        List<Card> fullhousehigher = make([("T", "C"), ("T", "C"), ("T", "C"), ("9", "C"), ("9", "C")]);
        List<Card> flush = make([("K", "C"), ("6", "C"), ("4", "C"), ("8", "C"), ("J", "C")]);
        List<Card> flushhigher = make([("A", "C"), ("6", "C"), ("4", "C"), ("8", "C"), ("J", "C")]);
        List<Card> straight = make([("T", "C"), ("9", "C"), ("8", "C"), ("7", "C"), ("6", "C")]);
        List<Card> straighthigher = make([("T", "C"), ("9", "C"), ("8", "C"), ("7", "C"), ("J", "C")]);
        List<Card> three = make([("T", "C"), ("T", "C"), ("T", "C"), ("7", "C"), ("4", "C")]);
        List<Card> threehigher = make([("T", "C"), ("T", "C"), ("T", "C"), ("7", "C"), ("J", "C")]);
        List<Card> twopair = make([("T", "C"), ("T", "C"), ("7", "C"), ("7", "C"), ("4", "C")]);
        List<Card> twopairhigherhighcard = make([("T", "C"), ("T", "C"), ("7", "C"), ("7", "C"), ("J", "C")]);
        List<Card> twopairhigherpair = make([("T", "C"), ("T", "C"), ("8", "C"), ("8", "C"), ("J", "C")]);
        List<Card> pair = make([("T", "C"), ("T", "C"), ("8", "C"), ("4", "C"), ("3", "C")]);
        List<Card> pairhigher = make([("T", "C"), ("T", "C"), ("8", "C"), ("4", "C"), ("5", "C")]);
        List<Card> highcard = make([("J", "C"), ("T", "C"), ("8", "C"), ("4", "C"), ("5", "C")]);
        List<Card> highcardhigher = make([("A", "C"), ("T", "C"), ("8", "C"), ("4", "C"), ("5", "C")]);


        Assert.Equal(HandWinner.Tie, HandComparisonUtility.HandleTie(royalflush, royalflush));
        Assert.Equal(HandWinner.Player1, HandComparisonUtility.HandleTie(royalflush, straightflush));
        Assert.Equal(HandWinner.Player2, HandComparisonUtility.HandleTie(straightflush, straightflushhigher));

        Assert.Equal(HandWinner.Tie, HandComparisonUtility.HandleTie(four, four));
        Assert.Equal(HandWinner.Player2, HandComparisonUtility.HandleTie(four, fourhigher));

        Assert.Equal(HandWinner.Player1, HandComparisonUtility.HandleTie(four, fullhouse));
        Assert.Equal(HandWinner.Tie, HandComparisonUtility.HandleTie(fullhouse, fullhouse));
        Assert.Equal(HandWinner.Player2, HandComparisonUtility.HandleTie(fullhouse, fullhousehigher));

        Assert.Equal(HandWinner.Player1, HandComparisonUtility.HandleTie(fullhouse, flush));
        Assert.Equal(HandWinner.Tie, HandComparisonUtility.HandleTie(flush, flush));
        Assert.Equal(HandWinner.Player2, HandComparisonUtility.HandleTie(flush, flushhigher));

        Assert.Equal(HandWinner.Tie, HandComparisonUtility.HandleTie(straight, straight));
        Assert.Equal(HandWinner.Player2, HandComparisonUtility.HandleTie(straight, straighthigher));

        Assert.Equal(HandWinner.Tie, HandComparisonUtility.HandleTie(three, three));
        Assert.Equal(HandWinner.Player2, HandComparisonUtility.HandleTie(three, threehigher));
        Assert.Equal(HandWinner.Player1, HandComparisonUtility.HandleTie(fullhouse, threehigher));

        Assert.Equal(HandWinner.Player1, HandComparisonUtility.HandleTie(three, twopairhigherpair));
        Assert.Equal(HandWinner.Tie, HandComparisonUtility.HandleTie(twopairhigherpair, twopairhigherpair));
        Assert.Equal(HandWinner.Player2, HandComparisonUtility.HandleTie(twopair, twopairhigherpair));
        Assert.Equal(HandWinner.Player2, HandComparisonUtility.HandleTie(twopairhigherhighcard, twopairhigherpair));
        Assert.Equal(HandWinner.Player1, HandComparisonUtility.HandleTie(twopairhigherhighcard, twopair));

        Assert.Equal(HandWinner.Player2, HandComparisonUtility.HandleTie(pair, twopair));
        Assert.Equal(HandWinner.Tie, HandComparisonUtility.HandleTie(pair, pair));
        Assert.Equal(HandWinner.Player2, HandComparisonUtility.HandleTie(pair, pairhigher));

        Assert.Equal(HandWinner.Player2, HandComparisonUtility.HandleTie(highcard, pairhigher));
        Assert.Equal(HandWinner.Tie, HandComparisonUtility.HandleTie(highcard, highcard));
        Assert.Equal(HandWinner.Player1, HandComparisonUtility.HandleTie(highcardhigher, highcard));
    }


    [Fact]
    public void TestGetBestHand() {
        List<Card> cards1 = make([("J", "C"), ("T", "C"), ("8", "C"), ("7", "C"), ("5", "C")]);
        List<Card> cards2 = make([("J", "D"), ("T", "S"), ("9", "C"), ("7", "C"), ("4", "C")]);

        List<Card> h1 = make([("9", "H"), ("T", "C")]);
        List<Card> h2 = make([("9", "H"), ("9", "C")]);

        Assert.Equal(make([("J", "C"), ("T", "C"), ("8", "C"), ("7", "C"), ("T", "C")]), HandComparisonUtility.GetBestHand(h1, cards1));
        Assert.Equal(make([("J", "C"), ("T", "C"), ("8", "C"), ("7", "C"), ("9", "C")]), HandComparisonUtility.GetBestHand(h2, cards1));
        Assert.Equal(make([("T", "C"), ("J", "D"), ("T", "S"), ("9", "H"), ("9", "C")]), HandComparisonUtility.GetBestHand(h1, cards2));



    }

    [Fact]
    public void TestCompareBotHands() {
        

    }
}