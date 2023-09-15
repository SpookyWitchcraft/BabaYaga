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
    let character = get name |> Async.RunSynchronously
    if character.Description = "" then "No description found :(" else character.Description