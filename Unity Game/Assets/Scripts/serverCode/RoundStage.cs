namespace Server;

public enum RoundStage {
    PreFlop = 0,
    Flop = 1, //3 cards
    Turn = 2, //4 cards
    River = 3, //5 cards
    ShowDown = 4, //no betting
}