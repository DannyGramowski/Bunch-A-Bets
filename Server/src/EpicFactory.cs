using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.Mvc;

namespace Server;


public class EpicFactory {
    private Epic? testEpic; // TODO will this be an issue if 2 people want to test their bot at the same timeOnly one can be set here at a time - if it's here, it's waiting for its randobots to connect
    private Epic? tournamentEpic;
    private List<Process?> processes = new List<Process?>();
    private bool isTournament = false;

    public bool RegisterBot(Bot bot, int requestedPlayers) {
        if (isTournament) {
            // just add the bot to the single existing epic
            if (tournamentEpic == null) {
                Console.WriteLine("ERROR: Something is seriously wrong, it's tournament day but the tournamentEpic wasn't started.");
                return false;
            }
            tournamentEpic.RegisterBot(bot);
        } else {
            testEpic = new Epic(requestedPlayers);
            testEpic.RegisterBot(bot);

            for (int i = 1; i < requestedPlayers; i++) {
                testEpic.RegisterBot(new Randobot(HttpServer.GetGlobalBotID(), Epic.STARTING_BANK));
            }
        }
        return true;
    }
}