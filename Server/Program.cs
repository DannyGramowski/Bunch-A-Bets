// See https://aka.ms/new-console-template for more information
using System.Net.Sockets;
using System.Net;
using System;

class Server {
    public static void Main() {
        TcpListener server = new TcpListener(IPAddress.Parse("127.0.0.1"), 8080);

        server.Start();
        
        Console.WriteLine("Server started on 127.0.0.1:8080 waiting for connections...");
        
        TcpClient client = server.AcceptTcpClient();
        
        Console.WriteLine("Client connected.");
        
        
    }
}

// Console.WriteLine("Hello, World!");