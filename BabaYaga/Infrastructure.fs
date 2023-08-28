module Infrastructure

open System.Net.Http
open Newtonsoft.Json
open System.Runtime.Serialization

[<DataContract>]
type TriviaQuestion = 
    { 
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

let client = new HttpClient()

let getTriviaQuestion () = 
    task {
        let! response = client.GetStringAsync("https://localhost:7242/api/trivia")
        
        let tq = JsonConvert.DeserializeObject<TriviaQuestion>(response)

        return tq
    } 
    |> Async.AwaitTask
    |> Async.RunSynchronously

let getMarvelCharacter (characterName:string) = 
    task {
        let! response = client.GetStringAsync($"https://localhost:7242/api/marvel/{characterName}")
        
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
        let! response = client.GetStringAsync($"https://localhost:7242/api/chatgpt/{question}")
        
        

        if response = "" then
            return ["No Good"]
        else
            let tq = JsonConvert.DeserializeObject<string list>(response)

            return tq
    } 
    |> Async.AwaitTask
    |> Async.RunSynchronously