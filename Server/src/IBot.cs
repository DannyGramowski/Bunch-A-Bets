namespace Server;

public interface IBot {
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
    void SetEpic(Epic epic);
    void TryStartEpic();
    void Close();

    private Dictionary<string, object> ToDictionary() {
        return new Dictionary<string, object>() {
            {"id", ID},
            {"name", Name},
            {"bank", Bank},
            {"state", GameData.RoundState},
            {"pot_value", GameData.PotValue}
        };
    }

    public static object SerializeBotsList(List<IBot> bots) {
        return bots.Select(bot => bot.ToDictionary()).ToArray();
    }
    
}
