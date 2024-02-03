module GitHub.Service

open Application.Types
open Types
open Infrastructure.ClientProxy
open System

type GitHubService(client:IClientProxy, auth:IAuth0Service) = 
    member _.Post (issue:GitHubRequest) : Async<Result<GitHubResponse, string>> = 
        async {
            let! token = auth.GetToken ()

            match token with
            | Error e -> return Error(e)
            | Ok a -> 
                let! results = client.Post issue (Token a.AccessToken) (buildUrl $"/api/github")
        
                return results
        } 

    member _.CreateIssue(input:string array) = 
        let user = (input[0].Split('!')[0]).Split(':')[1]
        let guid = Guid.NewGuid().ToString()
        let now = DateTime.UtcNow.ToString("yyyy-MM-dd")
        let title = $"{guid} - {now} - {user}"

        { Title = title; Body = $"{user}: {input[1]}"; Labels = [| "bug" |] }

type GitHubHandler(client:IClientProxy, auth:IAuth0Service, irc:IIrcBroadcaster) = 
    let service = GitHubService(client, auth)
    
    interface IMessageHandler with
        member _.Handle (inputs:string array) = 
            async {
                let user = (inputs[0].Split('!')[0]).Split(':')[1]

                let issue = service.CreateIssue inputs

                let! postIssueResponse = service.Post issue
                
                match postIssueResponse with
                | Ok a -> do! irc.Privmsg $"Thank you {user} an issue has been created.  You may check the status here, {a.HtmlUrl}."
                | Error b -> do! irc.Privmsg $"There was an error. {b}."
            }