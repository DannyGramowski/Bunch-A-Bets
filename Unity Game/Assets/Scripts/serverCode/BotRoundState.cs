using System.Collections.Generic;
using System.Linq;

namespace Server
{
    

    public enum BotRoundState {
        NotPlayed = 0,
        Folded = 1,
        Called = 2,
        Raised = 3,
        AllIn = 4
    }

    public static class BotRoundStateExtensions {
        private static readonly Dictionary<BotRoundState, string> RoundStateToString = new()
        {
            { BotRoundState.NotPlayed, "not_played" },
            { BotRoundState.Folded, "folded" },
            { BotRoundState.Called, "called" },
            { BotRoundState.Raised, "raised" },
            { BotRoundState.AllIn, "all_in" },
        };

        private static readonly Dictionary<string, BotRoundState> StringToRoundState = RoundStateToString
            .ToDictionary(kv => kv.Value, kv => kv.Key);

        public static string ToRoundStateString(this BotRoundState actionType)
        {
            return RoundStateToString[actionType];
        }

        public static BotRoundState FromRoundStateString(string value)
        {
            return StringToRoundState[value]; // Add safety if desired
        }
    }
}