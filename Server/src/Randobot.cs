
namespace Server;

public class Randobot : IBot {
   public int ID => _id;

    public string Name => $"Randobot {_id}";

    public int Bank { get => _bank; set => _bank = value; }

    public BotGameData GameData => _gameData;

    public DateTime LastChatTime { get => _lastChatTime; set => _lastChatTime = value; }


    private int _bank;
    private int _id;
    private BotGameData _gameData;
    private DateTime _lastChatTime;


    public Randobot(int id, int startingBank) {
        _id = id;
        _bank = startingBank;
        _gameData = new BotGameData();
    }

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

    public void Close() { }

    public bool HasMessageReceived() {
        return true;
    }

    public Dictionary<string, object> ReceiveMessage() {
        throw new NotImplementedException();
    }

    public void SendMessage(Dictionary<string, object> message) {
        throw new NotImplementedException();
    }
}