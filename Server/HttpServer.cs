using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.Mvc;

namespace Server;


public class HttpServer {
    // private WebApplication _app;
    // private Func<string, bool> _registerBot;
    
    /**
     * This is a blocking call
     * registerBot: function that takes in string name and returns tuple of (id, portNumber)
     */
    public static void Run(List<Bot> tourneyBots) {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions {
            Args = new []{$"ASPNETCORE_URLS={ServerUtils.IP}:5000"},
            ApplicationName = "HttpRegisterServer",
            ContentRootPath = Directory.GetCurrentDirectory(),
            WebRootPath = "wwwroot",
        });
        var app = builder.Build();
        
        app.MapPost("/register", (HttpRequest req) => {
            string? name = req.Query["name"];
            
            if (string.IsNullOrEmpty(name)) return Results.BadRequest(new { error = "Name is required" });

            int botId = tourneyBots.Count;
            int portNumber = GetOpenPort();
            Bot newBot = new Bot(botId, portNumber, name, 5.00f);
            
            lock (tourneyBots) {
                tourneyBots.Add(newBot);
            }
            
            var data = new { id = tourneyBots.Count, portNumber = portNumber };
            return Results.Json(data);
        });

        app.MapDelete("/register", (HttpRequest req) => {
            string? idStr = req.Query["id"];
            
            if (string.IsNullOrEmpty(idStr)) return Results.BadRequest(new { error = "id is required" });
            int id = int.Parse(idStr);

            Bot botToRemove = tourneyBots.Find(bot => bot.ID == id) ?? null;
            if (botToRemove == null) return Results.BadRequest(new { error = "Bot not found" });
            tourneyBots.Remove(botToRemove);
            return Results.Ok();
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