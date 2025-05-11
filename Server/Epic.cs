namespace Server;

public class Epic {
    private List<Bot> _bots = new List<Bot>();
    private Game _game1;
    
    public Epic() { 
        new Thread(() => HttpServer.Run(_bots)).Start();

        while (true) {
            
        }
    }
    
}