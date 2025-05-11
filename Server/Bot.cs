namespace Server;

public class Bot {
    public int ID => _id;
    
    private BotSocket _socket;
    private int _id; //sequential value set based on number of players registered. Starts at 1.
    
    public Bot(int id, int port) {
        _socket = new BotSocket(port);
        _id = id;
        Console.WriteLine($"Bot ID: {_id}, Bot Port: {port}");
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
}