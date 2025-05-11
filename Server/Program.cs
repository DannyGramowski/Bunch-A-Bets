// See https://aka.ms/new-console-template for more information
using System.Net.Sockets;
using System.Net;
using System;
using Server;

class main {
    public static void Main() {
        // Deck deck = new Deck();
        //
        // Console.WriteLine(deck);
        // HttpServer.Run(Register); //blocking call
        Epic epic = new Epic();//blocking
    }

    private static (int, int) Register(string name) {
        Console.WriteLine("Registering " + name);
        return (1, 2);
    }

    private static void StartServer() {
        TcpListener server = new TcpListener(IPAddress.Parse("127.0.0.1"), 8080);

        server.Start();
        
        Console.WriteLine("Server started on 127.0.0.1:8080 waiting for connections...");
        
        TcpClient client = server.AcceptTcpClient();
        
        Console.WriteLine("Client connected.");
    }
}

// Console.WriteLine("Hello, World!");