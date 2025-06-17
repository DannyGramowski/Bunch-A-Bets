# Tournament Rules

Howdy Partner! Welcome to the **Quick Draw Circuit**. Contestants will submit bots of their own creation to play Texas Hold'em Poker against other submitted bots. This document details the tournament structure, submission instructions, tournament rules, and the API for designing, connecting, and running your bot. The tournament will be held on **Friday, 8/22/25**, and for those who are in Rochester we will host all participants for a party at **115 Westmoreland Drive** (rides provided). If you are unable to be there in person (loser), the event will also be available through a Youtube Live.

The spirit of this friendly competition is to write your own algorithm for playing poker. We ask that you play to all the rules listed below, and don’t try to find loopholes or ways to get around the stated rules just to make the best bot. Remember to have fun! 

## Submissions

There is a $5 buy in for every bot submission and each player is allowed to submit up to 3 bots.

We will be using GitHub Classroom for everyone to submit their code. **CODE IS DUE BY AUGUST 13th**. If you want to submit multiple bots, they must all be in the same assignment but different folders. We will download and test everyone’s code over the following week. If we find issues with your code, we will reach out and you can submit an updated version. After the tournament, all bots will be open sourced.

You must Venmo $5 per bot by August 13th when code is due. Please send the money to @Daniel-Gramowski on Venmo with a funny message related to the tournament. Non-funny messages will be returned to the sender. The last 4 digits of my phone number are 3207.

### Required Files

> NOTE: Where specified, filenames must be exact.

