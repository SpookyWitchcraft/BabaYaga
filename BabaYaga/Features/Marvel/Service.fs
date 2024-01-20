module Marvel.Service

open Marvel.Types
open Application.Types

type MarvelHandler(client:IClientProxy, irc:IIrcBroadcaster) = 

    let get (characterName:string) : Async<Result<MarvelCharacter, string>> = 
        async {
            let! token = Auth0.Service.getToken client

            match token with 
            | Error e -> return Error(e)
            | Ok a -> 
                let! results = client.Get $"/api/marvel/{characterName}" a.AccessToken

                return results
        } 

    let getMarvelCharacter (name:string) = 
        async {
            let! character = get name 

            match character with
            | Error e -> return $"There was an error! {e}"
            | Ok a -> 
                if a.Description = "" then 
                    return "No description found :(" 
                else 
                    return a.Description
        }

    interface IMessageHandler with
        member _.Handle (inputs:string array) = 
            async {
                let! charDescription = getMarvelCharacter inputs[1]
        
                do! irc.Privmsg charDescription
            }