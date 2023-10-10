open System
open System.IO
open System.Net.Sockets
open Modules.Environment
open Modules.ConsoleWriter
open Application.Types
open System.Threading
open System.Collections.Generic

type ChannelMessage = 
    { UserInfo: string; Channel: string; Message: string }

let server = getEnvironmentVariables["SERVER"]
let port  = int (getEnvironmentVariables["PORT"])
let channel = getEnvironmentVariables["CHANNEL"]
let nick = getEnvironmentVariables["NICK"]
let password = getEnvironmentVariables["PASSWORD"]

//let initialState = 
//    let irc_client = new TcpClient();
//    irc_client.Connect(server, port)

//    let irc_reader = new StreamReader(irc_client.GetStream())
//    let irc_writer = new StreamWriter(irc_client.GetStream())
//    irc_writer.AutoFlush <- true
//    { 
//        client = irc_client
//        reader = irc_reader
//        writer = irc_writer
//        question = None
//        rounds = 0
//        scores = new Dictionary<string, int>()
//        botState = Unidentified
//    }

//let mutable state = initialState

//Console.ForegroundColor <- ConsoleColor.DarkRed

TcpClientProxy.writeAsync(sprintf "NICK %s\r\n" nick) |> Async.RunSynchronously
TcpClientProxy.writeAsync(sprintf "USER %s 0 * %s\r\n" nick nick) |> Async.RunSynchronously
    
let irc_ping (line:string) =
    let cookie = (line.Split ':')[1]
    let output = sprintf "PONG :%s %s" cookie server
    writeText <| Input line
    writeText <| Output output
    TcpClientProxy.writeAsync(output + "\r\n") |> Async.RunSynchronously

let identify (line:string) =
    let output = sprintf "nickserv identify %s\r\n" password
    writeText <| Input line
    TcpClientProxy.writeAsync(output) |> Async.RunSynchronously

let joinChannel (line:string) =
    let output = sprintf "JOIN %s" channel
    writeText <| Input line
    writeText <| Output output
    TcpClientProxy.writeAsync(output + "\r\n") |> Async.RunSynchronously

//let irc_privmsg (input : string) (message : string) =
//    state.writer.WriteLine(sprintf "PRIVMSG %s %s\r\n" channel message)
//    //this is the actual command someone typed
//    writeText <| Command input
//    //this is the actual output (shouldn't be in the same function)
//    writeText <| Output message

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

//let handleTriviaCommand (input:string) (triviaRounds:string) = 
//    let out = irc_privmsg input

//    match state.question with
//        | None -> 
//            state <- { state with question = Trivia.Service.getTriviaQuestion(); rounds = int triviaRounds }
//            out <| Trivia.Service.questionOutput state.question
//        | _ -> ()

//let handleCommand (input:string) (message:string) = 
//    let split = message.Split(' ', 2)
//    let command = split[0]

//    let out = irc_privmsg input

//    match command with
//    | "!coinflip" -> out <| CoinFlip.Service.flip ()
//    | "!roll" -> out <| Roll.Service.getDice split[1]
//    | "!trivia" -> 
//        let l = split.Length
//        if l = 2 then
//            handleTriviaCommand input split[1]
//        else
//            handleTriviaCommand input "0"
//    | "!chatgpt" -> 
//        let answer = ChatGpt.Service.getGptAnswer split[1]
//        answer 
//        |> List.iter out
//    | "!marvel" -> out <| Marvel.Service.getMarvelCharacter split[1]
//    | "!report" -> out <| GitHub.Service.createIssue input split[1]
//    | _ -> out "command not found 👻"

//let timer = new Timer(
//          TimerCallback (fun _ -> Trivia.Service.checkQuestionStatus(&state)),
//          state,
//          0,
//          500
//        )

let identifyAndJoin (line:string) = 
    identify line
    joinChannel line

//clean up initial irc commands
//set app to 'identified'
//split state by module
//reuse http stuff
//handle http codes better
//handle timers better
while(TcpClientProxy.reader.EndOfStream = false) do
    let line = TcpClientProxy.readAsync() |> Async.RunSynchronously

    let messageInfo = getMessageInfo line

    match messageInfo with
    | Some a -> //(Trivia.Service.checkAnswer &state a.Message a.UserInfo)
                match a with
                //| y when a.Message.StartsWith("!") -> handleCommand line y.Message
                | _ when a.Message.Contains("PING") -> irc_ping line
                | _ when a.Message.Contains("+iwx") -> identifyAndJoin line
                //| _ when state.botState = Unidentified && a.Message.Contains("+iwx") -> identifyAndJoin line
                | _ -> writeText <| Input line
    | _ -> Console.WriteLine(line)