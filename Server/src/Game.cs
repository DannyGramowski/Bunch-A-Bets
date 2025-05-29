using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Server;

using Json = Dictionary<string, object>;

public enum ErrorType {
    InvalidInput,
    NotExpected,
    BadActionType,
    BadValue,
}

public static class ErrorTypeExtensions {
    private static readonly Dictionary<ErrorType, string> ErrorToString = new()
    {
        { ErrorType.InvalidInput, "invalid_input" },
        { ErrorType.NotExpected, "not_expected" },
        { ErrorType.BadActionType, "bad_action_type" },
        { ErrorType.BadValue, "bad_value" },
    };

    private static readonly Dictionary<string, ErrorType> StringToError = ErrorToString
        .ToDictionary(kv => kv.Value, kv => kv.Key);

    public static string ToErrorString(this ErrorType errorType)
    {
        return ErrorToString[errorType];
    }

    public static ErrorType FromErrorString(string value)
    {
        return StringToError[value];
    }
}

public enum ActionType {
    Call,
    Raise,
    Fold,
}

public static class ActionTypeExtensions {
    private static readonly Dictionary<ActionType, string> ActionToString = new()
    {
        { ActionType.Call, "call" },
        { ActionType.Raise, "raise" },
        { ActionType.Fold, "fold" },
    };

    private static readonly Dictionary<string, ActionType> StringToAction = ActionToString
        .ToDictionary(kv => kv.Value, kv => kv.Key);

    public static string ToActionString(this ActionType actionType)
    {
        return ActionToString[actionType];
    }

    public static ActionType FromActionString(string value)
    {
        return StringToAction[value]; // Add safety if desired
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

    internal void PlayHand() {
        _deck = new Deck();
        foreach (Bot bot in _bots) {
            bot.GameData.NewHand(new List<Card>() { _deck.DrawCard(), _deck.DrawCard() });
        }

        //clear bot pots and reset round states for those still playing. Clears from any previous hands. This is probably redundant
        foreach (Bot bot in _bots) {
            bot.GameData.NewRound();
        }

        BotBet(_bots[_bots.Count - 1], BIG_BLIND);
        BotBet(_bots[_bots.Count - 2], SMALL_BLIND);

        int[] roundCards = [0, 3, 1, 1];

        bool result;
        for (int r = 0; r < 4; r++) {
            for (int i = 0; i < roundCards[r]; i++) {
                _centerCards.Add(_deck.DrawCard());
            }
            result = PlayRound();
            if (result) {
                break;
            }
        }


        HandleShowdown(_bots, _centerCards, _totalPot);
        _totalPot = 0;
    }

    /* Plays a single round of the game.
     * Returns true if the hand ends here (advances immediately to Showdown)
     */
    internal bool PlayRound() {
        Console.WriteLine("Beginning Round");
        // TODO Really, this should be a while true and keep going until the bets are set. Also probably needs some more logic for skipping bots who can't bet
        //while all bots are not either folded, all in, or their be meets the pot bet
        bool continueRound = true;
        while (continueRound)
        {
            continueRound = false;
            foreach (Bot bot in _bots)
            {
                //If the bot has played previous but someone after raised, they get another chance to call, raise or fold
                // TODO also, if all other bots have folded or all in, the round (and the entire hand) should be finished immediately - do this by returning True to PlayRound()
                if (EveryoneAllIn())
                {
                    return true;
                }
                if (!(bot.GameData.RoundState == BotRoundState.NotPlayed || (bot.GameData.StillBidding() && bot.GameData.PotValue != _highestBidValue))) continue;
                continueRound = true;

                if (bot.Bank == 0)
                {
                    TakeAction(ActionType.Fold, 0, bot);
                    continue;
                }

                var logData = GetBotRequestActionData(bot);
                logData["hand"] = new List<Json>(); //sanitize out hand data from logs
                WriteLog(bot, true, logData);

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
                    Thread.Sleep(5);
                }
                // wait off remainder of time
                while (true)
                {
                    if (DateTime.Now > startClock + TimeSpan.FromMilliseconds(ACTION_MIN_TIMEOUT_MS))
                    {
                        break;
                    }
                    GetAnyMessages(null); // Allows for sending messages during this time
                    Thread.Sleep(5);
                }
                GetAnyMessages(null); // Allow for one chance to send messages before next round, that way "reaction" messages get sent here
            }
        }

        if (EveryoneAllIn())
        {
            return true;
        }

        //clear bot pots and reset round states for those still playing
        foreach (Bot bot in _bots)
        {
            bot.GameData.NewRound();
        }
        _highestBidValue = 0;
        return false;
    }

