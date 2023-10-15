module Trivia.Service

open Trivia.Types
open Application.Types
open Infrastructure.ClientProxy
open Modules.Environment

open Newtonsoft.Json
open System.Net.Http.Headers
open System.Diagnostics
open System
open System.Collections.Generic
open System.Linq
open Modules.ConsoleWriter
open System.Timers
open System.Threading

let initialState = 
    { 
        questionStatus = Disabled
        rounds = 0
        scores = new Dictionary<string, int>()
    }

let mutable state = initialState

let get () = 
    async {
        let! token = Auth0.Service.getToken ()

        client.DefaultRequestHeaders.Authorization <- new AuthenticationHeaderValue("Bearer", token)

        let! response = client.GetStringAsync(buildUrl "/api/trivia") |> Async.AwaitTask
        
        let tq = JsonConvert.DeserializeObject<TriviaQuestion>(response)

        return tq
    } 
    
let getTriviaQuestion () = 
    async {
        let! triviaQuestion = get ()

        let currentTime = Stopwatch.GetTimestamp()

        return NeedsHint (currentTime, triviaQuestion)
    }

let questionOutput (questionStatus:QuestionStatus) = 
    match questionStatus with
        | TimesUp (_, b) | NeedsHint (_, b) | HasHint(_, b) ->
            $"[{b.Id}]: {b.Question}"

//let updateScores (winner:string) = 
//    if state.scores.ContainsKey winner then
//        state.scores[winner] <- state.scores[winner] + 1
//    else
//        state.scores.Add(winner, 1)

//let findWinner () = 
//    if state.scores.Count < 1 then
//        let output = "The round is now over!  No one scored!"
//        writeText <| Output output
//        output
//    else
//        let best = int <| state.scores.Values.Max()
//        let winners = 
//            state.scores
//            |> Seq.filter (fun kvp -> kvp.Value = best)
//            |> Seq.map (fun kvp -> kvp.Key)
//            |> Seq.toArray
//            |> String.concat ", "
//        let output = $"The Round is now over!  With a score of {best}, the win goes to: {winners}"
//        writeText <| Output output
//        output

let checkAnswer (message:ChannelMessage) = 
    async {
        match state.questionStatus with
        | TimesUp (_, b) | NeedsHint (_, b) | HasHint(_, b) -> 
            let results = b.Answer =? message.Message

            let user = (message.UserInfo.Split(':')[0]).Split('!')[0]

            if results then 
                let output = sprintf "PRIVMSG %s %s" getEnvironmentVariables["CHANNEL"] $"{user} wins!  The answer is {b.Answer}."
                TcpClientProxy.writeAsync(output) 
                writeText <| Output output
                updateScores user

                let roundsLeft = state.rounds - 1
                if roundsLeft > 0 then
                    let! q = getTriviaQuestion()
                    let m = questionOutput q
                    let output = sprintf "PRIVMSG %s %s" getEnvironmentVariables["CHANNEL"] m
                    TcpClientProxy.writeAsync(output) 
                    state <- { state with questionStatus = q; rounds = roundsLeft }
                    writeText <| Output output
                else
                    let winner = findWinner ()
                    let output = sprintf "PRIVMSG %s %s" getEnvironmentVariables["CHANNEL"] winner
                    TcpClientProxy.writeAsync(output) 
                    state <- { state with questionStatus = Disabled; scores = new Dictionary<string, int>() }
                    writeText <| Output output
            else
                ()
    }

//let createHint (answer:string) = 
//    let rand = new Random()
//    let values = List.init answer.Length (fun a -> 
//        let next = rand.Next(0, 3)
//        if next = 0 || not (Char.IsLetter answer[a] || Char.IsDigit answer[a]) then
//            string answer[a]
//        else
//            "*"
//        )
    
//    let asterisks = String.concat "" values
//    let index = rand.Next(0, answer.Length)
//    let pre = asterisks |> String.mapi (fun i x -> if i = index then answer[i] else x)
//    let final = if pre.Contains('*') then pre else pre |> String.mapi (fun i x -> if i = index then '*' else x)
//    "Here's a hint: " + final

let checkQuestionStatus = 
    async {
        match state.questionStatus with
        | TimesUp (_, y) ->
            let roundsLeft = state.rounds - 1
            if roundsLeft > 0 then
                let tuOutput = sprintf "PRIVMSG %s %s" getEnvironmentVariables["CHANNEL"] $"Times up! The answer is {y.Answer}"
                TcpClientProxy.writeAsync(tuOutput) 
                let! q = getTriviaQuestion()
                let m = questionOutput q
                let output = sprintf "PRIVMSG %s %s" getEnvironmentVariables["CHANNEL"] m
                TcpClientProxy.writeAsync(output) 
                state <- { state with questionStatus = q; rounds = roundsLeft }
                writeText <| Output tuOutput
            else
                let output = sprintf "PRIVMSG %s %s" getEnvironmentVariables["CHANNEL"] $"Times up! The answer is {y.Answer}"
                TcpClientProxy.writeAsync(output) 
                let winner = findWinner ()
                let winnerOutput = sprintf "PRIVMSG %s %s" getEnvironmentVariables["CHANNEL"] winner
                TcpClientProxy.writeAsync(winnerOutput) 
                state <- { state with questionStatus = Disabled; scores = new Dictionary<string, int>() }
                writeText <| Output output
        | HasHint (x, y) -> 
            let elapsed = (Stopwatch.GetTimestamp() - x) / Stopwatch.Frequency
            if elapsed >= 20 then 
                state <- { state with questionStatus = TimesUp (x, y) }
            else 
                ()
        | NeedsHint (x, y) -> 
            let elapsed = (Stopwatch.GetTimestamp() - x) / Stopwatch.Frequency

            if elapsed >= 10 then 
                let output = sprintf "PRIVMSG %s %s" getEnvironmentVariables["CHANNEL"] (createHint y.Answer)
                TcpClientProxy.writeAsync(output) 
                writeText <| Output output
                state <- { state with questionStatus = HasHint (x, y) }
            else
                ()
        | _ -> ()                                   
    }

let timer = new Timer(
        TimerCallback (fun _ -> async { do! checkQuestionStatus } |> Async.StartImmediate),
        null,
        0,
        500
    )

timer.Change(Timeout.Infinite, Timeout.Infinite)
timer.Change(0, 500)

let beginTrivia (triviaRounds:string) = 
    async {
        match state.questionStatus with
            | Disabled -> 
                let! nextQuestion = getTriviaQuestion();
                state <- { state with questionStatus = nextQuestion; rounds = int triviaRounds }
                do! IrcCommands.privmsg <| questionOutput state.questionStatus
            | _ -> ()
    }

let handleTriviaCommand (splitMessage:string array) = 
    async {
        match splitMessage.Length with
        | 2 -> do! beginTrivia splitMessage[1]
        | _ -> do! beginTrivia "0"
    }

