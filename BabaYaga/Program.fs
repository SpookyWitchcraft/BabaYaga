open Modules.ConsoleWriter
open Application.Types

let mutable botState = Unidentified

let getMessageInfo (line:string) = 
    let split = line.Split(':')
    
    match split.Length with
    | a when a < 3 -> Some({ UserInfo = ""; Channel = ""; Message = line})
    | _ -> 
        let messageDetails = split[1].Split(' ')
        match messageDetails with
        | a when a.Length < 4 -> 
            None
        | _ -> 
            Some({ UserInfo = split[1]; Channel = messageDetails[1]; Message = split[2]})
                
let handleCommand (input:string) (message:string) = 
    async {
        let split = message.Split(' ', 2)
        let command = split[0]

        writeText <| Command input

        match command with
        | "!coinflip" -> do! CoinFlip.Service.handleFlipCommand ()
        | "!roll" -> do! Roll.Service.handleRollCommand split[1]
        | "!trivia" -> do! Trivia.Service.handleTriviaCommand split
        | "!chatgpt" -> do! ChatGpt.Service.handleGptCommand split[1]
        | "!marvel" -> do! Marvel.Service.handleMarvelCommand split[1]
        | "!report" -> do! GitHub.Service.handleGitHubCommand input split[1]
        | _ -> do! IrcCommands.privmsg "command not found 👻"    
    }

let handleStateConditions (message:ChannelMessage) = 
    async {
        match Trivia.Service.state.questionStatus with
        | Trivia.Types.Disabled -> return ()
        | _ -> do! Trivia.Service.checkAnswer message
    }

let handleIdentification (line: string) = 
    async {
        do! IrcCommands.identifyAndJoin line

        botState <- Identified
    }

let handleEstablishedMessages (message:ChannelMessage) (line:string) = 
    async {
        match message with
        | _ when message.Message.StartsWith("!") -> do! handleCommand line message.Message
        | _ when message.Message.Contains("PING") -> do! IrcCommands.ping line
        | _ when botState = Unidentified && message.Message.Contains("+iwx") -> do! handleIdentification line
        | _ -> writeText <| Input line
    }

async {
    do! IrcCommands.initializeCommunication

    while(TcpClientProxy.reader.EndOfStream = false) do
        let! line = TcpClientProxy.readAsync() 

        let messageInfo = getMessageInfo line

        match messageInfo with
        | Some message -> 
            do! handleEstablishedMessages message line
            do! handleStateConditions message
        | _ -> writeText <| Input line
} |> Async.RunSynchronously