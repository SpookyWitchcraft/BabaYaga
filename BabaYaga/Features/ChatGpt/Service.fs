module ChatGpt.Service

open Infrastructure.ClientProxy
open Types

let get (question:string) = 
    async {
        let! token = Auth0.Service.getToken ()

        match token with 
        | Error e -> return Error(e)
        | Ok a -> 
            let! results = proxy.Get $"/api/chatgpt/{question}" a.AccessToken

            return results
    } 

let getGptAnswer (question:string) = 
    let answer = get question 
    answer

let handleGptCommand (question:string) = 
    async {
    let! answer = getGptAnswer question

    match answer with
    | Error e -> do! IrcCommands.privmsg $"There was an error, {e}"
    | Ok a -> do! 
        a.Lines 
        |> List.map IrcCommands.privmsg
        |> Async.Sequential
        |> Async.Ignore
    }