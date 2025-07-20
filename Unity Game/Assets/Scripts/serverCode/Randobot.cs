
namespace Server
{
    using System;
    using System.Collections.Generic;
    using Json = System.Collections.Generic.Dictionary<string, object>;

    public class Randobot : IBot
    {
        public int ID => _id;

        public string Name => $"Randobot {_id}";

        public int Bank { get => _bank; set => _bank = value; }

        public BotGameData GameData => _gameData;

        public DateTime LastChatTime { get => _lastChatTime; set => _lastChatTime = value; }


        private int _bank;
        private int _id;
        private BotGameData _gameData;
        private DateTime _lastChatTime;

        private Queue<Json> botResponses = new();


        public Randobot(int id, int startingBank)
        {
            _id = id;
            _bank = startingBank;
            _gameData = new BotGameData();
        }

        public int Bet(int amount)
        {
            if (amount >= Bank)
            {
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

        public bool HasMessageReceived()
        {
            return botResponses.Count > 0;
        }

        public Json ReceiveMessage()
        {
            return botResponses.Dequeue();
        }

        private Json TakeAction(ActionType action, int betAmount = 0)
        {
            Json result = new() {
            {"command", "take_action"},
            {"action_type", ActionTypeExtensions.ToActionString(action)}
        };

            if (betAmount > 0)
            {
                result["raise_amount"] = betAmount.ToString();
            }

            return result;
        }

        private Json SendChat(string chatMessage)
        {
            Json result = new() {
            {"command", "send_chat"},
            {"message", chatMessage}
        };
            return result;
        }

        public void SendMessage(Json message)
        {
            string cmd = message["command"].ToString();

            if (cmd == CommandExtensions.ToCommandString(Command.RequestAction))
            {
                var random = new Random();
                int randomInt = random.Next(1, 11);

                if (randomInt <= 2)
                {
                    int highestBidValue = int.Parse(message["highest_bid_value"].ToString());
                    int raiseAmount = random.Next(1, 22) * 10;
                    if (raiseAmount > 200)
                    {
                        raiseAmount = 2000; //big boi bet
                    }
                    botResponses.Enqueue(TakeAction(ActionType.Raise, highestBidValue + raiseAmount));
                }
                else if (randomInt <= 4)
                {
                    botResponses.Enqueue(TakeAction(ActionType.Fold));
                    botResponses.Enqueue(SendChat("I always get the worst cards!"));
                }
                else
                {
                    botResponses.Enqueue(TakeAction(ActionType.Call));
                }
            }
        }

        public override int GetHashCode()
        {
            return ID;
        }

        public override bool Equals(object? obj)
        {
            if (obj is Randobot other)
            {
                return other.ID == ID;
            }
            return false;
        }

        public override string ToString()
        {
            return $"ID: {ID}, Name: {Name}, Bank: {Bank}, Cards: {string.Join(",", GameData.Cards)}";
        }
    }

}