using System.Diagnostics;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Server;

using Json = Dictionary<string, string>;

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
    private const int BIG_BLIND = 10;
    private const int SMALL_BLIND = 5;


    private List<Bot> _bots;
    private Deck _deck;
    private List<Card> _centerCards = new List<Card>();
    private int _gameId;
    private int _handNumber;
    private RoundStage _roundStage = RoundStage.PreFlop;
    private int _totalPot = 0;
    private int _highestBidValue = 0;

    private static int _idCounter = 0;

    private List<string> _logs = new List<string>();

    /**
     * bots: need to add in random order
     */
    public Game(List<Bot> bots) {
        if (bots.Count < 2 || bots.Count > 6) {
            Console.Error.WriteLine("Invalid number of bots. must be between 2 and 6.");
        }

        _bots = bots;
        _gameId = _idCounter;
        _idCounter++;
        _deck = new Deck();
    }

    public void PlayGame() {
        _logs = new List<string>();
        for (int i = 0; i < _bots.Count; i++) {//TODO is this correct. This doesn't guarentee 6 games

            PlayHand();

            //move order of players
            Bot firstPlayer = _bots[0];
            _bots.RemoveAt(0);
            _bots.Add(firstPlayer);
            _handNumber++;
        }

    }

    private void PlayHand() {
        _deck = new Deck();
        foreach (Bot bot in _bots) {
            bot.GameData.NewHand(new List<Card>() { _deck.DrawCard(), _deck.DrawCard() });
        }

        //clear bot pots and reset round states for those still playing. Clears from any previous hands. This is probably redundant
        foreach (Bot bot in _bots) {
            bot.GameData.NewRound();
        }


        _bots[-1].Bet(BIG_BLIND);
        _bots[-2].Bet(SMALL_BLIND);

        PlayRound(); //initial round no cards

        _centerCards.Add(_deck.DrawCard());
        _centerCards.Add(_deck.DrawCard());
        _centerCards.Add(_deck.DrawCard());
        //deal 3 cards

        PlayRound(); //Flop

        _centerCards.Add(_deck.DrawCard());
        //deal 1 card

        PlayRound(); //Turn

        _centerCards.Add(_deck.DrawCard());
        //deal 1 card

        PlayRound(); //River


        //showdown
    }

    private void PlayRound() {
        _highestBidValue = 0;


        // TODO Really, this should be a while true and keep going until the bets are set. Also probably needs some more logic for skipping bots who can't bet
        //while all bots are not either folded, all in, or their be meets the pot bet
        bool continueRound = true;
        while (continueRound) {
            continueRound = false;
            foreach (Bot bot in _bots) {
                //If the bot has played previous but someone after raised, they get another chance to call, raise or fold
                if (!(bot.GameData.RoundState == BotRoundState.NotPlayed || (bot.GameData.StillBidding() && bot.GameData.PotValue != _highestBidValue))) continue;
                continueRound = true;

                if (bot.Bank == 0) {
                    TakeAction(ActionType.Fold, 0, bot);
                    continue;
                }

                var logData = GetBotRequestActionData(bot);
                logData["hand"] = ""; //sanitize out hand data from logs
                WriteLog(bot, logData);

                bot.SendMessage(GetBotRequestActionData(bot));

                DateTime startClock = DateTime.Now;
                while (true) {
                    bool actionTaken = GetAnyMessages(bot);
                    if (actionTaken) { break; }
                    if (DateTime.Now > startClock + TimeSpan.FromMilliseconds(ACTION_TIMEOUT_MS)) {
                        TakeAction(ActionType.Fold, 0, bot); // womp womp
                        break;
                    }
                    // TODO consider adding a brief wait here
                }
                // wait off remainder of time
                while (true) {
                    if (DateTime.Now > startClock + TimeSpan.FromMilliseconds(ACTION_MIN_TIMEOUT_MS)) {
                        break;
                    }
                    Thread.Sleep(10);
                }
            }
        }

        //clear bot pots and reset round states for those still playing
        foreach (Bot bot in _bots) {
            bot.GameData.NewRound();
        }
    }

    /* Handles messages from any bot. Only accepts TakeAction messages from the currently active bot. 
     * if a valid TakeAction message from the current bot was received during this cycle, returns trues,
     * otherwise returns false.
     */
    private bool GetAnyMessages(Bot activeBot) {
        bool isResolved = false;
        foreach (Bot b in _bots) {
            if (b.HasMessageReceived()) {
                Json message = b.ReceiveMessage();
                isResolved |= HandleResponse(message, b, b == activeBot);
            }
        }
        return isResolved;
    }

    private bool TakeAction(ActionType actionType, int raiseAmount, Bot bot) {
        BotGameData data = bot.GameData;

        if (actionType == ActionType.Fold) {
            data.RoundState = BotRoundState.Folded;
        } else {
            if (actionType == ActionType.Call) {
                data.RoundState = BotRoundState.Called;
                raiseAmount = _highestBidValue;
            } else {
                if (raiseAmount < _highestBidValue) {
                    SendErrorMessage(bot, ErrorType.InvalidInput);
                    return false;
                }

                data.RoundState = BotRoundState.Raised;
                _highestBidValue = raiseAmount;
            }

            if (bot.Bank < raiseAmount) {
                data.RoundState = BotRoundState.AllIn;
                raiseAmount = bot.Bank;
            }

            bot.Bank -= raiseAmount;
            data.PotValue = raiseAmount;
            _totalPot += raiseAmount;
        }

        //set action take
        //increase round and game pot
        //decrease player pot
        //only bid equal to their own pot
        //set all in if applicable
        // switch (actionType) {
        //     case ActionType.Call:
        //         if (float)

        //             break;
        //     case ActionType.Raise:

        //         break;

        //     case ActionType.Fold:
        //         data.RoundState = BotRoundState.Folded;
        //         result = 0;
        //         break;
        // }

        //add in new bot state and new bot pot
        WriteLog(bot, new Json() {
            {"action_type", ActionTypeExtensions.ToActionString(actionType)},
            {"raise_amount", raiseAmount.ToString()},
        });
        return true;
    }

    private void SendChat(string message, Bot bot) {
        if (bot.lastChatTime > DateTime.Now - TimeSpan.FromMilliseconds(CHAT_TIMEOUT_MS)) {
            return;
        }
        Json response = new Json() {
            {"command", Command.ReceiveChat.ToCommandString()},
            {"message", message},
            {"author_name", bot.Name},
        };
        foreach (Bot b in _bots) {
            b.SendMessage(response);
        }

        WriteLog(bot, response);
        bot.lastChatTime = DateTime.Now;
    }

    private void WriteLog(Bot bot, string str) {
        _logs.Add($"{bot.Name}: {str}");
    }

    private void WriteLog(Bot bot, Json data) {
        WriteLog(bot, JsonSerializer.Serialize(data));
    }

    private void SendLogs(Bot bot) {
        Json response = new Json() {
            {"command", Command.LogData.ToCommandString()},
            {"logs", JsonSerializer.Serialize(_logs)},
            {"author_name", bot.Name},
        };
        bot.SendMessage(response);

        WriteLog(bot, response);
    }

    /* Handles a single bot message mid-game. Returns true if the message reflects a valid TakeAction message,
     * and the action was performed successfully.
     */
    private bool HandleResponse(Json response, Bot bot, bool actionAllowed) {
        if (!response.ContainsKey(CommandExtensions.CommandText)) {
            bot.SendMessage(GetErrorMessageData(ErrorType.InvalidInput));
            return false;
        }
        Command cmd = CommandExtensions.FromCommandString(response[CommandExtensions.CommandText]);

        switch (cmd) {
            case Command.TakeAction:
                if (!actionAllowed) {
                    bot.SendMessage(GetErrorMessageData(ErrorType.NotExpected));
                    return false;
                }
                return HandleTakeAction(response, bot);
            case Command.SendChat:
                if (!response.ContainsKey("message") || response["message"].Length == 0) {
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

    private bool HandleTakeAction(Json response, Bot bot) {
        if (!response.ContainsKey("action_type")) {
            SendErrorMessage(bot, ErrorType.InvalidInput);
            return false;
        }
        ActionType actionType;
        try {
            actionType = ActionTypeExtensions.FromActionString(response["action_type"]);
        }
        catch (JsonException jse) {
            SendErrorMessage(bot, ErrorType.BadActionType);
            return false;
        }

        int raiseAmount = 0;
        if (actionType == ActionType.Raise) {
            if (!response.ContainsKey("raise_amount")) {
                SendErrorMessage(bot, ErrorType.BadActionType);
                return false;
            }
            if (!int.TryParse(response["raise_amount"], NumberStyles.Number, CultureInfo.InvariantCulture, out raiseAmount)) {
                SendErrorMessage(bot, ErrorType.BadValue);
                return false;
            }

            // Count digits after decimal
            var parts = response["raise_amount"].Split('.');
            if (parts.Length > 1 && parts[1].Length > 2) {
                SendErrorMessage(bot, ErrorType.BadValue);
                return false;
            }
        }
        // if (actionType == ActionType.Call) {
        //     raiseAmount = 
        // }
        return TakeAction(actionType, raiseAmount, bot);
    }

    private void SendErrorMessage(Bot bot, ErrorType error) {
        Json data = GetErrorMessageData(error);
        WriteLog(bot, data);
        bot.SendMessage(data);

    }

    private Json GetErrorMessageData(ErrorType error) {
        return new Json() {
            {"command", Command.ConfirmAction.ToCommandString()},
            {"result", "success"},
            {"error", error.ToErrorString()},
        };
    }

    private Json GetBotRequestActionData(Bot bot) {
        return new Json() {
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