module GitHub.Service

open Types
open Infrastructure.ClientProxy
open System

let post (issue:GitHubRequest) : Async<GitHubResponse> = 
    async {
        let! token = Auth0.Service.getToken()

        let! results = post issue (Token token) (buildUrl $"/api/github")
        
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

        return $"Thank you {user} an issue has been created.  You may check the status here, {response.HtmlUrl}."
    }

let handleGitHubCommand (input:string) (issue:string) = 
    async {
        let! issueResponse = createIssue input issue
        
        do! IrcCommands.privmsg issueResponse
    }