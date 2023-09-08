module Infrastructure.Requests.ChatGpt.ChatGptAnswer

open Infrastructure.Modules.ClientProxy
open Newtonsoft.Json
open Infrastructure.Requests.Auth0
open System.Net.Http.Headers

let get (question:string) (authUrl:string) (id:string) (secret:string) (audience:string) = 
    task {
        let token = Auth0Token.getToken authUrl id secret audience

        client.DefaultRequestHeaders.Authorization <- new AuthenticationHeaderValue("Bearer", token)

        let! response =  client.GetStringAsync(buildUrl $"/api/chatgpt/{question}")
        
        if response = "" then
            return ["No Good"]
        else
            let tq = JsonConvert.DeserializeObject<string list>(response)

            return tq
    } 
    |> Async.AwaitTask
    |> Async.RunSynchronously