
using System;
using System.Diagnostics;
using UnityEngine;

namespace Server
{



    public class RandobotService
    {
        public static string randobotFilename = "../../Randobot/main.py";
        public static Process? CreateRandobot()
        {
            UnityEngine.Debug.Log("create randobot");
            var psi = new ProcessStartInfo
            {
                FileName = "python", // Or "python3" on some systems
                Arguments = randobotFilename, // Path to your Python script
                UseShellExecute = true, //TODO set to false. true is helpful for testing
            };

            return Process.Start(psi);
        }
    }

}