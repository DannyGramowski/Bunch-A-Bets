// See https://aka.ms/new-console-template for more information
using System.Net.Sockets;
using System.Net;
using System;
using Server;
using System.Threading;
using UnityEngine;

class Program
{
    private static readonly System.Random threadSafeRandom = new System.Random();
    public const bool VERBOSE_DEBUGGING = true;
    public static void Main()
    {
        EpicFactory epicFactory = new EpicFactory();
        new Thread(() => HttpServer.Run(epicFactory)).Start();
        Debug.Log("Out of here");
    }

    public static int Random(int a, int b)
    {
        int value;
        lock (threadSafeRandom)
        {
            value = threadSafeRandom.Next(a, b);
        }
        return value;
    }
}