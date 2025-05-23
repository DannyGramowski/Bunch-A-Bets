namespace Server;

using System.Text.Json;

public class Epic {
    public const int STARTING_BANK = 500;
    private List<Bot> _bots = new List<Bot>();
    private Game _game1;

    public Epic() {
        new Thread(() => HttpServer.Run(_bots)).Start();

        Thread.Sleep(2000);
        RandobotService.CreateRandobot();
        RandobotService.CreateRandobot();
        RandobotService.CreateRandobot();
        RandobotService.CreateRandobot();
        RandobotService.CreateRandobot();
        RandobotService.CreateRandobot();

        while (true)
        {
            string input = Console.ReadLine(); // Blocks until input is received
            Console.WriteLine("You typed: " + input);
            Console.WriteLine(_bots.Count);
            lock (_bots)
            {
                Game game = new Game(_bots);
                game.PlayGame();

                //need to detect start game from standard in to actuall start the game after everyone has registered
                //probably wait until both games are finished before starting new ones after the epic has begun
                // foreach (Bot bot in _bots)
                // {
                //     if (bot.HasMessageReceived())
                //     {
                //         var msg = bot.ReceiveMessage();
                //         Console.WriteLine($"msg received from bot: {bot.Name} is {JsonSerializer.Serialize(msg)}");
                //     }
                // }
            }
        }
    }

}