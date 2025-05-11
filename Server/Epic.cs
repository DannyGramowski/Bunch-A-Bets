namespace Server;

using System.Text.Json;

public class Epic {
    private List<Bot> _bots = new List<Bot>();
    private Game _game1;
    
    public Epic() { 
        new Thread(() => HttpServer.Run(_bots)).Start();

        while (true) {
            lock (_bots) {
                
                foreach (Bot bot in _bots) {
                    if (bot.HasMessageReceived()) {
                        var msg = bot.ReceiveMessage();
                        Console.WriteLine($"msg received from bot: {bot.Name} is {JsonSerializer.Serialize(msg)}");
                    }
                }
            }
        }
    }
    
}