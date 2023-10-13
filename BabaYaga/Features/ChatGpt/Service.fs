module ChatGpt.Service

open Newtonsoft.Json
open Infrastructure.ClientProxy
open System.Net.Http.Headers

let get (question:string) = 
    async {
        let! token = Auth0.Service.getToken ()

        client.DefaultRequestHeaders.Authorization <- new AuthenticationHeaderValue("Bearer", token)

        let! response =  client.GetStringAsync(buildUrl $"/api/chatgpt/{question}") |> Async.AwaitTask
        
        if response = "" then
            return ["No Good"]
        else
            let tq = JsonConvert.DeserializeObject<string list>(response)

            return tq
    } 

let getGptAnswer (question:string) = 
    let answer = get question 
    answer