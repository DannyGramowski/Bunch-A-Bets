// See https://aka.ms/new-console-template for more information
using System.Net.Sockets;
using System.Net;
using System;
using Server;

class Program {
    public static void Main(string[] args) {
        // Deck deck = new Deck();
        //
        // Console.WriteLine(deck);
        // HttpServer.Run(Register); //blocking call
        if (args.Length == 1)
        {
            RandobotService.randobotFilename = args[0];
        }
        EpicFactory epicFactory = new EpicFactory();
        new Thread(() => HttpServer.Run(epicFactory)).Start();
    }
}

// Console.WriteLine("Hello, World!");