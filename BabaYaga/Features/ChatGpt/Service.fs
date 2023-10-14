module ChatGpt.Service

open Infrastructure.ClientProxy
open Types

let get (question:string) : Async<GptResponse> = 
    async {
        let! token = Auth0.Service.getToken ()

        let! results = get $"/api/chatgpt/{question}" token

        return results
    } 

let getGptAnswer (question:string) = 
    let answer = get question 
    answer