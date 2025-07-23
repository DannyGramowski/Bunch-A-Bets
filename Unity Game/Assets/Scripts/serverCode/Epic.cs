
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Server
{


    public class Epic
    {
        public const int STARTING_BANK = 5000;
        private List<IBot> _bots = new List<IBot>();
        public int requestedPlayers;
        private int handCount;

        public Epic(int requestedPlayers, int handCount)
        {
            this.requestedPlayers = requestedPlayers;
            this.handCount = handCount;
            Thread.Sleep(2000);
        }

        public void RunTest()
        {
            lock (_bots)
            {
                    Debug.Log($"Thread ID: {System.Threading.Thread.CurrentThread.ManagedThreadId}");
                try
                {
                    Game game = new Game(_bots, true); // TODO eh
                    game.PlayGame(handCount);
                }
                catch (Exception e)
                {
                    Debug.Log($"Something major failed when running game. Error: {e.Message} {e.StackTrace}");
                }

            }
            Debug.Log("Finished Test Game");
            try
            {
                foreach (IBot b in _bots)
                {
                    b.Close();
                }
            }
            catch (Exception e)
            {
                Debug.Log($"Failed to close bot sockets. Error: {e.Message} {e.StackTrace}");
            }
        }

        public void RunTournament()
        {
            while (true)
            {
                // TODO specify 15 games, and then finals
                lock (_bots)
                {
                    Game game = new Game(_bots, true);
                    game.PlayGame(6);
                }
            }
        }

        public void TryStart()
        {
            if (IsFilled())
            {
                new Thread(() => RunTest()).Start();
            }
        }

        public void RegisterBot(IBot bot)
        {
            lock (_bots)
            {
                bot.SetEpic(this);
                _bots.Add(bot);
            }
        }

        public bool IsFilled()
        {
            return _bots.Count >= requestedPlayers;
        }
    }

}