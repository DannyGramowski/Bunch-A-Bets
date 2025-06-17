using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Server;


public class HttpServer {
    private static readonly int START_EXTERNAL_PORT = 26100;
    private static readonly int STOP_EXTERNAL_PORT = 26599;

    private static int GLOBAL_ID = 1;

    public static int GetGlobalBotID() {
        return GLOBAL_ID++;
    }

    /**
     * This is a blocking call
     * registerBot: function that takes in string name and returns tuple of (id, portNumber)
     */
    public static void Run(EpicFactory epicFactory) {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions {
            Args = new[] { $"ASPNETCORE_URLS=0.0.0.0:5000" },
            ApplicationName = "HttpRegisterServer",
            ContentRootPath = Directory.GetCurrentDirectory(),
            WebRootPath = "wwwroot",
        });
        builder.WebHost.UseUrls("http://0.0.0.0:5000");
        var app = builder.Build();

        app.MapPost("/register", async (HttpRequest req) =>
        {

            Console.WriteLine($"New Bot requested to register from {req.HttpContext.Connection.RemoteIpAddress?.ToString()}");
            string? bodyStr = await (new StreamReader(req.Body).ReadToEndAsync());
            try
            {
                Console.WriteLine(bodyStr);
                if (bodyStr == null)
                {
                    return Results.BadRequest(new { error = "Request body is required" });
                }
                Dictionary<string, JsonElement> body = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(bodyStr);

                JsonElement name;
                body.TryGetValue("name", out name);

                if ((name.ValueKind != JsonValueKind.String) || string.IsNullOrEmpty(name.GetString())) return Results.BadRequest(new { error = "Name is required" });
                if (name.GetString()?.Length > 30) return Results.BadRequest(new { error = "Names have a max length of 30 characters." });

                JsonElement testGameSize;
                body.TryGetValue("test_game_size", out testGameSize);

                int gameSize = 6;

                if ((testGameSize.ValueKind == JsonValueKind.Number) && testGameSize.GetInt32() > 1 && testGameSize.GetInt32() <= 6)
                {
                    gameSize = testGameSize.GetInt32();
                }

                JsonElement testHandCount;
                body.TryGetValue("test_hand_count", out testHandCount);

                int handCount = 6;

                if ((testHandCount.ValueKind == JsonValueKind.Number) && testHandCount.GetInt32() >= 1 && testHandCount.GetInt32() <= 24)
                {
                    handCount = testHandCount.GetInt32();
                }


                int botId = GetGlobalBotID();
                int portNumber = GetOpenPort();
                Bot newBot = new Bot(botId, portNumber, name.GetString(), Epic.STARTING_BANK);

                epicFactory.RegisterBot(newBot, gameSize, handCount);

                var data = new { id = botId, portNumber = portNumber };
                return Results.Json(data);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to initialize bot from /register message: {bodyStr}. Error: {e.Message} {e.StackTrace}");
            }
            return Results.Json(new { });
        });

        app.MapGet("/", (HttpRequest req) => {
            return Results.Text("Hello World!");
        });

        app.Run();
    }

    private static int GetOpenPort() {
        int randomPort = -1;
        for (int ct = 0; ct < 300; ct ++)
        {
            randomPort = Random.Shared.Next(START_EXTERNAL_PORT, STOP_EXTERNAL_PORT);
            try
            {
                var listener = new TcpListener(IPAddress.Any, randomPort); // Port 0 = let OS assign
                listener.Start();
                listener.Stop();
                break;
            }
            catch (SocketException)
            {
                Console.WriteLine("Tried to assign to in-use port " + randomPort.ToString());
            }
        }
        
        return randomPort;
    }
}