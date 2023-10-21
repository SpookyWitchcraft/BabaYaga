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
open System.Threading

let initialState = 
    { 
        questionStatus = Disabled
        rounds = 0
        scores = Dictionary<string, int>()
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

        return NeedsHint (currentTime, triviaQuestion)
    }

let questionOutput (questionStatus:QuestionStatus) = 
    match questionStatus with
        | TimesUp q | NeedsHint (_, q) | HasHint(_, q) ->
            $"[{q.Id}]: {q.Question}"
        | _ -> "Something went wrong."

let updateScores (winner:string) = 
    if state.scores.ContainsKey winner then
        state.scores[winner] <- state.scores[winner] + 1
    else
        state.scores.Add(winner, 1)

let findWinner () = 
    if state.scores.Count < 1 then
        "The round is now over!  No one scored!"
    else
        let best = int <| state.scores.Values.Max()
        let winners = 
            state.scores
            |> Seq.filter (fun kvp -> kvp.Value = best)
            |> Seq.map (fun kvp -> kvp.Key)
            |> Seq.toArray
            |> String.concat ", "
        let output = $"The Round is now over!  With a score of {best}, the win goes to: {winners}"
        writeText <| Output output
        output

let checkRoundsAndWinner = 
    async {
        match state.rounds - 1 with
        | x when x <= 0 ->
            let winner = findWinner ()
            do! IrcCommands.privmsg winner
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
        | TimesUp q | NeedsHint (_, q) | HasHint(_, q) -> 
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

let elapsedTime (timestamp:int64) = 
    (Stopwatch.GetTimestamp() - timestamp) / Stopwatch.Frequency
 
let checkQuestionStatus = 
    async {
        match state.questionStatus with
        | NewQuestion -> 
            let! question = getTriviaQuestion()
            ignore <| timer.Change(500, 500)
            state <- { state with questionStatus = question }
            do! IrcCommands.privmsg <| questionOutput question
        | NeedsHint (x, y) ->
            match elapsedTime x >= 10 with
            | true -> 
                state <- { state with questionStatus = HasHint (x, y) }
                do! IrcCommands.privmsg (createHint y.Answer)
            | _ -> ()
        | HasHint (x, y) ->
            match elapsedTime x >= 20 with
            | true -> state <- { state with questionStatus = TimesUp y }
            | _ -> ()
        | TimesUp y ->
            match state.rounds - 1 with 
            | x when x <= 0 -> 
                do! IrcCommands.privmsg $"Times up! The answer is {y.Answer}"
                do! IrcCommands.privmsg <| findWinner ()
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