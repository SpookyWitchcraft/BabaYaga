module GitHub.Service

open Types
open Infrastructure.ClientProxy
open System

//type HttpPost = string -> HttpContent -> System.Threading.Tasks.Task<HttpResponseMessage>

let post (issue:GitHubRequest) = 
    async {
        let! token = Auth0.Service.getToken()

        match token with
        | Error e -> return Error(e)
        | Ok a -> 
            let! results = post issue (Token a.AccessToken) (buildUrl $"/api/github")
        
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

let handleGitHubCommand (input:string) (issue:string) = 
    async {
        let! issueResponse = createIssue input issue
        
        do! IrcCommands.privmsg issueResponse
    }