using System.Diagnostics;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Server;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ErrorType {
    [EnumMember(Value = "invalid_input")] InvalidInput,
    [EnumMember(Value = "not_expected")] NotExpected,
    [EnumMember(Value = "bad_action_type")] BadActionType,
    [EnumMember(Value = "bad_value")] BadValue,
}

public static class ErrorTypeExtensions {
    public static string ToErrorString(this ErrorType errorType) {
        // Serialize to JSON, e.g., "invalid_input"
        string json = JsonSerializer.Serialize(errorType);
        return json.Trim('"'); // Remove surrounding quotes
    }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ActionType {
    [EnumMember(Value = "call")] Call,
    [EnumMember(Value = "raise")] Raise,
    [EnumMember(Value = "fold")] Fold,
}

public static class ActionTypeExtensions {
    public static string ToActionString(this ActionType actionType) {
        // Serialize to JSON, e.g., "raise"
        string json = JsonSerializer.Serialize(actionType);
        return json.Trim('"'); // Remove surrounding quotes
    }
    public static ActionType FromActionString(string value) {
        return JsonSerializer.Deserialize<ActionType>($"\"{value}\"");
    }
}

public class Game {

    private const int CHAT_TIMEOUT_MS = 5 * 1000;
    private const int ACTION_TIMEOUT_MS = 3 * 1000;
    private const int ACTION_MIN_TIMEOUT_MS = 1 * 1000;

    private List<Bot> _bots;
    private Deck _deck;
    private List<Card> _centerCards = new List<Card>();
    private int _gameId;
    private int _handNumber;
    private RoundStage _roundStage = RoundStage.PreFlop;
    private float _totalPot = 0;

    private static int _idCounter = 0;

    private List<string> _logs = new List<string>();

    /**
     * bots: need to add in random order
     */
    public Game(List<Bot> bots)
    {
        if (bots.Count < 2 || bots.Count > 6)
        {
            Console.Error.WriteLine("Invalid number of bots. must be between 2 and 6.");
        }

        _bots = bots;
        _gameId = _idCounter;
        _idCounter++;
    }

    public void PlayGame() {
        _logs = new List<string>();
        for (int i = 0; i < _bots.Count; i++)
        {
            PlayHand();
        }

    }

    private void PlayHand() {
        _deck = new Deck();
        foreach (Bot bot in _bots) {
            bot.GameData.NewHand(new List<Card>() { _deck.DrawCard(), _deck.DrawCard() });
        }

        //big and small blinds

        //play all rounds

        //showdown

        //move order of player
        Bot firstPlayer = _bots[0];
        _bots.RemoveAt(0);
        _bots.Add(firstPlayer);
        _handNumber++;
    }

    private void PlayRound() {
        foreach (Bot bot in _bots) {
            bot.GameData.NewRound();
        }

        foreach (Bot bot in _bots) // TODO Really, this should be a while true and keep going until the bets are set. Also probably needs some more logic for skipping bots who can't bet
        {
            if (bot.GameData.RoundState != BotRoundState.NotPlayed) continue;

            bot.SendMessage(GetBotRequestActionData(bot));

            DateTime startClock = DateTime.Now;
            while (true)
            {
                bool actionTaken = GetAnyMessages(bot);
                if (actionTaken) { break; }
                if (DateTime.Now > startClock + TimeSpan.FromMilliseconds(ACTION_TIMEOUT_MS))
                {
                    TakeAction(ActionType.Fold, 0, bot); // womp womp
                    break;
                }
                // TODO consider adding a brief wait here
            }
            // wait off remainder of time
            while (true)
            {
                if (DateTime.Now > startClock + TimeSpan.FromMilliseconds(ACTION_MIN_TIMEOUT_MS))
                {
                    break;
                }
                Thread.Sleep(10);
            }
        }
    }

    /* Handles messages from any bot. Only accepts TakeAction messages from the currently active bot. 
     * if a valid TakeAction message from the current bot was received during this cycle, returns trues,
     * otherwise returns false.
     */
    private bool GetAnyMessages(Bot activeBot)
    {
        bool isResolved = false;
        foreach (Bot b in _bots)
        {
            if (b.HasMessageReceived())
            {
                Dictionary<string, string> message = b.ReceiveMessage();
                isResolved |= HandleResponse(message, b, b == activeBot);
            }
        }
        return isResolved;
    }