    internal bool EveryoneAllIn()
    {
        int notAllInCount = 0;
        foreach (Bot bot in _bots)
        {
            if (bot.GameData.RoundState != BotRoundState.Folded && bot.GameData.RoundState != BotRoundState.AllIn) { notAllInCount ++; }
        }
        return notAllInCount <= 1;
    }

    internal static void HandleShowdown(List<Bot> bots, List<Card> centerCards, int totalPot) {
        Console.WriteLine("SHOWDOWN TIME BABY");

        var botsCopy = bots.ToList();

        var ct = botsCopy.Count(b => b.GameData.RoundState != BotRoundState.Folded && b.GameData.RoundState != BotRoundState.NotPlayed);
        //protect against everyone being folded if that ever happens
        if (ct == 0) {
            foreach (Bot b in botsCopy) {
                b.GameData.RoundState = BotRoundState.Called;
            }
        }

        int count = 5;// prevent infinite loops
        //I understand this is complicated. Unfortunatley due to edges cases like ties and bots can only win what they bet it is like this.
        while (totalPot > 0 && count > 0) {
            //This will contain at least 2 bots if there is a tie.
            List<Bot> highestHands = new();
            //This contains the bet value initially.
            //This acts as the available pot to take from of the losing bots for the winners. In the case of an overflow from the winners, each bots value will contain the value left to disperse.
            var botBets = botsCopy.ToDictionary(bot => bot.ID, bot => bot.GameData.PotValueOfHand);

            //finds the best hands from all bots that made it to the end
            foreach (Bot b in botsCopy) {
                if (b.GameData.RoundState == BotRoundState.Folded) continue;
                if (highestHands.Count == 0) {
                    highestHands.Add(b);
                    continue;
                }

                var handComparisonResult = HandComparisonUtility.CompareBotHands(highestHands[0], b, centerCards);
                if (handComparisonResult == HandWinner.Tie) {
                    highestHands.Add(b);
                } else if (handComparisonResult == HandWinner.Player2) {
                    highestHands.Clear();
                    highestHands.Add(b);
                }
            }

            // //This will lose some money do to int division if pot % count != 0. This will only be 1 or 2 cents so its not a big deal
            // var amountPerBot = totalPot / highestHands.Count;
            foreach (Bot winningBot in highestHands) {
                foreach (Bot bot in botsCopy) {
                    if (highestHands.Contains(bot)) {
                        //this will get called multiple times if there is a tie but PotValueOf Hand will be 0 after the first time so it wont do anything
                        if (bot.GameData.PotValueOfHand == 0) continue;
                        bot.Bank += bot.GameData.PotValueOfHand;
                        totalPot -= bot.GameData.PotValueOfHand;
                        bot.GameData.PotValueOfHand = 0;
                    } else {
                        int value = Math.Min(botBets[winningBot.ID], botBets[bot.ID] / highestHands.Count);
                        winningBot.Bank += value;
                        totalPot -= value;
                        bot.GameData.PotValueOfHand -= value;

                        //handle round off error
                        if (bot.GameData.PotValueOfHand <= highestHands.Count - 1) {
                            totalPot -= bot.GameData.PotValueOfHand;
                            bot.GameData.PotValueOfHand = 0;
                        }
                    }
                }

                botsCopy.Remove(winningBot);
            }

            count--;
        }
    }

    /* Handles messages from any bot. Only accepts TakeAction messages from the currently active bot. 
     * if a valid TakeAction message from the current bot was received during this cycle, returns trues,
     * otherwise returns false.
     */
    private bool GetAnyMessages(Bot? activeBot) {
        bool isResolved = false;
        foreach (Bot b in _bots) {
            if (b.HasMessageReceived()) {
                Json message = b.ReceiveMessage();
                isResolved |= HandleResponse(message, b, b == activeBot);
            }
        }
        return isResolved;
    }

