module Infrastructure.Requests.GitHub.GitHubIssue

open Newtonsoft.Json
open System.Net.Http
open Infrastructure.Modules.ClientProxy
open System.Text
open Domain.Contracts.GitHubRequest
open Domain.Contracts.GitHubResponse

let post (issue:GitHubRequest) = 
    task {
        let serialized = JsonConvert.SerializeObject(issue)

        let content = new StringContent(serialized, Encoding.UTF8, "application/json")

        let! response = client.PostAsync(buildUrl $"/api/github", content)
        
        let! results = response.Content.ReadAsStringAsync()

        if results = "" then
            return { HtmlUrl = "I wasn't able to create that Issue ❌" }
        else
            let tq = JsonConvert.DeserializeObject<GitHubResponse>(results)

            return tq
    } 
    |> Async.AwaitTask
    |> Async.RunSynchronously