#### Docker Config (`Dockerfile`)
This is the [Dockerfile](https://www.docker.com/) which will be used to run your bot. To help combat network and versioning issues, everyone **must** provide a docker file along with their bot. We will run everyone’s bot on our local machine for the tournament. Development and testing will still be done over the network from your personal computer. Testing does not require a dockerfile - for more instructions on testing before the tournament, see [Testing Your Bot](#testing-your-bot). Note that if your bot crashes during the tournament, it will not be restarted, and there is no rejoining the tournament once it has started (even if you [re-register your bot](#bot-registration)).

#### JSON Config (`config.json`)
This JSON file will tell us the name of your bot, as well as filenames for any assets used. It must be named *bot_config.json* and it must contain at least the *name* and *avatar_filename* fields. It can also optionally contain *particle_filename* and *sound_effect_filename*, where the value is the filename of the associated file. Here is an example JSON config:

```json
{
    "name": "Billy the Kid",
    "avatar_filename": "mugshot.png",
    "particle_filename": "tumbleweeds.png",
    "sound_effect_filename": "gunshots.mp3"
}
```

#### code (any filenames)
Your bot code in any language of your choice.

### Asset files

> NOTE: Please, no excess profanity here

Avatar (any filename) **MANDATORY**.
This is an image file that will show up next to your cards. It will show as a square in the final game, so any avatar that is not a square will be cropped.

Particle (any filename)
When you win a hand, particles will appear to celebrate your victory. You can optionally provide a custom image to appear instead of the default particles.

Sound Effect (any filename)
A sound effect will also play when you win a hand. This optional file must be a .wav or .mp3, and must be no longer than 5 seconds.

## Tournament Structure

The tournament will be broken up into 2 stages: The Trials, and High Noon.

### The Trials (Preliminaries)

Games are played with 6 bots and last 6 hands so every bot plays in every spot of the rotation. Players are drawn randomly from the pool of the bots who have played the least number of games and still have chips in their pot. The Trials are made up of 15 games or until there are 6 players remaining. If there are more than 6 bots after 15 rounds, the bots with the 6 largest pots will advance to the finals. Bots keep their pots between games in The Trials. Everyone starts the tournament with 5000 chips.

### High Noon (Finals)

Finals will involve 24 hands or until 1 player is left. Everyone will reset to a 5000 chip pot regardless of their final score from The Trials. Placements will be determined by when bots are eliminated i.e. 2nd bot whose pot goes to 0, will receive 5th place. If there is more than 1 bot remaining after 24 hands, ties will be decided by pot size.

## Payout

Players, not individual bots, receive prizes based on their final placement. If you end up with 2 bots in the finals, you will receive the payout for the higher placed bot.

4th / 5th place: $5

3rd place: 20% of remaining pot

2nd place: 30% of remaining pot

1st place: 50% of remaining pot (and eternal glory)

### Development Restrictions

- Any language is permitted, as long as you are able to perform the HTTP request and TCP connection
- No use of any external API calls or network requests
Memory limit: 100MB
  - This is achieved with Docker’s flag [`--memory=100m`](https://docs.docker.com/engine/containers/resource_constraints/#limit-a-containers-access-to-memory).
- CPU limit: 1 core
  - This is achieved with Docker’s flag [`--cpus=1`](https://docs.docker.com/engine/containers/resource_constraints/#configure-the-default-cfs-scheduler). Note that multithreading will not provide any performance gains.
- [Action Request](#request_action) Time Limit: 3 seconds
- File size limit: 10MB total
- You are allowed to look at generic strategy videos for poker, but **not videos for poker bots**.
- If you are interested in writing an AI to make decisions, some special rules apply:
  - You must write and train the AI model yourself. You are allowed to use existing libraries for creating machine learning models like PyTorch or Tensorflow, but your work must be your own (you’re not allowed to take inspiration from existing ML poker bots).
  - You may use an existing dataset to train the ML model.
  - You may exceed the 10MB file size limit for a single folder within your application which contains the weights for your ML model.
  - You are still limited to the CPU, memory, and time limits for your application.

> Additional Note: If you find any of the rules in this document to be inaccurate or nonspecific, please let us know.

# Poker Rules

Every hand, each bot is dealt 2 cards that they use in combination with the 5 community cards to make the best 5 card hand. These 5 community cards get revealed between the 4 betting rounds (see [Rounds](#rounds)). 

### Blinds

Each hand, two bots pay blinds that rotate clockwise. Due to the 6-hand game length, each player will act as both the Small Blind and Big Blind once.

- Small Blind: 50 chips (second to last in betting order)
- Big Blind: 100 chips (last in betting order)

### Rounds

0. Preflop: This is immediately after everyone has received their cards and before any community cards are shown. The blind bets count as bets for this round for the sake of calling and raising. Everyone must call(or check for big blind) up to 100 chips or they can raise or fold.

1. Flop: 3 Cards are shown and betting repeats.

2. Turn: 1 additional card is shown and betting repeats.

3. River: 1 additional card is shown and betting repeats.

4. Showdown: No betting happens here. This is where hands are compared and money is dispersed.

### Potential Actions

Every round you have 3 options available to you:

1. Call: You bet up to the current highest bet. If you have already bet equal to the highest bet nothing happens and you stay in the hand. If you have already bid 50 chips and the highest bid for the round is 100 chips, you must add 50 chips to the central pot
2. Raise: You bet higher than the current highest bet. Everyone else that has already either called or raised and is not all in, is now back in the bidding order and must perform an action in response to your raise. There is a cap of 5 raise cycles per betting round to prevent infinite loop situations.
3. Fold: Throw your hand into the center. You are no longer part of the hand and you forfeit any chips you have bet on this hand.

### All In

If you call (or raise), and the current highest bid is larger than you can afford (bank + previous bets), you are considered all in. This removes your bot from the bidding order. In the showdown, you can only win an amount from each player equal to your total bet for the hand - all remaining chips will be distributed to the 2nd place hand. If multiple players go all in on a hand, you may have more than 2 winners (see [hand_result](#hand_result)).

### Hand Rankings

The winner of the hand is determined based on the hand strengths below from all non-folded players. If bots have the same type of hand, ties are broken by the high card. For straights and flushes this means the highest value card. For a One or Two pair it will be the highest unshared card. If the best 5 cards are identical, the pot is split between the 2 players.

1. Royal Flush
2. Straight Flush
3. Four of a Kind
4. Full House
5. Flush
6. Straight
7. Three of a Kind
8. Two Pair
9. One Pair
10. High Card

### Rule Changes

All bots cards will be revealed at the end of every hand regardless of any actions taken. This is to provide some additional information to the bots playing, since there is no human element for them to analyze.

No max bets. This is more a clarification, since it is the standard, but you are allowed to bet as much as you want as long as you have chips for it. The minimum raise is 1 chip (and all bet amounts must be integers).

Each betting round is capped at circling the table 5 times - any raises after this point will be considered a call.

# Technical Details

### Connecting to the Server

Your bots will communicate with the game server through a TCP connection. The server will send your bot "messages" (as JSON strings), and your bot may respond via JSON string messages of its own. You will connect to the following IP:

> [64.201.234.55](http://64.201.234.55:5000)

You can receive a port to communicate with the server by registering your bot (see [Bot Registration](#bot-registration)). You do not need to send a message to indicate that your bot has connected - it is automatically connected once the TCP socket connection is established.

## Bot Registration

Bot registration is done through a HTTP API on port 5000, using the `/register` route. The format is as follows:

```
Method: POST
URL: http://64.201.234.55:5000/register
Content-Type: application/json
Request Body:
{
    "name": NAME,
    "test_game_size": TEST_GAME_SIZE,
    "test_hand_count": TEST_HAND_COUNT,
}
```

`name` (str): This is the name of your bot. It must EXACTLY match the name you register as. It must use the Unicode UTF-8 encoding (no emojis) and be **no longer than 30 characters**.

`test_game_size` (int): This optional parameter allows you to specify the number of bots[2-6] when running a test game on the server. Defaults to 6 if not specified. See [Testing Your Bot](#testing-your-bot)

`test_hand_count` (int): This optional parameter allows ou to specify the number of test hands[1-24] when running a test game on the server. Defaults to 6 if not specified. 

This endpoint returns a JSON response in the following format:

```json
{
"port_number": PORT_NUMBER,
"bot_id": BOT_ID
}
```

`port_number` (int): This exact port number must be used when you connect to the server via a socket. See [Connecting to the Server](#connecting-to-the-server). 

`bot_id` (int): This will be the ID your bot was assigned during tournament registration (internal to the server). This will be the same ID that shows up in Gameplay messages.


## Developing a Bot

Here are some tips to help you develop your poker bot. You are heavily encouraged to peruse the Randobot code and implement the functions associated with it. You are allowed to use any language of your choice as long as the language supports http calls and web sockets (This includes MATLAB if you’re brave).

This is a list of the required functions for your bot to participate in the tournament. You must be able to receive the request_action input from the server through the socket then send the correctly formatted take_action response back.

- [/register API call](#bot-registration)
- [Connect to the server on the given port via TCP](#connecting-to-the-server)

#### Server -> Bot:
- [request_action](#request_action)

#### Bot -> Server:
- [take_action](#take_action)

These are other messages that provide additional information and functionality. Your bot will still be able to participate without these, but they can provide a competitive edge, or some laughs.

#### Server -> Bot:
- [confirm_action](#confirm_action)
- [hand_result](#hand_result)
- [receive_chat](#receive_chat)
- [log_data](#log_data)

#### Bot -> Server:
- [send_chat](#send_chat)
- [get_logs](#get_logs)

### The Randobot

The Randobot is a dummy example bot which can be found in the `Randobot/` folder. The bot is written in Python, and provides a basic overview of how to connect to the server, start up a TCP connection, and handle the different types of message requests from the server. The folder also contains a properly configured Dockerfile which may serve as an example for you.

## Testing Your Bot

Before the tournament date, the gameplay server will be running 24/7 in "testing mode". You can connect to the server and play a test game against a specified number of bots. This will allow you to run a demo game against very, very bad bots just to make sure your bot runs properly and responds correctly to the message types. As specified in [Bot Registration](#bot-registration), you may pass the optional `test_game_size` parameter in order to change the game size. This game will run for a maximum of 6 hands. In addition you may pass the optional `test_hand_size` parameter to change the number of hands played depending on what you are trying to test. 

On the day of the competition, the /request command will change its functionality, and instead wait until the tournament begins to start sending requests to your bot.

Prior to the competition, we will test running your bot with Docker in order to ensure that it runs on our system and can play properly. Any minor bugs will not be accounted for (that’s on you, buddy).

If you are struggling with getting a Docker deployment working for your bot, feel free to reach out to the developers (Brandon and Danny) via the tournament Discord.

## Gameplay API

The primary gameplay loop consists of your bot receiving a message through the TCP socket (called a `request_action` message) and responding with a `take_action` message. In addition, your bot will receive messages at the end of the round with the result of the round, whenever another player sends a chat, and whenever your bot sends ANY take_action message or ANY invalid message to the server. You should plan to implement a primary loop for receiving and handling messages. Below is an "API" detailing the server and client (bot) message format - note that the only server message you *must* respond to is the `request_action` message.

### Server Messages

> #### **`request_action`**

The server will send this message to your bot when it is your turn to play. You must respond to this message within 3 seconds (including network delay), or your action will be forfeited and your bot will be considered folded for the round. Your bot will no longer receive this message once its personal pot is empty (including if you are all-in for a hand), or if it has folded the current hand. This 3 second window should be the only time you are performing relevant computations for the game. **Do not hog computation resources outside of your window**. Logic for receiving and responding to messages is fine.

```json
{
"command": "request_action",
"hand": [CARD, CARD],
"community_cards": [CARD, ...],
"game_number": GAME_NUMBER,
"hand_number": HAND_NUMBER,
"round_number": ROUND_NUMBER,
"players": [PLAYER, ...],
"highest_bid_value": HIGHEST_BID_VALUE,
"total_pot_value": TOTAL_POT_VALUE
}
```

`command` (str): The literal string "request_action"

`hand` (list[Card]): A list of cards that represents your dealt hand. All Cards use the following format, where both `value` and `suit` are one-letter strings:

```json
{
"value": "1" | "2" | ... | "9" | "T" | "J" | "Q" | "K" | "A",
"suit": "D" | "H" | "C" | "S"
}
```

`community_cards` (list[Card]): A list of cards that represents the cards publicly available at the table. This list changes size depending on the round number.

`game_number` (int): The index of the game within the tournament

`hand_number` (int): The index of the hand within the game 

`round_number` (int): The index of the round within the hand. Round 0 is the preflop, round 1 is after the flop, 2 is after the turn, and 3 is after the river (immediately before showdown).

`players` (list[Player]): A list of players in the game. All Players use the following format:

```json
{
    "id": BOT_ID, // this is the server’s id for the bot, guaranteed to be unique
"name": BOT_NAME, // this is the bot’s chosen name
"hand": [CARD, CARD] | [], // this is the bot’s hand. Only your player’s is revealed before the showdown
"State": ROUND_STATE, // this is the current state of the bot. Folded // if they haven’t acted this round, otherwise it is the action they // did. "not_played" | "folded" | "called" | "raised" | "all_in"
"round_bet": POT_VALUE, // this is the number of the chips the bot has bet so far this round
"hand_bet": TOTAL_BET, // the total amount this bot has bet in the current hand
"bank": BANK // the total number of chips the bot has remaining
}
```

NOTE: the bots in this list are sorted in betting order; i.e. the first player in the list is "under the gun" (first to bet), and the last person in the list is the big blind.

`highest_bid_value` (int): The current highest bid; calling will raise your current bet to this value or do nothing if your current bet is equal.

`total_pot_value` (int): The total number of chips that have been bet this hand. 

> #### **`confirm_action`**

The server will send this message to your bot when it sends ANY take_action message, or ANY invalid message. It dictates success when your bot’s action was received and taken successfully, or an error if your bot’s action was invalidly formatted, unrecognized, or otherwise improper.

```json
{
    "command": "confirm_action",
"result": "success" | "error",
"error": ERROR
}
```

`command` (str): The literal string "confirm_action"

`result` (str): Either the literal string "success" or "error", depending on the result of the action

`error` (str): Only gets sent if this is an error message. Indicates the type of error with your action. The following error types exist:

`INVALID_INPUT`: your message was malformed (forgot an argument, or was not proper JSON)

`BAD_ACTION_TYPE`: your take_action message indicated an action other than "call", "raise", or "fold"

`BAD_VALUE`: your input used the wrong type for an argument (such as a string instead of an integer for raise_amount)

`NOT_EXPECTED`: your message indicated an action which was not allowed at the time (such as a take_action without receiving a request_action)

`INVALID_RAISE_AMOUNT`: your "raise" action indicated a raise amount less than the current highest bid

> #### **`hand_result`**

The server will send this message to your bot during the showdown of each hand, revealing EACH player’s hand (regardless of their status or if they won the pot) and the winners of the main and side pots.

```json
{
    "command": "hand_result",
"game_number": GAME_NUMBER,
"hand_number": HAND_NUMBER,
"players": [PLAYER, ...],
"pots": [POT, ...]
}
```

`command` (str): The literal string "hand_result"

`game_number` (int): The index of the game within the tournament

`hand_number` (int): The index of the hand within the game (each game consists of 1 hand per player)

`players` (list[Player]): The list of players at the table. All players have their hands revealed. Pot sizes are calculated after the winnings are distributed.

`pots` (list[Pot]): The list of pots that were distributed as a result of the hand. This will usually just be the one main pot, but will include side pots if any players went all-in and won part of the full pot. Each Pot has the following format:

```json
{
    "pot_amount": POT_AMOUNT, // the size of the pot in chips, and how much was won
"winners": [PLAYER, ...] // the full JSON definition for all Players who split the pot (usually just one)
}
```

> #### **`receive_chat`**

The server will send this message to your bot whenever it receives a chat from any bot (including the same bot). Any more than 1 chat message every 5 seconds will be ignored, and not forwarded to other bots.

```json
{
    "command": "receive_message",
"message": MESSAGE,
"author": AUTHOR
}
```

`command` (str): The literal string "receive_message"

`message` (str): The message sent from the bot

`author` (Player): The player who sent the message

> #### **`log_data`**

The server will send this message to your bot in response to a `get_logs` message. You should NOT parse and use these logs in your bot in order to make decisions, as it is meant as a developer tool. A bot which uses these logs to make decisions will be disqualified.

```json
{
    "command": "log_data",
"logs": [LOG, ...],
}
```

`command` (str): The literal string "log_data"

`logs` (list[str]): A list of logs from the server. This will be capped at 2k characters (2k bytes), so if you are planning on using this feature, make sure to keep your receive buffer at least that large, otherwise the incoming message will not be valid JSON.

### Client Messages

> #### **`take_action`**

Your bot should send this message in response to a `request_action` message from the server. If the server does not receive a valid action from your bot within 3 seconds of it requesting an action, you will be considered to have folded your hand. The server will use the first valid `take_action` message it receives within the 3-second limit - so if your message was invalid (see `confirm_action`), you do have a chance to submit another `take_action` message.

```json
{
    "command": "take_action",
"action": "call" | "raise" | "fold",
"raise_amount": RAISE_AMOUNT
}
```

`command` (str): The literal string "take_action"

`action` (str): One of the literal strings "call", "raise", or "fold". Calling raises your current bet to the current highest bid, which will be 100 if no bets have been made yet. Raising raises your current bet to exactly `raise_amount` (so if you had 60 chips in for the round, and raised to 150, your bot puts 90 chips into the pot). Folding bets nothing and removes you from the hand. Any time your bot attempts to call or raise to more chips than it currently has, it will bet all its chips and be considered "all in".

`raise_amount` (int): The amount to raise the current highest bid to. Does nothing if the action type is not "raise". This MUST be at least the current highest bid, or else will be considered an invalid action.

> #### **`send_chat`**

Sending this message to the server will send a chat message to all players (who will receive a `receive_chat` message). The chat will also show in the UI during the tournament. You are NOT permitted to use this feature to collude strategically with specific players, but you may prepare responses to other bot’s chat messages for the bit. Any more than 1 chat message every 5 seconds will be ignored, and not forwarded to other bots.

```json
{
    "command": "send_chat",
"message": MESSAGE
}
```

`command` (str): The literal string "send_chat"

`message` (str): A message to send in the UI as well as to other players. It must not contain any of the following characters
    - Curly Braces `{` or `}`
    - Unescaped Double Quotes `"`
    - Newlines `\n`
    - Any characters not in the Unicode UTF-8 encoding

> #### **`get_logs`**

Requests a log history from the server, which will be matched with a `log_data` response message. You should NOT parse and use these logs in your bot in order to make decisions, as it is meant as a developer tool. A bot which uses these logs to make decisions will be disqualified.

```json
{
    "command": "get_logs"
}
```

`command` (str): The literal string "get_logs"
