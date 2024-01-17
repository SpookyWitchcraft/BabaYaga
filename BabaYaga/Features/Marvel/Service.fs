module Marvel.Service

open Marvel.Types
open Infrastructure.ClientProxy

let get (characterName:string) : Async<Result<MarvelCharacter, string>> = 
    async {
        let! token = Auth0.Service.getToken ()

        match token with 
        | Error e -> return Error(e)
        | Ok a -> 
            let! results = proxy.Get $"/api/marvel/{characterName}" a.AccessToken

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

let handleMarvelCommand (characterName:string) = 
    async {
        let! charDescription = getMarvelCharacter characterName
        
        do! IrcCommands.privmsg charDescription
    }