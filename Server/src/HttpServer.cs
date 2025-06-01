using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Server;


public class HttpServer {
    // private WebApplication _app;
    // private Func<string, bool> _registerBot;

    private static int GLOBAL_ID = 1;

    /**
     * This is a blocking call
     * registerBot: function that takes in string name and returns tuple of (id, portNumber)
     */
    public static void Run(EpicFactory epicFactory)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = new[] { $"ASPNETCORE_URLS={ServerUtils.IP}:5000" },
            ApplicationName = "HttpRegisterServer",
            ContentRootPath = Directory.GetCurrentDirectory(),
            WebRootPath = "wwwroot",
        });
        var app = builder.Build();

        app.MapPost("/register", async (HttpRequest req) =>
        {
            bool isRandobot = req.HttpContext.Connection.RemoteIpAddress?.ToString() == "127.0.0.1"; // Please do not attempt to forge this, it's important to prevent recursive logic in game testing
            string? bodyStr = await (new StreamReader(req.Body).ReadToEndAsync());
            Console.WriteLine(bodyStr);
            if (bodyStr == null)
            {
                return Results.BadRequest(new { error = "Request body is required" });
            }
            Dictionary<string, JsonElement> body = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(bodyStr);

            JsonElement name;
            body.TryGetValue("name", out name);

            if ((name.ValueKind != JsonValueKind.String) || string.IsNullOrEmpty(name.GetString())) return Results.BadRequest(new { error = "Name is required" });

            JsonElement testGameSize;
            body.TryGetValue("test_game_size", out testGameSize);

            int gameSize = 6;

            if ((testGameSize.ValueKind == JsonValueKind.Number) && testGameSize.GetInt32() > 1 && testGameSize.GetInt32() <= 6)
            {
                gameSize = testGameSize.GetInt32();
            }

            int botId = GLOBAL_ID;
            int portNumber = GetOpenPort();
            Bot newBot = new Bot(botId, portNumber, name.GetString(), Epic.STARTING_BANK);

            epicFactory.RegisterBot(newBot, gameSize, isRandobot);

            var data = new { id = GLOBAL_ID, portNumber = portNumber };

            GLOBAL_ID++;

            return Results.Json(data);
        });

        app.Run();
    }

    private static int GetOpenPort() {
        var listener = new TcpListener(IPAddress.Loopback, 0); // Port 0 = let OS assign
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}