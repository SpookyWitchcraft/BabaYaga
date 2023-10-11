﻿open System
open System.IO
open System.Net.Sockets
open Modules.Environment
open Modules.ConsoleWriter
open Application.Types
open System.Threading
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

let handleTriviaCommand (input:string) (triviaRounds:string) = 
    let out = irc_privmsg input

    match state.question with
        | None -> 
            state <- { state with question = Trivia.Service.getTriviaQuestion(); rounds = int triviaRounds }
            out <| Trivia.Service.questionOutput state.question
        | _ -> ()

let handleCommand (input:string) (message:string) = 
    let split = message.Split(' ', 2)
    let command = split[0]

    let out = irc_privmsg input

    match command with
    | "!coinflip" -> out <| CoinFlip.Service.flip ()
    | "!roll" -> out <| Roll.Service.getDice split[1]
    | "!trivia" -> 
        let l = split.Length
        if l = 2 then
            handleTriviaCommand input split[1]
        else
            handleTriviaCommand input "0"
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



//clean up initial irc commands
//set app to 'identified'
//split state by module
//reuse http stuff
//handle http codes better
//handle timers better
async {
    while(TcpClientProxy.reader.EndOfStream = false) do
        let! line = TcpClientProxy.readAsync() 

        let messageInfo = getMessageInfo line

        match messageInfo with
        | Some a -> //(Trivia.Service.checkAnswer &state a.Message a.UserInfo)
                    match a with
                    | y when a.Message.StartsWith("!") -> handleCommand line y.Message
                    | _ when a.Message.Contains("PING") -> irc_ping line
                    | _ when a.Message.Contains("+iwx") -> identifyAndJoin line
                    | _ when state.botState = Unidentified && a.Message.Contains("+iwx") -> identifyAndJoin line
                    | _ -> writeText <| Input line
        | _ -> Console.WriteLine(line)
} |> Async.RunSynchronously