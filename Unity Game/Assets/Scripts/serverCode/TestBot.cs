
using System;
using System.Collections.Generic;

namespace Server
{

    public class TestBot : IBot
    {
        public int ID => _id;

        public string Name => $"Test {_id.ToString()}";

        public int Bank { get => _bank; set => _bank = value; }

        public BotGameData GameData => _gameData;

        public DateTime LastChatTime { get => _lastChatTime; set => _lastChatTime = value; }


        private int _bank;
        private int _id;
        private BotGameData _gameData;
        private DateTime _lastChatTime;

        private static int TestBotId = 0;
        public TestBot(int bank = 0, List<Card>? cards = null, BotRoundState roundState = BotRoundState.NotPlayed, int potValue = 0, int potValueOfHand = 0)
        {
            _id = TestBotId;
            TestBotId++;

            _bank = bank;
            if (cards == null)
            {
                cards = new();
            }
            _gameData = new BotGameData(cards, roundState, potValue, potValueOfHand);
        }

        public int Bet(int amount)
        {
            if (amount > Bank)
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


        public bool HasMessageReceived()
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, object> ReceiveMessage()
        {
            throw new NotImplementedException();
        }

        public void SendMessage(Dictionary<string, object> message)
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            //to close _socket in real bot
        }

        public void SetEpic(Epic epic)
        {
            //implement if you want to test epic
        }

        public void TryStartEpic()
        {
            //implement if you want to test epic
        }

        public override int GetHashCode()
        {
            return ID;
        }

        public override bool Equals(object? obj)
        {
            if (obj is TestBot other)
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