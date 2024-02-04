open Modules.ConsoleWriter
open TcpClientProxy
open Application.Types
open Infrastructure.ClientProxy
open IrcCommands
open Marvel.Service
open ChatGpt.Service
open GitHub.Service
open Trivia.Service
open Auth0.Service

let mutable botState = Unidentified

let tcp = TcpProxy() :> ITcpProxy
let http = ClientProxy()
let irc = IrcBroadcaster(tcp) :> IIrcBroadcaster
let auth = Auth0Service(http) :> IAuth0Service
let triviaService = TriviaService(http, auth)
let triviaHandler = TriviaHandler(triviaService, irc)

let createDictionary http irc = 
    let marvelService = MarvelService(http, auth)
    let chatGptService = ChatGptService(http, auth)
    let gitHubService = GitHubService(http, auth)

    dict [
        "marvel", MarvelHandler(marvelService, irc) :> IMessageHandler
        "chatgpt", ChatGptHandler(chatGptService, irc) :> IMessageHandler
        "git", GitHubHandler(gitHubService, irc) :> IMessageHandler
        "trivia", triviaHandler :> IMessageHandler
        ]

let app = 
    {
        TcpProxy = tcp
        Irc = irc
        Handlers = createDictionary http irc
    }
                
let handleCommand (input:string) (message:string) = 
    async {
        let split = message.Split(' ', 2)
        let command = split[0]

        writeText <| Command input  

        match command with
        | "!coinflip" -> do! CoinFlip.Service.handleFlipCommand app.Irc.Privmsg
        | "!roll" -> do! Roll.Service.handleRollCommand app.Irc.Privmsg split[1]
        | "!trivia" -> do! app.Handlers["trivia"].Handle split
        | "!chatgpt" -> do! app.Handlers["chatgpt"].Handle split
        | "!marvel" -> do! app.Handlers["marvel"].Handle split
        | "!report" -> do! app.Handlers["git"].Handle [| input; split[1] |]
        | _ -> do! app.Irc.Privmsg "command not found 👻"    
    }

let handleStateConditions (message:ChannelMessage) = 
    async {
        match triviaHandler.State.questionStatus with
        | Trivia.Types.Disabled -> return ()
        | _ -> do! triviaHandler.CheckAnswer message
    }

let handleIdentification (line: string) = 
    async {
        do! app.Irc.IdentifyAndJoin line

        botState <- Identified
    }

let handleEstablishedMessages (message:ChannelMessage) (line:string) = 
    async {
        match message with
        | _ when message.Message.StartsWith("!") -> do! handleCommand line message.Message
        | _ when message.Message.Contains("PING") -> do! app.Irc.Ping line
        | _ when botState = Unidentified && message.Message.Contains("+iwx") -> do! handleIdentification line
        | _ -> writeText <| Input line
    }

async {
    do! app.Irc.InitializeCommunication()

    while(app.TcpProxy.Reader.EndOfStream = false) do
        let! line = app.TcpProxy.ReadAsync() 

        let messageInfo = getMessageInfo line

        match messageInfo with
        | Some message -> 
            do! handleEstablishedMessages message line
            do! handleStateConditions message
        | _ -> writeText <| Input line
} |> Async.RunSynchronously