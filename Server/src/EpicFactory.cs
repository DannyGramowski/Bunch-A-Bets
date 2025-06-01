using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.Mvc;

namespace Server;


public class EpicFactory
{
    private Epic? testEpic; // Only one can be set here at a time - if it's here, it's waiting for its randobots to connect
    private Epic? tournamentEpic;
    private List<Process?> processes = new List<Process?>();
    private bool isTournament = false;

    public bool RegisterBot(Bot bot, int requestedPlayers, bool isRandobot)
    {
        if (isTournament)
        {
            // just add the bot to the single existing epic
            if (tournamentEpic == null)
            {
                Console.WriteLine("ERROR: Something is seriously wrong, it's tournament day but the tournamentEpic wasn't started.");
                return false;
            }
            tournamentEpic.RegisterBot(bot);
        }
        else
        {
            if (!isRandobot || testEpic == null) // This allows for testing *with* the randobot
            {
                int timeout = 200; // 20 seconds to allow randobots to connect. If not, force start last game
                while (testEpic != null)
                {
                    Thread.Sleep(100);
                    timeout--;
                    if (timeout <= 0)
                    {
                        // If it's taking this long, something happened (maybe origin bot disconnected) - kill all bots, never run the game
                        foreach (Process? p in processes)
                        {
                            p?.Kill();
                        }
                        testEpic = null;
                    }
                }
                testEpic = new Epic(requestedPlayers, this);
                testEpic.RegisterBot(bot);
                processes = new List<Process?>();
                for (int i = 1; i < requestedPlayers; i++)
                {
                    processes.Add(RandobotService.CreateRandobot());
                }
                testEpic.SetProcesses([.. processes]);
            }
            else
            {
                if (testEpic != null)
                {
                    lock (testEpic)
                    {
                        testEpic.RegisterBot(bot);
                    }
                }
            }
        }
        return true;
    }

    public void ClearTestEpic()
    {
        testEpic = null;
    }
}