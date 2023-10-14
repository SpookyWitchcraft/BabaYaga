open System
open Modules.ConsoleWriter
open Application.Types
open System.Collections.Generic

type ChannelMessage = 
    { UserInfo: string; Channel: string; Message: string }

let initialState = 
    { 
        question = None
        rounds = 0
        scores = new Dictionary<string, int>()
        botState = Unidentified
    }

let mutable state = initialState

Console.ForegroundColor <- ConsoleColor.DarkRed

let getMessageInfo (line:string) = 
    let split = line.Split(':')
    
    if split.Length < 3 then
        Some({ UserInfo = ""; Channel = ""; Message = line})
    else
        let messageDetails = split[1].Split(' ')
        if messageDetails.Length < 4 then
            None
        else
            Some({ UserInfo = split[1]; Channel = messageDetails[2]; Message = split[2]})

let handleCommand (input:string) (message:string) = 
    async {
        let split = message.Split(' ', 2)
        let command = split[0]

        writeText <| Command input

        match command with
        | "!coinflip" -> do! CoinFlip.Service.handleFlipCommand ()
        | "!roll" -> do! Roll.Service.handleRollCommand message
        | "!trivia" -> do! Trivia.Service.handleTriviaCommand split
        | "!chatgpt" -> do! ChatGpt.Service.handleGptCommand split[1]
        | "!marvel" -> do! Marvel.Service.handleMarvelCommand split[1]
        | "!report" -> do! GitHub.Service.handleGitHubCommand input split[1]
        | _ -> do! IrcCommands.privmsg "command not found 👻"    
    }

//clean up initial irc commands
//set app to 'identified'
//split state by module
//reuse http stuff
//handle http codes better
//handle timers better

async {
    do! IrcCommands.initializeCommunication

    while(TcpClientProxy.reader.EndOfStream = false) do
        let! line = TcpClientProxy.readAsync() 

        let messageInfo = getMessageInfo line

        match messageInfo with
        | Some a -> //(Trivia.Service.checkAnswer &state a.Message a.UserInfo)
                    match a with
                    | y when a.Message.StartsWith("!") -> do! handleCommand line y.Message
                    | _ when a.Message.Contains("PING") -> do! IrcCommands.ping line
                    | _ when a.Message.Contains("+iwx") -> do! IrcCommands.identifyAndJoin line
                    //| _ when state.botState = Unidentified && a.Message.Contains("+iwx") -> IrcCommands.identifyAndJoin line
                    | _ -> writeText <| Input line
        | _ -> Console.WriteLine(line)
} |> Async.RunSynchronously