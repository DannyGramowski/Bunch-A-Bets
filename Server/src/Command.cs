
using System.Text.Json;
using Server;

namespace Server;

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Runtime.Serialization;

public enum Command {
    RequestAction,
    ConfirmAction,
    LogData,
    ReceiveChat,
    HandResult,
    TakeAction,
    GetLogs,
    SendChat,
}

public static class CommandExtensions {
    private static readonly Dictionary<Command, string> CommandToString = new()
    {
        { Command.RequestAction, "request_action" },
        { Command.ConfirmAction, "confirm_action" },
        { Command.LogData, "log_data" },
        { Command.ReceiveChat, "receive_chat" },
        { Command.HandResult, "hand_result" },
        { Command.TakeAction, "take_action" },
        { Command.GetLogs, "get_logs" },
        { Command.SendChat, "send_chat" },
    };

    private static readonly Dictionary<string, Command> StringToCommand = CommandToString
        .ToDictionary(kv => kv.Value, kv => kv.Key);

    public static string ToCommandString(this Command command)
    {
        return CommandToString[command];
    }

    public static Command FromCommandString(string value)
    {
        return StringToCommand[value]; // Consider TryGetValue + exception handling for safety
    }

    public static string CommandText => "command";
}
