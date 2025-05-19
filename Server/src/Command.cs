
using System.Text.Json;
using Server;

namespace Server;

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Runtime.Serialization;

[JsonConverter(typeof(JsonStringEnumConverter))]

public enum Command {
    [EnumMember(Value = "request_action")] RequestAction,
    [EnumMember(Value = "confirm_action")] ConfirmAction,
    [EnumMember(Value = "log_data")] LogData,
    [EnumMember(Value = "receive_chat")] ReceiveChat,
    [EnumMember(Value = "hand_result")] HandResult,
    [EnumMember(Value = "take_action")] TakeAction,
    [EnumMember(Value = "get_logs")] GetLogs,
    [EnumMember(Value = "send_chat")] SendChat,

}

public static class CommandExtensions {
    public static string ToCommandString(this Command command) {
        // Serialize to JSON, e.g., "request_action"
        string json = JsonSerializer.Serialize(command);
        return json.Trim('"'); // Remove surrounding quotes
    }
    
    public static Command FromCommandString(string value) {
        return JsonSerializer.Deserialize<Command>($"\"{value}\"");
    }

    public static string CommandText => "command";
}
