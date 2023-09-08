module Infrastructure.Requests.Marvel.MarvelCharacterDescription

open Infrastructure.Modules.ClientProxy
open Newtonsoft.Json
open Domain.Contracts.MarvelCharacter
open Infrastructure.Requests.Auth0
open System.Net.Http.Headers

let get (characterName:string) (authUrl:string) (id:string) (secret:string) (audience:string) = 
    task {
        let token = Auth0Token.getToken authUrl id secret audience

        client.DefaultRequestHeaders.Authorization <- new AuthenticationHeaderValue("Bearer", token)

        let! response = client.GetStringAsync(buildUrl $"/api/marvel/{characterName}")
        
        if response = "" then
            return { Id = ""; Name = ""; Description = "" }
        else
            let tq = JsonConvert.DeserializeObject<MarvelCharacter>(response)

            return tq
    } 
    |> Async.AwaitTask
    |> Async.RunSynchronously