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
open System.Threading

let initialState = 
    { 
        questionStatus = Disabled
        rounds = 0
        scores = Dictionary<string, int>()
        timestamp = 0
    }

let mutable state = initialState

let get () = 
    async {
        let! token = Auth0.Service.getToken ()

        client.DefaultRequestHeaders.Authorization <- AuthenticationHeaderValue("Bearer", token)

        let! response = client.GetStringAsync(buildUrl "/api/trivia") |> Async.AwaitTask
        
        let tq = JsonConvert.DeserializeObject<TriviaQuestion>(response)

        return tq
    } 
    
let getTriviaQuestion () = 
    async {
        let! triviaQuestion = get ()

        let currentTime = Stopwatch.GetTimestamp()

        return NeedsHint triviaQuestion, currentTime
    }

let questionOutput (questionStatus:QuestionStatus) = 
    match questionStatus with
        | TimesUp q | NeedsHint q | HasHint q ->
            $"[{q.Id}]: {q.Question}"
        | _ -> "Something went wrong."

let elapsedTime (timestamp:int64) = 
    (Stopwatch.GetTimestamp() - timestamp) / Stopwatch.Frequency

let getScore () = 
    match state.questionStatus with
    | NeedsHint _ -> 202 - int (elapsedTime state.timestamp)
    | HasHint _ -> 50 - int (elapsedTime state.timestamp)
    | _ -> 200

let updateScores (winner:string) = 
    if state.scores.ContainsKey winner then
        state.scores[winner] <- state.scores[winner] + getScore()
    else
        state.scores.Add(winner, getScore())

let writeWinners(winners: string[]) = 
    async {
        for w in winners do
            do! IrcCommands.privmsg w
    }

let findWinner () = 
    if state.scores.Count < 1 then
        [|"The round is now over!  No one scored!"|]
    else
        let takeCount = if state.scores.Count > 3 then 3 else state.scores.Count
        let winners = 
            state.scores
            |> Seq.sortByDescending(fun x -> x.Value)
            |> Seq.take(takeCount)
            |> Seq.toArray
            |> Array.indexed
            |> Array.map(fun (i, x) -> $"{[i + 1]}: {x.Key} = {x.Value}")
        winners

let checkRoundsAndWinner = 
    async {
        match state.rounds - 1 with
        | x when x <= 0 ->
            do! IrcCommands.privmsg $"The game has ended, here are the scores!"
            do! writeWinners <| findWinner()
            state <- { state with questionStatus = Disabled; scores = Dictionary<string, int>() }
        | _ -> state <- { state with questionStatus = Answered; rounds = state.rounds - 1 }
    }

let matchResults (question:TriviaQuestion) (results:bool) (user:string) =
    async {
        match results with
        | true ->
            do! IrcCommands.privmsg $"{user} wins! The answer is {question.Answer}."
            updateScores user
            do! checkRoundsAndWinner
        | _ -> ()
    }

let checkAnswer (message:ChannelMessage) = 
    async {
        match state.questionStatus with
        | NeedsHint q | HasHint q -> 
            let results = q.Answer =? message.Message

            let user = (message.UserInfo.Split(':')[0]).Split('!')[0]

            do! matchResults q results user
        | _ -> ()
    }

let createHint (answer:string) = 
    let rand = Random()
    let values = List.init answer.Length (fun a -> 
        let next = rand.Next(0, 3)
        if next = 0 || not (Char.IsLetter answer[a] || Char.IsDigit answer[a]) then
            string answer[a]
        else
            "*"
        )
    let asterisks = String.concat "" values
    let index = rand.Next(0, answer.Length)
    let pre = asterisks |> String.mapi (fun i x -> if i = index then answer[i] else x)
    let final = if pre.Contains('*') then pre else pre |> String.mapi (fun i x -> if i = index then '*' else x)
    "Here's a hint: " + final

let mutable timer = new Timer((fun _ -> ()), null, Timeout.Infinite, Timeout.Infinite)
 
let checkQuestionStatus = 
    async {
        match state.questionStatus with
        | NewQuestion -> 
            let! (question, time) = getTriviaQuestion()
            ignore <| timer.Change(500, 500)
            state <- { state with questionStatus = question; timestamp = time }
            do! IrcCommands.privmsg <| questionOutput question
        | NeedsHint q ->
            match elapsedTime state.timestamp >= 10 with
            | true -> 
                state <- { state with questionStatus = HasHint q }
                do! IrcCommands.privmsg (createHint q.Answer)
            | _ -> ()
        | HasHint q ->
            match elapsedTime state.timestamp >= 20 with
            | true -> state <- { state with questionStatus = TimesUp q }
            | _ -> ()
        | TimesUp y ->
            match state.rounds - 1 with 
            | x when x <= 0 -> 
                do! IrcCommands.privmsg $"Times up! The answer is {y.Answer}"
                do! IrcCommands.privmsg $"The game has ended, here are the scores!"
                do! writeWinners <| findWinner ()
                state <- { state with questionStatus = Disabled; scores = Dictionary<string, int>() }
            | _ ->
                state <- { state with questionStatus = Answered; rounds = state.rounds - 1 }
                do! IrcCommands.privmsg $"Times up! The answer is {y.Answer}"
        | Answered ->
            ignore <| timer.Change(5000, 500)
            state <- { state with questionStatus = NewQuestion }
        | Disabled -> ignore <| timer.Change(Timeout.Infinite, Timeout.Infinite)
    }

let beginTrivia (triviaRounds:string) = 
    async {
        match state.questionStatus with
            | Disabled -> 
                state <- { state with questionStatus = NewQuestion; rounds = int triviaRounds }
                do! checkQuestionStatus
            | _ -> ()
    }

let handleTriviaCommand (splitMessage:string array) = 
    async {
        match splitMessage.Length with
        | 2 -> do! beginTrivia splitMessage[1]
        | _ -> do! beginTrivia "0"
    }

timer <- new Timer(
        TimerCallback (fun _ -> async { do! checkQuestionStatus } |> Async.StartImmediate),
        null,
        Timeout.Infinite,
        Timeout.Infinite
    )