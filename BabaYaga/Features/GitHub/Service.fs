module GitHub.Service

open Application.Types
open Types
open Infrastructure.ClientProxy
open System

type GitHubHandler(client:IClientProxy, irc:IIrcBroadcaster) = 
    let post (issue:GitHubRequest) = 
        async {
            let! token = Auth0.Service.getToken client

            match token with
            | Error e -> return Error(e)
            | Ok a -> 
                let! results = client.Post issue (Token a.AccessToken) (buildUrl $"/api/github")
        
                return results
        } 
    
    let createIssue (input:string) (issue:string) =
        async {
            let user = (input.Split('!')[0]).Split(':')[1]
            let guid = Guid.NewGuid().ToString()
            let now = DateTime.UtcNow.ToString("yyyy-MM-dd")
            let title = $"{guid} - {now} - {user}"

            let request = { Title = title; Body = $"{user}: {issue}"; Labels = [| "bug" |] }
    
            let! response = post request 

            match response with
            | Ok a -> return $"Thank you {user} an issue has been created.  You may check the status here, {a.HtmlUrl}."
            | Error b -> return $"There was an error. {b}."
        }
    
    interface IMessageHandler with
        member _.Handle (inputs:string array) = 
            async {
                let! issueResponse = createIssue inputs[0] inputs[1]
        
                do! irc.Privmsg issueResponse
            }