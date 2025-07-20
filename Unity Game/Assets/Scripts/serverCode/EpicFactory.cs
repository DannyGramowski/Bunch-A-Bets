using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace Server
{


    public class EpicFactory
    {
        private Epic? tournamentEpic;
        private bool isTournament = false;

        public bool RegisterBot(Bot bot, int requestedPlayers, int gameCount)
        {
            if (isTournament)
            {
                // just add the bot to the single existing epic
                if (tournamentEpic == null)
                {
                    Debug.Log("ERROR: Something is seriously wrong, it's tournament day but the tournamentEpic wasn't started.");
                    return false;
                }
                tournamentEpic.RegisterBot(bot);
            }
            else
            {
                Epic testEpic = new Epic(requestedPlayers, gameCount);
                testEpic.RegisterBot(bot);

                for (int i = 1; i < requestedPlayers; i++)
                {
                    Randobot randobot = new Randobot(HttpServer.GetGlobalBotID(), Epic.STARTING_BANK);
                    testEpic.RegisterBot(randobot);
                    Debug.Log($"Registered Randobot {randobot.Name}");
                }
            }
            return true;
        }
    }

}