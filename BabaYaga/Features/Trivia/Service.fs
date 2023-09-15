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

let get () = 
    async {
        let token = Auth0.Service.getToken ()

        client.DefaultRequestHeaders.Authorization <- new AuthenticationHeaderValue("Bearer", token)

        let! response = client.GetStringAsync(buildUrl "/api/trivia") |> Async.AwaitTask
        
        let tq = JsonConvert.DeserializeObject<TriviaQuestion>(response)

        return tq
    } 
    
    |> Async.RunSynchronously

let getTriviaQuestion () = 
    let triviaQuestion = get ()

    let currentTime = Stopwatch.GetTimestamp()

    Some <| NeedsHint (currentTime, triviaQuestion)

let questionOutput (questionStatus:QuestionStatus option) = 
    match questionStatus with
    | Some a -> 
        match a with | TimesUp (_, b) | NeedsHint (_, b) | HasHint(_, b) ->
            $"[{b.Id}]: {b.Question}"
    | _ -> "Oops there was a problem ☀"

let updateScores (state:byref<ApplicationState>) (winner:string) = 
    if state.scores.ContainsKey winner then
        state.scores[winner] <- state.scores[winner] + 1
    else
        state.scores.Add(winner, 1)

let findWinner (state:byref<ApplicationState>) = 
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
        $"The Round is now over!  With a score of {best}, the win goes to: {winners}"

let checkAnswer (state:byref<ApplicationState>) (message:string) (userInfo:string) = 
    match state.question with
    | None -> ()
    | Some q ->
        match q with
        | TimesUp (_, b) | NeedsHint (_, b) | HasHint(_, b) -> 
            let results = b.Answer =? message
 
            let user = (userInfo.Split(':')[0]).Split('!')[0]

            if results then 
                let output = sprintf "PRIVMSG %s %s" getEnvironmentVariables["CHANNEL"] $"{user} wins!  The answer is {b.Answer}."
                state.writer.WriteLine(output)
                
                updateScores &state user

                let roundsLeft = state.rounds - 1
                if roundsLeft > 0 then
                    let q = getTriviaQuestion()
                    let m = questionOutput q
                    let output = sprintf "PRIVMSG %s %s" getEnvironmentVariables["CHANNEL"] m
                    state.writer.WriteLine(output)
                    state <- { state with question = q; rounds = roundsLeft }
                    //writeText <| Output output
                else
                    let winner = findWinner &state
                    let output = sprintf "PRIVMSG %s %s" getEnvironmentVariables["CHANNEL"] winner
                    state.writer.WriteLine(output)
                    state <- { state with question = None; scores = new Dictionary<string, int>() }
            else
                ()

let createHint (answer:string) = 
    let rand = new Random()
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

let checkQuestionStatus (state:byref<ApplicationState>) = 
    match state.question with
    | Some a ->
        match a with
        | TimesUp (_, y) ->
            let roundsLeft = state.rounds - 1
            if roundsLeft > 0 then
                let tuOutput = sprintf "PRIVMSG %s %s" getEnvironmentVariables["CHANNEL"] $"Times up! The answer is {y.Answer}"
                state.writer.WriteLine(tuOutput)
                let q = getTriviaQuestion()
                let m = questionOutput q
                let output = sprintf "PRIVMSG %s %s" getEnvironmentVariables["CHANNEL"] m
                state.writer.WriteLine(output)
                state <- { state with question = q; rounds = roundsLeft }
            else
                let output = sprintf "PRIVMSG %s %s" getEnvironmentVariables["CHANNEL"] $"Times up! The answer is {y.Answer}"
                state.writer.WriteLine(output)
                let winner = findWinner &state
                let winnerOutput = sprintf "PRIVMSG %s %s" getEnvironmentVariables["CHANNEL"] winner
                state.writer.WriteLine(winnerOutput)
                state <- { state with question = None }
            //writeText <| Output output
        | HasHint (x, y) -> 
            let elapsed = (Stopwatch.GetTimestamp() - x) / Stopwatch.Frequency
            if elapsed >= 20 then 
                state <- { state with question = Some <| TimesUp (x, y) }
            else 
                ()
        | NeedsHint (x, y) -> 
            let elapsed = (Stopwatch.GetTimestamp() - x) / Stopwatch.Frequency

            if elapsed >= 10 then 
                let output = sprintf "PRIVMSG %s %s" getEnvironmentVariables["CHANNEL"] (createHint y.Answer)
                state.writer.WriteLine(output)
                //writeText <| Output output
                state <- { state with question = Some <| HasHint (x, y) }
            else
                ()
    | _ -> ()

