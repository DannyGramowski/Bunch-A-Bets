namespace Server;

using System.Diagnostics;
using System.Text.Json;

public class Epic
{
    public const int STARTING_BANK = 500;
    private List<Bot> _bots = new List<Bot>();
    public int requestedPlayers;
    private List<Process?> botProcesses;
    private EpicFactory factory;

    public Epic(int requestedPlayers, EpicFactory epicFactory)
    {
        this.requestedPlayers = requestedPlayers;
        this.botProcesses = new List<Process?>();
        this.factory = epicFactory;
        Thread.Sleep(2000);
    }

    public void SetProcesses(List<Process?> botProcesses)
    {
        this.botProcesses = botProcesses;
    }

    public void RunTest()
    {
        lock (_bots)
        {
            Game game = new Game(_bots);
            game.PlayGame();
        }
        Console.WriteLine("Finished Test Game");
        foreach (Process? p in botProcesses)
        {
            p?.Kill();
        }
    }

    public void RunTournament()
    {
        while (true)
        {
            // TODO specify 15 games, and then finals
            lock (_bots)
            {
                Game game = new Game(_bots);
                game.PlayGame();
            }
        }
    }

    public void TryStart()
    {
        if (IsFilled())
        {
            new Thread(() => RunTest()).Start();
            // RunTest();
            factory.ClearTestEpic();
        }
    }

    public void RegisterBot(Bot bot)
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