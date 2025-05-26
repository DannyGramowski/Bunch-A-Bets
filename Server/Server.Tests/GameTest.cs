namespace Server.Tests;

using Xunit;
using Server;

public class GameTest {

    private List<Card> Make(string values) {
        //(value, suit)
        List<Card> result = new();
        foreach (var pair in values.Split(" ")) {
            result.Add(new Card(pair[0], pair[1]));
        }

        return result;
    }

    private static int id = 1;
    private Bot MakeBot(string str, int bank=500, int potValue = 0, int handPotValue = 0, BotRoundState roundState = BotRoundState.Called) {
        Bot bot = new Bot(id, -1, id.ToString(), 500);
        id++;

        var data = bot.GameData;
        data.Cards = Make(str);
        data.PotValue = potValue;
        data.PotValueOfHand = handPotValue;
        data.RoundState = roundState;
        bot.Bank = bank;

        return bot;
    }

    private List<Bot> MakeManyBots(string[] strs, int bank=500, int potValue = 0, int handPotValue = 0, BotRoundState roundState = BotRoundState.Called) {
        List<Bot> bots = new();
        foreach (string str in strs) {
            bots.Add(MakeBot(str, bank, potValue, handPotValue, roundState));
        }
        return bots;
    }

    private Bot GetBotByStringCards(List<Bot> bots, String str) {
        Card c1 = new Card(str[0], str[1]);
        Card c2 = new Card(str[3], str[4]);

        foreach (Bot bot in bots) {
            if(bot.GameData.Cards.Contains(c1) && bot.GameData.Cards.Contains(c1)) {
                return bot;
            }
        }

        throw new Exception("Bot not found");
    }

    [Fact]
    public void TestBidding() {
        Bot bot = MakeBot("AC AD");
        bot.Bet(100);

        Assert.Equal(400, bot.Bank);
        Assert.Equal(100, bot.GameData.PotValue);
        Assert.Equal(100, bot.GameData.PotValueOfHand);

        bot.GameData.NewRound();
        Assert.Equal(0, bot.GameData.PotValue);
        Assert.Equal(100, bot.GameData.PotValueOfHand);

        bot.GameData.NewHand(new());
        Assert.Equal(0, bot.GameData.PotValueOfHand);

        bot.Bet(500);
        Assert.Equal(0, bot.Bank);
        Assert.Equal(400, bot.GameData.PotValue);
        Assert.Equal(400, bot.GameData.PotValueOfHand);
        Assert.Equal(BotRoundState.AllIn, bot.GameData.RoundState);
    }

    [Fact]
    public void TestRound() {

    }

    [Fact]
    public void TestShowdown() {
        List<Card> communityCards = Make("AD AS QH JS 7S");
        Bot winningBot;

        {
            List<Bot> allFolded = MakeManyBots(["2C 3H", "2H 3C", "5S 4C", "AH AC", "6D 5H", "7C 5C"], handPotValue: 500);
            foreach (Bot bot in allFolded) {
                bot.GameData.RoundState = BotRoundState.Folded;
                bot.GameData.RoundState = BotRoundState.Folded;
            }
            Game.HandleShowdown(allFolded, communityCards, 500);
            winningBot = GetBotByStringCards(allFolded, "AH AC");
            foreach (Bot bot in allFolded) {
                if (bot.Equals(winningBot)) {
                    Assert.Equal(1000, bot.Bank);
                } else {
                    Assert.Equal(500, bot.Bank);
                }
            }
        }

        {
            List<Bot> mostFolded = MakeManyBots(["2C 3H", "2H 3C", "5S 4C", "AH AC", "6D 5H", "7C 5C"], handPotValue: 500);
            for (int i = 1; i < mostFolded.Count; i++) {
                mostFolded[i].GameData.RoundState = BotRoundState.Folded;
            }

            Game.HandleShowdown(mostFolded, communityCards, 500);
            winningBot = GetBotByStringCards(mostFolded, "2C 3H");
            foreach (Bot bot in mostFolded) {
                if (bot.Equals(winningBot)) {
                    Assert.Equal(1000, bot.Bank);
                } else {
                    Assert.Equal(500, bot.Bank);
                }
            }
        }

        {
            List<Bot> tie = MakeManyBots(["2C 3H", "2H 3C", "5S 4C", "AH KC", "AC KH", "7C 5C"], handPotValue: 500);
            Game.HandleShowdown(tie, communityCards, 500);
            Bot winningBot1 = GetBotByStringCards(tie, "AH KC");
            Bot winningBot2 = GetBotByStringCards(tie, "AC KH");

            foreach (Bot bot in tie) {
                if (bot.Equals(winningBot1) || bot.Equals(winningBot2)) {
                    Assert.Equal(750, bot.Bank);
                } else {
                    Assert.Equal(500, bot.Bank);
                }
            }
        }

        {
            List<Bot> twoAllIn = MakeManyBots(["2C 3H", "2H 3C", "5S 4C", "AH KC", "AC KH", "7C 5C"], handPotValue: 500);
            Bot allInBot1 = GetBotByStringCards(twoAllIn, "AH KC");
            allInBot1.GameData.RoundState = BotRoundState.AllIn;
            allInBot1.GameData.PotValueOfHand = 750;
            Bot allInBot2 = GetBotByStringCards(twoAllIn, "AC KH");
            allInBot2.GameData.RoundState = BotRoundState.AllIn;
            allInBot2.GameData.PotValueOfHand = 250;
            Game.HandleShowdown(twoAllIn, communityCards, 1000);

            foreach (Bot bot in twoAllIn) {
                if (bot.Equals(allInBot1)) {
                    Assert.Equal(1250, bot.Bank);
                } else if (bot.Equals(allInBot2)) {
                    Assert.Equal(750, bot.Bank);
                } else {
                    Assert.Equal(500, bot.Bank);
                }
            }
        }   
    }


}