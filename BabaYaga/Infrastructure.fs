module Infrastructure

open System.Net.Http
open Newtonsoft.Json
open System.Runtime.Serialization
open System.Text

let root = ""

let buildUrl (suffix:string) = 
    $"{root}{suffix}"

[<DataContract>]
type TriviaQuestion = 
    { 
        [<field:DataMember(Name = "id")>]
        Id: string
        [<field:DataMember(Name = "question")>]
        Question: string
        [<field:DataMember(Name = "answer")>]
        Answer: string
        [<field:DataMember(Name = "category")>]
        Category: string }

[<DataContract>]
type MarvelCharacter = 
    { 
        [<field:DataMember(Name = "id")>]
        Id: string
        [<field:DataMember(Name = "name")>]
        Name: string
        [<field:DataMember(Name = "description")>]
        Description: string }


type GitHubRequest = 
    {
        Title: string
        Body: string
        Labels: string array }

[<DataContract>]
type GitHubResponse = 
    {[<field:DataMember(Name = "htmlUrl")>]
        HtmlUrl: string }

let client = new HttpClient()

let getTriviaQuestion () = 
    task {
        let! response = client.GetStringAsync(buildUrl "/api/trivia")
        
        let tq = JsonConvert.DeserializeObject<TriviaQuestion>(response)

        return tq
    } 
    |> Async.AwaitTask
    |> Async.RunSynchronously

let getMarvelCharacter (characterName:string) = 
    task {
        let! response = client.GetStringAsync(buildUrl $"/api/marvel/{characterName}")
        
        if response = "" then
            return { Id = ""; Name = ""; Description = "" }
        else
            let tq = JsonConvert.DeserializeObject<MarvelCharacter>(response)

            return tq
    } 
    |> Async.AwaitTask
    |> Async.RunSynchronously

let getGptAnswer (question:string) = 
    task {
        let! response = client.GetStringAsync(buildUrl $"/api/chatgpt/{question}")
        
        if response = "" then
            return ["No Good"]
        else
            let tq = JsonConvert.DeserializeObject<string list>(response)

            return tq
    } 
    |> Async.AwaitTask
    |> Async.RunSynchronously

let postGitHubIssue (issue:GitHubRequest) = 
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