    internal bool TakeAction(ActionType actionType, int raiseAmount, Bot bot) {
        BotGameData data = bot.GameData;

        if (actionType == ActionType.Fold)
        {
            data.RoundState = BotRoundState.Folded;
        }
        else
        {
            if (actionType == ActionType.Call)
            {
                data.RoundState = BotRoundState.Called;
                raiseAmount = _highestBidValue;
            }
            else
            {
                if (raiseAmount < _highestBidValue)
                {
                    SendErrorMessage(bot, ErrorType.InvalidInput);
                    return false;
                }

                data.RoundState = BotRoundState.Raised;
            }

            BotBet(bot, raiseAmount);
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
        WriteLog(bot, false, new Json() {
            {"action_type", ActionTypeExtensions.ToActionString(actionType)},
            {"raise_amount", bot.GameData.PotValue},
        });
        return true;
    }

    internal void BotBet(Bot bot, int amount)
    {
        int actualBetAmount = bot.Bet(amount - bot.GameData.PotValue);
        _totalPot += actualBetAmount;
        if (bot.GameData.PotValue > _highestBidValue)
        {
            _highestBidValue = bot.GameData.PotValue;
        }
    }

    private void SendChat(string message, Bot bot)
    {
        if (bot.lastChatTime > DateTime.Now - TimeSpan.FromMilliseconds(CHAT_TIMEOUT_MS))
        {
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

        WriteLog(bot, false, response);
        bot.lastChatTime = DateTime.Now;
    }

    private void WriteLog(Bot bot, bool outgoing, string str)
    {
        string outgoingString = outgoing ? "received" : "sent";
        _logs.Add($"{bot.Name} {outgoingString}: {str}");
        Console.WriteLine($"{bot.Name} {outgoingString}: {str}");
    }

    private void WriteLog(Bot bot, bool outgoing, Json data) {
        WriteLog(bot, outgoing, JsonSerializer.Serialize(data));
    }

    private void SendLogs(Bot bot) {
        Json response = new Json() {
            {"command", Command.LogData.ToCommandString()},
            {"logs", JsonSerializer.Serialize(_logs)},
            {"author_name", bot.Name},
        };
        bot.SendMessage(response);

        WriteLog(bot, true, response);
    }

    /* Handles a single bot message mid-game. Returns true if the message reflects a valid TakeAction message,
     * and the action was performed successfully.
     */
    private bool HandleResponse(Json response, Bot bot, bool actionAllowed)
    {
        try
        {
            if (!response.ContainsKey(CommandExtensions.CommandText))
            {
                bot.SendMessage(GetErrorMessageData(ErrorType.InvalidInput));
                return false;
            }
            Command cmd = CommandExtensions.FromCommandString(response[CommandExtensions.CommandText].ToString());

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
                    if (!response.ContainsKey("message") || response["message"].ToString().Length == 0)
                    {
                        bot.SendMessage(GetErrorMessageData(ErrorType.InvalidInput));
                        return false;
                    }
                    SendChat(response["message"].ToString(), bot);
                    return false;
                case Command.GetLogs:
                    SendLogs(bot);
                    return false;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error handling message from Bot {bot.Name}: {e.Message}");
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
            actionType = ActionTypeExtensions.FromActionString(response["action_type"].ToString());
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
            if (!int.TryParse(response["raise_amount"].ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out raiseAmount)) {
                SendErrorMessage(bot, ErrorType.BadValue);
                return false;
            }

            // Count digits after decimal
            var parts = response["raise_amount"].ToString().Split('.');
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
        WriteLog(bot, true, data);
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
            {"game_number", _gameId},
            {"hand_number", _handNumber},
            {"round_number", _roundStage},
            {"players", Bot.SerializeBotsList(_bots) },
            {"highest_bid_value", _highestBidValue},
            {"total_pot_value", _totalPot}
        };
    }
}