namespace Server;
using System.Net.Sockets;
using System.Net;
using System.Threading;

public class BotSocket {
    public BotSocket(int port) {
        try {
            Thread thread = new Thread(() => CreateSocket(port));
            thread.Start();
        } catch (Exception e) {
          Console.WriteLine(e);
        }
    }

    private void CreateSocket(int port) {
        TcpListener server = new TcpListener(IPAddress.Parse(ServerUtils.IP), port);

        server.Start();
        
        Console.WriteLine($"Server started on {ServerUtils.IP}:{port} waiting for connections...");
        
        TcpClient client = server.AcceptTcpClient();
        
        Console.WriteLine("Client connected on port " + port);

        // while (true) {
            //look for input
        // }
    } 
}