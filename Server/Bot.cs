using System.Text.Json;

namespace Server;

public class Bot {
    public int ID => _id;
    public string Name => _name;
    public BotGameData GameData => _gameData;
    
    
    private BotSocket _socket;
    
    private int _id; //sequential value set based on number of players registered. Starts at 1.
    private string _name;
    private float _bank;
    private BotGameData _gameData;
    public DateTime lastChatTime;
    
    public Bot(int id, int port, string name, float startingBank)
    {
        _socket = new BotSocket(port);
        _id = id;
        _name = name;
        Console.WriteLine($"Bot ID: {_id}, Bot Port: {port}, Bot Name: {_name}");
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


    public Dictionary<string, string> ToDictionary()
    {
        return new Dictionary<string, string>() {
            {"id", ID.ToString()},
            {"name", _name},
            {"bank", _bank.ToString()},
            {"state", ((int)_gameData.RoundState).ToString()},
            {"pot_value", _gameData.PotValue.ToString()}
        };
    }
    
    
    public static string SerializeBotsList(List<Bot> bots) {
        return JsonSerializer.Serialize(bots.Select(bot => bot.ToDictionary()).ToArray());
    }
}