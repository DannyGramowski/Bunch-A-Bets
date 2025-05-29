using System.Text.Json;

namespace Server;

public class Bot {
    public int ID => _id;
    public string Name => _name;
    public BotGameData GameData => _gameData;

    public int Bank;

    private BotSocket _socket;

    private int _id; //sequential value set based on number of players registered. Starts at 1.
    private string _name;
    private BotGameData _gameData;
    public DateTime lastChatTime;

    public Bot(int id, int port, string name, int startingBank) {
        _socket = new BotSocket(port);
        _id = id;
        _name = name;
        _gameData = new BotGameData();
        Bank = startingBank;
        Console.WriteLine($"Bot ID: {_id}, Bot Port: {port}, Bot Name: {_name}");
    }

    /// <summary>
    /// Subtracts the amount if able to. Otherwise marks the bot as all in and sets bank to 0
    /// </summary>
    /// <param name="amount"></param>
    /// <returns>
    ///     The amount that was bet. If this is less than inputted, they went all in. To get the amount they went all in, check GameData.PotValue
    /// </returns>
    public int Bet(int amount) {
        if (amount > Bank) {
            amount = Bank;
            Bank = 0;
            _gameData.PotValue += amount;
            _gameData.PotValueOfHand += amount;
            _gameData.RoundState = BotRoundState.AllIn;
            
            return amount;
        }

        Bank -= amount;
        _gameData.PotValue += amount;
        _gameData.PotValueOfHand += amount;

        return amount;
    }

    public void SendMessage(Dictionary<string, object> message) {
        _socket.SendMessage(message);
    }

    public Dictionary<string, object> ReceiveMessage() {
        return _socket.ReadMessage();
    }

    public List<Dictionary<string, object>> ReceiveMessageBlocking() {
        List<Dictionary<string, object>> message = new List<Dictionary<string, object>>();
        while (message.Count == 0) {
            Thread.Sleep(10);
            message = _socket.ReceiveMessage();
        }
        return message;
    }

    public bool HasMessageReceived() => _socket.HasMessageReceived();


    public Dictionary<string, object> ToDictionary() {
        return new Dictionary<string, object>() {
            {"id", ID},
            {"name", _name},
            {"bank", Bank},
            {"state", _gameData.RoundState},
            {"pot_value", _gameData.PotValue}
        };
    }

    public override string ToString() {
        return $"ID: {ID}, Name: {_name}, Bank: {Bank}, Cards: {string.Join(",",GameData.Cards)}";
    }

    public override int GetHashCode() {
        return ID;
    }

    public override bool Equals(object? obj) {
        if (obj is Bot other) {
            return other.ID == ID;
        }
        return false;
    }
    
    public static object SerializeBotsList(List<Bot> bots) {
        return bots.Select(bot => bot.ToDictionary()).ToArray();
    }
}