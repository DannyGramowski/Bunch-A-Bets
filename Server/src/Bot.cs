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
        Bank = startingBank;
        Console.WriteLine($"Bot ID: {_id}, Bot Port: {port}, Bot Name: {_name}");
    }

    /// <summary>
    /// Subtracts the amount if able to. Otherwise marks the bot as all in and sets bank to 0
    /// </summary>
    /// <param name="amount"></param>
    /// <returns>
    ///     True if the entire went through. False if they went all in. To get the amount they went all in, check GameData.PotValue
    /// </returns>
    public bool Bet(int amount) {
        if (amount > Bank) {
            amount = Bank;
            Bank = 0;
            _gameData.PotValue = amount;
            _gameData.RoundState = BotRoundState.AllIn;
            
            return false;
        }

        Bank -= amount;
        _gameData.PotValue = amount;

        return true;
    }

    public void SendMessage(Dictionary<string, string> message) {
        _socket.SendMessage(message);
    }

    public Dictionary<string, string> ReceiveMessage() {
        return _socket.ReadMessage();
    }

    public Dictionary<string, string> ReceiveMessageBlocking() {
        Dictionary<string, string> message = new Dictionary<string, string>();
        while (message.Count == 0) {
            Thread.Sleep(10);
            message = _socket.ReceiveMessage();
        }
        return message;
    }
    
    public bool HasMessageReceived() => _socket.HasMessageReceived();


    public Dictionary<string, string> ToDictionary() {
        return new Dictionary<string, string>() {
            {"id", ID.ToString()},
            {"name", _name},
            {"bank", Bank.ToString()},
            {"state", ((int)_gameData.RoundState).ToString()},
            {"pot_value", _gameData.PotValue.ToString()}
        };
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
    
    
    public static string SerializeBotsList(List<Bot> bots) {
        return JsonSerializer.Serialize(bots.Select(bot => bot.ToDictionary()).ToArray());
    }
}