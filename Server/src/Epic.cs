namespace Server;

using System.Diagnostics;
using System.Text.Json;

public class Epic
{
    public const int STARTING_BANK = 5000;
    private List<IBot> _bots = new List<IBot>();
    public int requestedPlayers;

    public Epic(int requestedPlayers)
    {
        this.requestedPlayers = requestedPlayers;
        Thread.Sleep(2000);
    }

    public void RunTest()
    {
        lock (_bots)
        {
            Game game = new Game(_bots, false);
            game.PlayGame(24);
        }   
        Console.WriteLine("Finished Test Game");
        foreach (IBot b in _bots)
        {
            b.Close();
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

    // public void TryStart()
    // {
    //     if (IsFilled())
    //     {
    //         new Thread(() => RunTest()).Start();
    //         // RunTest();
    //         factory.ClearTestEpic();
    //     }
    // }

    public void RegisterBot(IBot bot)
    {
        lock (_bots)
        {
            // bot.SetEpic(this);
            _bots.Add(bot);
        }
    }

    public bool IsFilled()
    {
        return _bots.Count >= requestedPlayers;
    }
}