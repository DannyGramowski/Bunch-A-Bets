namespace Server;

public class Bot {
    public int ID => _id;
    public string Name => _name;
    
    private BotSocket _socket;
    private int _id; //sequential value set based on number of players registered. Starts at 1.
    private string _name;
    
    public Bot(int id, int port, string name) {
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
    
    public bool HasMessageReceived() => _socket.HasMessageReceived();
}