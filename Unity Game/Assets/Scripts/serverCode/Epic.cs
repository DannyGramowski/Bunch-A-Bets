namespace Server;

using System.Diagnostics;
using System.Text.Json;

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
            try
            {
                Game game = new Game(_bots, false);
                game.PlayGame(handCount);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Something major failed when running game. Error: {e.Message} {e.StackTrace}");
            }
            
        }   
        Console.WriteLine("Finished Test Game");
        try
        {
            foreach (IBot b in _bots)
            {
                b.Close();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to close bot sockets. Error: {e.Message} {e.StackTrace}");
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