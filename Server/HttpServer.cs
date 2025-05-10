using Microsoft.AspNetCore.Mvc;

namespace Server;


public class HttpServer {
    // private WebApplication _app;
    // private Func<string, bool> _registerBot;
    
    /**
     * registerBot: function that takes in string name and returns tuple of (id, portNumber)
     */
    public static void run(Func<string, (int, int)> registerBot) {
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();
        
        app.MapPost("/register", (HttpRequest req) => {
            string name = req.Query["name"];
            
            if (string.IsNullOrEmpty(name))
            {
                return Results.BadRequest(new { error = "Name is required" });
            }
            
            var result = registerBot.Invoke(name);
            if (result.Item1 == -1) {
                Console.WriteLine("not found");
                return Results.NotFound();
            }

            var data = new { id = result.Item1, portNnumber = result.Item2 };
                Console.WriteLine("good, ", data);
            return Results.Json(data);
        });    
        
        app.Run();
    }

    // private IResult Register(string name) {
    //     return Results.Ok($"Bot '{name}' registered.");
    //
    // }
}

// var builder = WebApplication.CreateBuilder(args);
// var app = builder.Build();
//
// app.MapGet("/", () => "Hello World!");
//
// app.Run();