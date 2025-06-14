namespace Server;

public interface IBot
{
    int ID { get; }
    string Name { get; }
    int Bank { get; set; }
    BotGameData GameData { get; }
    DateTime LastChatTime { get; set; }


    int Bet(int amount);
    int GetHashCode();
    bool HasMessageReceived();
    Dictionary<string, object> ReceiveMessage();
    void SendMessage(Dictionary<string, object> message);
    // void SetEpic(Epic epic);
    // void TryStartEpic();
    void Close();

    public Dictionary<string, object> ToDictionary(bool showHand)
    {
        int potValueOfHand = GameData.PotValueOfHand;
        if (potValueOfHand == 0)
        {
            potValueOfHand = GameData.PotValueOfHandCache; // This allows us to keep the hand value for winners in Showdown
        }
        Dictionary<string, object> result = new Dictionary<string, object>() {
            {"id", ID},
            {"name", Name},
            {"bank", Bank},
            {"state", BotRoundStateExtensions.ToRoundStateString(GameData.RoundState)},
            {"round_bet", GameData.PotValue},
            {"hand_bet", potValueOfHand}
        };
        if (showHand)
        {
            result["hand"] = Card.SerializeCardList(GameData.Cards);
        }
        return result;
    }

    public static object SerializeBotsList(List<IBot> bots, bool showHand)
    {
        return bots.Select(bot => bot.ToDictionary(showHand)).ToArray();
    }

    public void SetEpic(Epic epic) { }

    public void CacheHandBet()
    {
        GameData.PotValueOfHandCache = GameData.PotValueOfHand;
    }
    
}
