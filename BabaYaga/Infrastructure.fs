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

let client = new HttpClient()

let getTriviaQuestion () = 
    task {
        let! response = client.GetStringAsync("https://localhost:7242/api/trivia")
        
        let tq = JsonConvert.DeserializeObject<TriviaQuestion>(response)

        return tq
    } 
    |> Async.AwaitTask
    |> Async.RunSynchronously