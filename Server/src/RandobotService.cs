namespace Server;

using System.Diagnostics;


public class RandobotService {
    public static Process? CreateRandobot()
    {
        Console.WriteLine("create randobot");
        var psi = new ProcessStartInfo
        {
            FileName = "python", // Or "python3" on some systems
            Arguments = "../../Randobot/main.py", // Path to your Python script
            UseShellExecute = true, //TODO set to false. true is helpful for testing
        };

        return Process.Start(psi);
    }
}