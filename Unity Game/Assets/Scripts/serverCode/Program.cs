// See https://aka.ms/new-console-template for more information
using System.Net.Sockets;
using System.Net;
using System;
using Server;

class Program {
    public const bool VERBOSE_DEBUGGING = true;
    public static void Main(string[] args) {
        if (args.Length == 1) {
            RandobotService.randobotFilename = args[0];
        }
        EpicFactory epicFactory = new EpicFactory();
        new Thread(() => HttpServer.Run(epicFactory)).Start();
    }
}