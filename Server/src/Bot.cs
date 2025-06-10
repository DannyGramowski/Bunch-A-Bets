using System.Text.Json;

namespace Server;

public class Bot : IBot {
    public int ID => _id;
    public string Name => _name;
    public BotGameData GameData => _gameData;
    public int Bank { get { return _bank; } set { _bank = value; } }
    public DateTime LastChatTime{get {return _lastChatTime;} set{ _lastChatTime = value; }}

    private BotSocket _socket;

    private int _id; //sequential value set based on number of players registered. Starts at 1.
    private string _name;
    private int _bank;
    private BotGameData _gameData;
    private DateTime _lastChatTime;
    // private Epic? epic;

    public Bot(int id, int port, string name, int startingBank) {
        _socket = new BotSocket(port, this);
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

    public bool HasMessageReceived() => _socket.HasMessageReceived();

    public override string ToString() {
        return $"ID: {ID}, Name: {_name}, Bank: {Bank}, Cards: {string.Join(",", GameData.Cards)}";
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

    // public void SetEpic(Epic epic) {
    //     this.epic = epic;
    // }

    // public void TryStartEpic() {
    //     epic?.TryStart();
    // }

    public void Close() {
        _socket.CloseSocket();
    }
}