module GitHub.Service

open Types
open Newtonsoft.Json
open System.Net.Http
open Infrastructure.ClientProxy
open System.Text
open System

let post (issue:GitHubRequest) = 
    async {
        let serialized = JsonConvert.SerializeObject(issue)

        let content = new StringContent(serialized, Encoding.UTF8, "application/json")

        let! response = client.PostAsync(buildUrl $"/api/github", content) |> Async.AwaitTask
        
        let! results = response.Content.ReadAsStringAsync() |> Async.AwaitTask

        if results = "" then
            return { HtmlUrl = "I wasn't able to create that Issue ❌" }
        else
            let tq = JsonConvert.DeserializeObject<GitHubResponse>(results)

            return tq
    } 
    
    

let createIssue (input:string) (issue:string) =
    let user = (input.Split('!')[0]).Split(':')[1]
    let guid = Guid.NewGuid().ToString()
    let now = DateTime.UtcNow.ToString("yyyy-MM-dd")
    let title = $"{guid} - {now} - {user}"

    let request = { Title = title; Body = $"{user}: {issue}"; Labels = [| "bug" |] }
    
    let response = post request |> Async.RunSynchronously

    $"Thank you {user} an issue has been created.  You may check the status here, {response.HtmlUrl}."