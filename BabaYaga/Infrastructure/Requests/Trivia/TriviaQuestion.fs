module Infrastructure.Requests.Trivia.TriviaQuestion

open Infrastructure.Modules.ClientProxy
open Newtonsoft.Json
open Domain.Contracts.TriviaQuestion
open Infrastructure.Requests.Auth0
open System.Net.Http.Headers

let get (authUrl:string) (id:string) (secret:string) (audience:string) = 
    task {
        let token = Auth0Token.getToken authUrl id secret audience

        client.DefaultRequestHeaders.Authorization <- new AuthenticationHeaderValue("Bearer", token)

        let! response = client.GetStringAsync(buildUrl "/api/trivia")
        
        let tq = JsonConvert.DeserializeObject<TriviaQuestion>(response)

        return tq
    } 
    |> Async.AwaitTask
    |> Async.RunSynchronously