    private bool TakeAction(ActionType actionType, float raiseAmount, Bot bot) {
        return true;
    }

    private void SendChat(string message, Bot bot)
    {
        if (bot.lastChatTime > DateTime.Now - TimeSpan.FromMilliseconds(CHAT_TIMEOUT_MS))
        {
            // Might want to warn the bot here, but maybe not
            return;
        }
        Dictionary<string, string> response = new Dictionary<string, string>() {
            {"command", Command.ReceiveChat.ToCommandString()},
            {"message", message},
            {"author_name", bot.Name},
        };
        foreach (Bot b in _bots)
        {
            if (b == bot) { continue; }
            b.SendMessage(response);
        }
        bot.lastChatTime = DateTime.Now;
    }

    private void SendLogs(Bot bot)
    {
        bot.SendMessage(new Dictionary<string, string>() {
            {"command", Command.LogData.ToCommandString()},
            {"logs", JsonSerializer.Serialize(_logs)},
            {"author_name", bot.Name},
        });
    }

    /* Handles a single bot message mid-game. Returns true if the message reflects a valid TakeAction message,
     * and the action was performed successfully.
     */
    private bool HandleResponse(Dictionary<string, string> response, Bot bot, bool actionAllowed)
    {
        if (!response.ContainsKey(CommandExtensions.CommandText))
        {
            bot.SendMessage(GetErrorMessageData(ErrorType.InvalidInput));
            return false;
        }
        Command cmd = CommandExtensions.FromCommandString(response[CommandExtensions.CommandText]);
        switch (cmd)
        {
            case Command.TakeAction:
                if (!actionAllowed)
                {
                    bot.SendMessage(GetErrorMessageData(ErrorType.NotExpected));
                    return false;
                }
                return HandleTakeAction(response, bot);
            case Command.SendChat:
                if (!response.ContainsKey("message") || response["message"].Length == 0)
                {
                    bot.SendMessage(GetErrorMessageData(ErrorType.InvalidInput));
                    return false;
                }
                SendChat(response["message"], bot);
                return false;
            case Command.GetLogs:
                SendLogs(bot);
                return false;
        }
        return false;
    }

    private bool HandleTakeAction(Dictionary<string, string> response, Bot bot) {
        if (!response.ContainsKey("action_type"))
        {
            bot.SendMessage(GetErrorMessageData(ErrorType.InvalidInput));
            return false;
        }
        ActionType actionType;
        try
        {
            actionType = ActionTypeExtensions.FromActionString(response["action_type"]);
        }
        catch (JsonException jse)
        {
            bot.SendMessage(GetErrorMessageData(ErrorType.BadActionType));
            return false;
        }
        float raiseAmount = 0;
        if (actionType == ActionType.Raise)
        {
            if (!response.ContainsKey("raise_amount"))
            {
                bot.SendMessage(GetErrorMessageData(ErrorType.BadActionType));
                return false;
            }
            if (!float.TryParse(response["raise_amount"], NumberStyles.Number, CultureInfo.InvariantCulture, out raiseAmount))
            {
                bot.SendMessage(GetErrorMessageData(ErrorType.BadValue));
                return false;
            }

            // Count digits after decimal
            var parts = response["raise_amount"].Split('.');
            if (parts.Length > 1 && parts[1].Length > 2) {
                bot.SendMessage(GetErrorMessageData(ErrorType.BadValue));
                return false;
            }
        }
        return TakeAction(actionType, raiseAmount, bot);
    }

    private Dictionary<string, string> GetErrorMessageData(ErrorType error)
    {
        return new Dictionary<string, string>()
        {
            {"command", Command.ConfirmAction.ToCommandString()},
            {"result", "success"},
            {"error", error.ToErrorString()},
        };
    }

    private Dictionary<string, string> GetBotRequestActionData(Bot bot)
    {
        return new Dictionary<string, string>() {
            {"command", Command.RequestAction.ToCommandString()},
            {"hand", Card.SerializeCardList(bot.GameData.Cards)},
            {"center_cards", Card.SerializeCardList(_centerCards)},
            {"game_number", _gameId.ToString()},
            {"hand_number", _handNumber.ToString()},
            {"round_number", ((int)_roundStage).ToString()},
            {"players", Bot.SerializeBotsList(_bots) },
            {"highest_bid_value", _bots.Max(_bot => _bot.GameData.PotValue).ToString()},
            {"total_pot_value", _totalPot.ToString()}
        };
    }
}