using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using Newtonsoft.Json;
using UnityEngine;

namespace Server
{

    using Json = Dictionary<string, object>;

    public enum ErrorType
    {
        InvalidInput,
        NotExpected,
        BadActionType,
        BadValue,
        InvalidRaiseAmount,
    }

    public static class ErrorTypeExtensions
    {
        private static readonly Dictionary<ErrorType, string> ErrorToString = new()
    {
        { ErrorType.InvalidInput, "invalid_input" },
        { ErrorType.NotExpected, "not_expected" },
        { ErrorType.BadActionType, "bad_action_type" },
        { ErrorType.BadValue, "bad_value" },
        { ErrorType.InvalidRaiseAmount, "invalid_raise_amount" },

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

    public enum ActionType
    {
        Call,
        Raise,
        Fold,
    }

    public static class ActionTypeExtensions
    {
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

    public class Game
    {

        private const int CHAT_TIMEOUT_MS = 5 * 1000;
        private const int ACTION_TIMEOUT_MS = 3 * 1000;
        private const int ACTION_MIN_TIMEOUT_MS = 1 * 1000;
        private const int BIG_BLIND = 100;
        private const int SMALL_BLIND = 50;


        private List<IBot> _bots;
        private Deck _deck;
        private List<Card> _centerCards = new List<Card>();
        private int _gameId;
        private int _handNumber;
        private RoundStage _roundStage = RoundStage.PreFlop;
        private int _totalPot = 0;
        private int _highestBidValue = 0;
        private int _numberTimesRaiseThisRound = 0;

        private static int _idCounter = 0;
        private bool _isTournament;

        private List<string> _logs = new List<string>();

        /**
         * bots: need to add in random order
         */
        public Game(List<IBot> bots, bool isTournament)
        {
            if (bots.Count < 2 || bots.Count > 6)
            {
                Console.Error.WriteLine("Invalid number of bots. must be between 2 and 6.");
            }

            _isTournament = isTournament;
            _bots = bots;
            _gameId = _idCounter;
            _idCounter++;
            _deck = new Deck();
        }

        public void PlayGame(int numGames)
        {
            _logs = new List<string>();
            GameManager.RunOnMainThread(() => GameManager.Manager.SetPlayers(_bots));
            for (int i = 0; i < numGames; i++)
            {
                try
                {
                    PlayHand();
                }
                catch (Exception e)
                {
                    Debug.Log($"Exception while playing hand: {e.Message} {e.StackTrace}");
                }

                //move order of players
                IBot firstPlayer = _bots[0];
                _bots.RemoveAt(0);
                _bots.Add(firstPlayer);
                _handNumber++;

                // If there are less than 2 bots left in the game, we can't play anymore... just end this game
                int remainingBots = 0;
                foreach (IBot b in _bots)
                {
                    if (b.Bank > 0)
                    {
                        remainingBots++;
                    }
                }
                if (remainingBots < 2)
                {
                    break;
                }
            }

        }

        internal void PlayHand()
        {
            _deck = new Deck();
            foreach (IBot bot in _bots)
            {
                bot.GameData.NewHand(new List<Card>() { _deck.DrawCard(), _deck.DrawCard() });
            }

            //clear bot pots and reset round states for those still playing. Clears from any previous hands. This is probably redundant
            foreach (IBot bot in _bots)
            {
                bot.GameData.NewRound();
            }

            int[] roundCards = new int[] { 0, 3, 1, 1 };

            _centerCards.Clear();
            _roundStage = RoundStage.PreFlop;

            bool result;
            for (int r = 0; r < 4; r++)
            {
                for (int i = 0; i < roundCards[r]; i++)
                {
                    _centerCards.Add(_deck.DrawCard());
                }
                result = PlayRound();
                _roundStage += 1;
                if (result)
                {
                    break;
                }
            }

            for (int i = _centerCards.Count; i < 5; i++)
            {
                _centerCards.Add(_deck.DrawCard());
            }

            List<Json> pots = HandleShowdown(_bots, _centerCards, _totalPot);
            Json handResultData = GetHandResultData(pots);
            bool alreadyLogged = false;
            foreach (IBot b in _bots)
            {
                b.SendMessage(handResultData);
                if (!alreadyLogged)
                {
                    WriteLog(b, true, handResultData, true);
                    alreadyLogged = true;
                }
            }
            _totalPot = 0;
        }

        /* Plays a single round of the game.
         * Returns true if the hand ends here (advances immediately to Showdown)
         */
        internal bool PlayRound()
        {
            Debug.Log("Beginning Round");

            //clear bot pots and reset round states for those still playing
            foreach (IBot bot in _bots)
            {
                
                if (bot.GameData.PotValue > 0)
                {
                    int amount = bot.GameData.PotValue;
                    GameManager.RunOnMainThread(() => GameManager.Manager.GetPlayerByBotId(bot.ID).PushChips(amount));
                }
                bot.GameData.NewRound();
                if (Program.VERBOSE_DEBUGGING)
                {
                    Debug.Log($"Bot state is {bot.GameData.RoundState}");
                }
            }

            _highestBidValue = 0;
            if (_roundStage == RoundStage.PreFlop)
            {
                // Can't just use simple integer indexing here, as this could assign blinds to bots with 0 in the bank
                int i;
                for (i = _bots.Count - 1; i >= 0; i--)
                {
                    if (_bots[i].Bank > 0)
                    {
                        BotBet(_bots[i], BIG_BLIND);
                        break;
                    }
                }
                for (int j = i - 1; j >= 0; j--)
                {
                    if (_bots[j].Bank > 0)
                    {
                        BotBet(_bots[j], SMALL_BLIND);
                        break;
                    }
                }
            }

            // TODO Really, this should be a while true and keep going until the bets are set. Also probably needs some more logic for skipping bots who can't bet
            //while all bots are not either folded, all in, or their be meets the pot bet
            _numberTimesRaiseThisRound = 0;
            bool continueRound = true;
            while (continueRound)
            {
                continueRound = false;
                foreach (IBot bot in _bots)
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
                    {//shame
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
                    if (_isTournament)
                    {
                        while (true)
                        {
                            if (DateTime.Now > startClock + TimeSpan.FromMilliseconds(ACTION_MIN_TIMEOUT_MS))
                            {
                                break;
                            }
                            GetAnyMessages(null); // Allows for sending messages during this time
                            Thread.Sleep(5);
                        }
                    }
                    else
                    {
                        Thread.Sleep(20); // Prevent this from being blazingly fast, which slips under the polling speed of the Randobot python script lol
                    }
                    GetAnyMessages(null); // Allow for one chance to send messages before next round, that way "reaction" messages get sent here
                }
                _numberTimesRaiseThisRound++;
            }

            if (EveryoneAllIn())
            {
                return true;
            }
            return false;
        }

        internal bool EveryoneAllIn()
        {
            int notAllInCount = 0;
            foreach (IBot bot in _bots)
            {
                if (bot.GameData.RoundState != BotRoundState.Folded && bot.GameData.RoundState != BotRoundState.AllIn) { notAllInCount++; }
            }
            return notAllInCount <= 1;
        }

        internal static List<Json> HandleShowdown(List<IBot> bots, List<Card> centerCards, int totalPot)
        {
            Debug.Log("SHOWDOWN TIME BABY");

            var botsCopy = bots.ToList();

            List<Json> pots = new List<Json>();

            var ct = botsCopy.Count(b => b.GameData.RoundState != BotRoundState.Folded);

            if (ct == 0) //protect against everyone being folded if that ever happens
            {
                return pots;
            }

            if (Program.VERBOSE_DEBUGGING)
            {
                foreach (IBot b in botsCopy)
                {
                    Debug.Log(b.ToString());
                }
            }

            Dictionary<int, int> botToTotalEarnings = new Dictionary<int, int>();
            int count = 5;// prevent infinite loops
            foreach (IBot b in botsCopy)
            {
                b.CacheHandBet();
                botToTotalEarnings.Add(b.ID, 0);
            }


            //I understand this is complicated. Unfortunatley due to edges cases like ties and bots can only win what they bet it is like this.
            while (totalPot > 0 && count > 0)
            {
                //This will contain at least 2 bots if there is a tie.
                List<IBot> highestHands = new();
                //This contains the bet value initially.
                //This acts as the available pot to take from of the losing bots for the winners. In the case of an overflow from the winners, each bots value will contain the value left to disperse.
                var botBets = botsCopy.ToDictionary(bot => bot.ID, bot => bot.GameData.PotValueOfHand);

                //finds the best hands from all bots that made it to the end
                foreach (IBot b in botsCopy)
                {
                    if (b.GameData.RoundState == BotRoundState.Folded) continue;
                    if (highestHands.Count == 0)
                    {
                        highestHands.Add(b);
                        continue;
                    }

                    var handComparisonResult = HandComparisonUtility.CompareBotHands(highestHands[0], b, centerCards);
                    if (handComparisonResult == HandWinner.Tie)
                    {
                        highestHands.Add(b);
                    }
                    else if (handComparisonResult == HandWinner.Player2)
                    {
                        highestHands.Clear();
                        highestHands.Add(b);
                    }
                }

                int sidePotValue = 0;

                // //This will lose some money do to int division if pot % count != 0. This will only be 1 or 2 cents so its not a big deal
                // var amountPerBot = totalPot / highestHands.Count;
                foreach (IBot winningBot in highestHands)
                {
                    foreach (IBot bot in botsCopy)
                    {
                        if (highestHands.Contains(bot))
                        {
                            //this will get called multiple times if there is a tie but PotValueOf Hand will be 0 after the first time so it wont do anything
                            if (bot.GameData.PotValueOfHand == 0) continue;
                            bot.Bank += bot.GameData.PotValueOfHand;
                            botToTotalEarnings[bot.ID] += bot.GameData.PotValueOfHand;
                            sidePotValue += bot.GameData.PotValueOfHand;
                            totalPot -= bot.GameData.PotValueOfHand;
                            bot.GameData.PotValueOfHand = 0;
                        }
                        else
                        {
                            int value = Math.Min(botBets[winningBot.ID], botBets[bot.ID] / highestHands.Count);
                            winningBot.Bank += value;
                            botToTotalEarnings[winningBot.ID] += value;
                            sidePotValue += value; // Yes, this might mean that the pot isn't equally split among bots here. Shame. If this comes up in the tournament I will personally give you a b**wjob for free. And I hope it happens.
                            totalPot -= value;
                            bot.GameData.PotValueOfHand -= value;

                            //handle round off error
                            if (bot.GameData.PotValueOfHand <= highestHands.Count - 1)
                            {
                                totalPot -= bot.GameData.PotValueOfHand;
                                bot.GameData.PotValueOfHand = 0;
                            }
                        }
                    }

                    botsCopy.Remove(winningBot);
                }


                pots.Add(new Json()
            {
                { "pot_amount", sidePotValue },
                { "winners", highestHands.Select(b => b.ToDictionary(false)).ToArray() },
            });

                count--;
            }

            // Now we deliver the goods
            foreach (KeyValuePair<int, int> kvp in botToTotalEarnings)
            {
                if (kvp.Value > 0)
                {
                    int amount = kvp.Value;
                    GameManager.RunOnMainThread(() => GameManager.Manager.GetPlayerByBotId(kvp.Key).WinPot(amount));
                }
            }

            return pots;
        }

        /* Handles messages from any bot. Only accepts TakeAction messages from the currently active bot. 
         * if a valid TakeAction message from the current bot was received during this cycle, returns trues,
         * otherwise returns false.
         */
        private bool GetAnyMessages(IBot? activeBot)
        {
            bool isResolved = false;
            foreach (IBot b in _bots)
            {
                if (b.HasMessageReceived())
                {
                    Json message = b.ReceiveMessage();
                    isResolved |= HandleResponse(message, b, b == activeBot);
                }
            }
            return isResolved;
        }

        internal bool TakeAction(ActionType actionType, int raiseAmount, IBot bot)
        {
            BotGameData data = bot.GameData;

            if (actionType == ActionType.Fold)
            {
                data.RoundState = BotRoundState.Folded;
            }
            else
            {
                if (actionType == ActionType.Call || _numberTimesRaiseThisRound >= 5)
                {
                    data.RoundState = BotRoundState.Called;
                    raiseAmount = _highestBidValue;
                }
                else
                {
                    if (raiseAmount < _highestBidValue)
                    {
                        SendErrorMessage(bot, ErrorType.InvalidRaiseAmount);
                        return false;
                    }
                    data.RoundState = BotRoundState.Raised;
                }

                BotBet(bot, raiseAmount);

                SendSuccessMessage(bot);
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
            return true;
        }

        internal void BotBet(IBot bot, int amount)
        {
            int actualBetAmount = bot.Bet(amount - bot.GameData.PotValue);
            _totalPot += actualBetAmount;
            if (actualBetAmount > 0)
            {
                GameManager.RunOnMainThread(() => GameManager.Manager.GetPlayerByBotId(bot.ID).Bet(actualBetAmount));
            }
            if (bot.GameData.PotValue > _highestBidValue)
            {
                _highestBidValue = bot.GameData.PotValue;
            }
        }

        private void SendChat(string message, IBot bot)
        {
            if (bot.LastChatTime > DateTime.Now - TimeSpan.FromMilliseconds(CHAT_TIMEOUT_MS))
            {
                return;
            }
            Json response = new Json() {
            {"command", Command.ReceiveChat.ToCommandString()},
            {"message", message},
            {"author", bot.ToDictionary(false)},
        };
            foreach (IBot b in _bots)
            {
                b.SendMessage(response);
            }

            WriteLog(bot, true, response);
            bot.LastChatTime = DateTime.Now;
        }

        private void WriteLog(IBot bot, bool outgoing, string str, bool logAnyways = false)
        {
            if (!Program.VERBOSE_DEBUGGING && outgoing && !logAnyways)
            {
                //dont log outgoing messages if we dont want the entire log.
                return;
            }
            string outgoingString = outgoing ? "received" : "sent";

            string escapedStr = str.Replace("\"", "'");

            string log = $"({DateTime.Now}) {bot.Name} {outgoingString}: {escapedStr}";
            _logs.Add(log);
            Debug.Log(log);

        }

        private void WriteLog(IBot bot, bool outgoing, Json data, bool logAnyways = false)
        {
            WriteLog(bot, outgoing, JsonConvert.SerializeObject(data), logAnyways);
        }

        private void SendLogs(IBot bot)
        {
            List<string> logsCapped = new List<string>();
            int ptr = _logs.Count - 1;
            int totalChars = 0;
            while (ptr > 0)
            {
                string cappedLog = _logs[ptr].Length <= 100 ? _logs[ptr] : _logs[ptr].Substring(0, 100);
                if (totalChars + cappedLog.Length > 1000) // 1000 is our best bet for a safe limit
                {
                    break;
                }
                totalChars += cappedLog.Length;
                logsCapped.Add(cappedLog);
                ptr--;
            }
            Json response = new Json() {
            {"command", Command.LogData.ToCommandString()},
            {"logs", logsCapped.ToArray()}
        };

            bot.SendMessage(response);

            WriteLog(bot, true, new Json() {
            {"command", Command.LogData.ToCommandString()},
            {"logs", new string[] { "Removed to prevent recursive logging" }}
        });
        }

        /* Handles a single bot message mid-game. Returns true if the message reflects a valid TakeAction message,
         * and the action was performed successfully.
         */
        private bool HandleResponse(Json response, IBot bot, bool actionAllowed)
        {
            try
            {
                WriteLog(bot, false, response);

                if (!response.ContainsKey(CommandExtensions.CommandText))
                {
                    SendErrorMessage(bot, ErrorType.InvalidInput);
                    return false;
                }
                Command cmd = CommandExtensions.FromCommandString(response[CommandExtensions.CommandText].ToString());

                switch (cmd)
                {
                    case Command.TakeAction:
                        if (!actionAllowed)
                        {
                            SendErrorMessage(bot, ErrorType.NotExpected);
                            return false;
                        }
                        return HandleTakeAction(response, bot);
                    case Command.SendChat:
                        if (!response.ContainsKey("message") || response["message"].ToString().Length == 0)
                        {
                            SendErrorMessage(bot, ErrorType.InvalidInput);
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
                Debug.Log($"Error handling message from Bot {bot.Name}: {e.Message}");
            }

            return false;
        }

        private bool HandleTakeAction(Json response, IBot bot)
        {
            if (!response.ContainsKey("action_type"))
            {
                SendErrorMessage(bot, ErrorType.InvalidInput);
                return false;
            }
            ActionType actionType;
            try
            {
                actionType = ActionTypeExtensions.FromActionString(response["action_type"].ToString());
            }
            catch (KeyNotFoundException knfe)
            {
                SendErrorMessage(bot, ErrorType.BadActionType);
                return false;
            }

            int raiseAmount = 0;
            if (actionType == ActionType.Raise)
            {
                if (!response.ContainsKey("raise_amount"))
                {
                    SendErrorMessage(bot, ErrorType.BadActionType);
                    return false;
                }
                if (!int.TryParse(response["raise_amount"].ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out raiseAmount))
                {
                    SendErrorMessage(bot, ErrorType.BadValue);
                    return false;
                }

                // Count digits after decimal
                var parts = response["raise_amount"].ToString().Split('.');
                if (parts.Length > 1 && parts[1].Length > 2)
                {
                    SendErrorMessage(bot, ErrorType.BadValue);
                    return false;
                }
            }
            // if (actionType == ActionType.Call) {
            //     raiseAmount = 
            // }
            return TakeAction(actionType, raiseAmount, bot);
        }

        private void SendErrorMessage(IBot bot, ErrorType error)
        {
            Json data = GetErrorMessageData(error);
            WriteLog(bot, true, data);
            bot.SendMessage(data);

        }

        private void SendSuccessMessage(IBot bot)
        {
            Json data = new Json() {
            {"command", Command.ConfirmAction.ToCommandString()},
            {"result", "success" }
        };
            WriteLog(bot, true, data);
            bot.SendMessage(data);
        }

        private Json GetErrorMessageData(ErrorType error)
        {
            return new Json() {
            {"command", Command.ConfirmAction.ToCommandString()},
            {"result", "error"},
            {"error", error.ToErrorString()},
        };
        }

        private Json GetBotRequestActionData(IBot bot)
        {
            return new Json() {
            {"command", Command.RequestAction.ToCommandString()},
            {"game_number", _gameId},
            {"hand_number", _handNumber},
            {"round_number", _roundStage},
            {"hand", Card.SerializeCardList(bot.GameData.Cards)},
            {"center_cards", Card.SerializeCardList(_centerCards)},
            {"players", IBot.SerializeBotsList(_bots, false) },
            {"highest_bid_value", _highestBidValue},
            {"total_pot_value", _totalPot}
        };
        }

        private Json GetHandResultData(List<Json> pots)
        {
            return new Json() {
            {"command", Command.HandResult.ToCommandString()},
            {"game_number", _gameId},
            {"hand_number", _handNumber},
            {"center_cards", Card.SerializeCardList(_centerCards)},
            {"players", IBot.SerializeBotsList(_bots, true) },
            {"pots", pots}
        };
        }
    }

}