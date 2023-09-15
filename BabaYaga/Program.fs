open System
open System.IO
open System.Net.Sockets
open Modules.Environment
open Application.Types
open Trivia.Types
open System.Threading

type ChannelMessage = 
    { UserInfo: string; Channel: string; Message: string }

type ConsoleMessage = 
    | Command of string
    | Input of string
    | Output of string

let initialState = 
    let server = getEnvironmentVariables["SERVER"]
    let port  = int (getEnvironmentVariables["PORT"])
    let irc_client = new TcpClient();
    irc_client.Connect(server, port)

    let irc_reader = new StreamReader(irc_client.GetStream())
    let irc_writer = new StreamWriter(irc_client.GetStream())
    irc_writer.AutoFlush <- true
    { 
        client = irc_client
        reader = irc_reader
        writer = irc_writer
        question = None
    }

let mutable state = initialState

let server = getEnvironmentVariables["SERVER"]
let port  = int (getEnvironmentVariables["PORT"])
let channel = getEnvironmentVariables["CHANNEL"]
let nick = getEnvironmentVariables["NICK"]

Console.ForegroundColor <- ConsoleColor.DarkRed



state.writer.WriteLine(sprintf "NICK %s\r\n" nick)
state.writer.WriteLine(sprintf "USER %s %s %s %s\r\n" nick nick nick nick)

let writeText (input:ConsoleMessage) = 
    match input with
    | Command message -> 
        Console.ForegroundColor <- ConsoleColor.Red
        Console.WriteLine(message)
    | Input message -> 
        Console.ForegroundColor <- ConsoleColor.DarkRed
        Console.WriteLine(message)
    | Output message -> 
        Console.ForegroundColor <- ConsoleColor.DarkYellow
        Console.WriteLine(message)
    
let irc_ping (writer : StreamWriter) (line:string) =
    let cookie = (line.Split ':')[1]
    let output = sprintf "PONG :%s %s\r\n" cookie server

    writer.WriteLine(output)

let joinChannel (writer : StreamWriter) =
    let output = sprintf "JOIN %s\r\n" channel
    writer.WriteLine(output)

let irc_privmsg (input : string) (message : string) =
    state.writer.WriteLine(sprintf "PRIVMSG %s %s\r\n" channel message)
    writeText <| Command input
    writeText <| Output message

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
    let split = message.Split(' ', 2)
    let command = split[0]

    let out = irc_privmsg input

    match command with
    | "!coinflip" -> out <| CoinFlip.Service.flip ()
    | "!roll" -> out <| Roll.Service.getDice split[1]
    | "!trivia" -> 
        match state.question with
        | None -> 
            state <- { state with question = Trivia.Service.getTriviaQuestion() }
            out <| Trivia.Service.questionOutput state.question
        | Some questionStatus ->
            match questionStatus with
            | TimesUp _ -> 
                state <- { state with question = Trivia.Service.getTriviaQuestion() }
                out <| Trivia.Service.questionOutput state.question
            | _ -> ()
    | "!chatgpt" -> 
        let answer = ChatGpt.Service.getGptAnswer split[1]
        answer 
        |> List.iter out
    | "!marvel" -> out <| Marvel.Service.getMarvelCharacter split[1]
    | "!report" -> out <| GitHub.Service.createIssue input split[1]
    | _ -> out "command not found 👻"

let timer = new Timer(
          TimerCallback (fun _ -> Trivia.Service.checkQuestionStatus(&state)),
          state,
          0,
          500
        )

while(state.reader.EndOfStream = false) do
    let line = state.reader.ReadLine()

    let messageInfo = getMessageInfo line

    match messageInfo with
    | Some a -> (Trivia.Service.checkAnswer &state a.Message a.UserInfo)
                match a with
                | y when a.Message.StartsWith("!") -> handleCommand line y.Message
                | _ when a.Message.Contains("PING") -> irc_ping state.writer line
                | _ when a.Message.Contains("+iwx") -> joinChannel state.writer
                | _ -> writeText <| Input line
    | _ -> Console.WriteLine(line)