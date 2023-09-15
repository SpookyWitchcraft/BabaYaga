module Trivia.Service

open Trivia.Types
open Application.Types
open Infrastructure.ClientProxy
open Modules.Environment

open Newtonsoft.Json
open System.Net.Http.Headers
open System.Diagnostics
open System

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
                //writeText <| Output output

                state <- { state with question = None }
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
        | TimesUp (x, y) ->
            state <- { state with question = None }
            let output = sprintf "PRIVMSG %s %s" getEnvironmentVariables["CHANNEL"] $"Times up! The answer is {y.Answer}"
            state.writer.WriteLine(output)
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

let questionOutput (questionStatus:QuestionStatus option) = 
    match questionStatus with
    | Some a -> 
        match a with | TimesUp (_, b) | NeedsHint (_, b) | HasHint(_, b) ->
            $"[{b.Id}]: {b.Question}"
    | _ -> "Oops there was a problem ☀"