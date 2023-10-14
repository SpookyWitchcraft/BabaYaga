module Marvel.Service

open Marvel.Types

open Infrastructure.ClientProxy

let get (characterName:string) : Async<MarvelCharacter> = 
    async {
        let! token = Auth0.Service.getToken ()

        let! results = get $"/api/marvel/{characterName}" token

        return results
    } 

let getMarvelCharacter (name:string) = 
    async {
        let! character = get name 
        if character.Description = "" then 
            return "No description found :(" 
        else 
            return character.Description
    }