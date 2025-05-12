using System.Text.Json;

namespace Server;

public class Game {
    private List<Bot> _bots;
    private Deck _deck;
    private List<Card> _centerCards = new List<Card>();
    private int _gameId;
    private int _handNumber;
    private RoundStage _roundStage = RoundStage.PreFlop;
    private float _totalPot = 0;
    
    private static int _idCounter = 0;
    
    /**
     * bots: need to add in random order
     */
    public Game(List<Bot> bots) {
        if (bots.Count < 2 || bots.Count > 6) {
            Console.Error.WriteLine("Invalid number of bots. must be between 2 and 6.");
        }
        
        _bots = bots;
        _gameId = _idCounter;
        _idCounter++;
    }

    public void PlayGame() {
        for (int i = 0; i < _bots.Count; i++) {
            PlayHand();
        }

    }

    private void PlayHand() {
        _deck = new Deck();
        foreach (Bot bot in _bots) {
            bot.GameData.NewHand(new List<Card>() {_deck.DrawCard(), _deck.DrawCard()});
        }
        
        //big and small blinds
        
        //play all rounds
        
        //showdown
        
        //move order of player
        Bot firstPlayer = _bots[0];
        _bots.RemoveAt(0);
        _bots.Add(firstPlayer);
        _handNumber++;
    }

    private void PlayRound() {
        foreach (Bot bot in _bots) {
            bot.GameData.NewRound();
        }

        foreach (Bot bot in _bots) {
            if (bot.GameData.RoundState != BotRoundState.NotPlayed) continue;
            
            bot.SendMessage(GetBotRequestActionData(bot));

            var response = bot.ReceiveMessageBlocking();
            HandleResponse(response, bot);    
        }
    }

    private void HandleResponse(Dictionary<string, string> response, Bot bot) {
        if(!response.ContainsKey(CommandExtensions.CommandText)) {
            bot.SendMessage(new Dictionary<string, string>() {
                { "error", "error"}
            });
            
        }
        Command cmd = CommandExtensions.FromCommandString(response[CommandExtensions.CommandText]);
        switch (cmd) {
            case Command.TakeAction:
                break;
        }
    }

    private Dictionary<string, string> GetBotRequestActionData(Bot bot) {
        return new Dictionary<string, string>() {
            {"command", Command.RequestAction.ToCommandString()},
            {"hand", Card.SerializeCardList(bot.GameData.Cards)},
            {"center_cards", Card.SerializeCardList(_centerCards)},
            {"game_number", _gameId.ToString()},
            {"hand_number", _handNumber.ToString()},
            {"round_number", ((int)_roundStage).ToString()},
            {"players", Bot.SerializeBotsList(_bots) },
            {"highest_bid_value", _bots.Max(_bot => _bot.GameData.PotValue).ToString()},
            {"total_pot_value", _totalPot.ToString()}
        };
    } 
}