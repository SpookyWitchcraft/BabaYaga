module Marvel.Service

open Marvel.Types

open Newtonsoft.Json
open System.Net.Http.Headers
open Infrastructure.ClientProxy

let get (characterName:string) = 
    async {
        let token = Auth0.Service.getToken ()

        client.DefaultRequestHeaders.Authorization <- new AuthenticationHeaderValue("Bearer", token)

        let! response = client.GetStringAsync(buildUrl $"/api/marvel/{characterName}") |> Async.AwaitTask
        
        if response = "" then
            return { Id = ""; Name = ""; Description = "" }
        else
            let tq = JsonConvert.DeserializeObject<MarvelCharacter>(response)

            return tq
    } 

let getMarvelCharacter (name:string) = 
    async {
        let! character = get name 
        if character.Description = "" then 
            return "No description found :(" 
        else 
            return character.Description
    }