module Infrastructure.ClientProxy

open System.Net.Http
open Modules.Environment
open System.Text
open System.Text.Json
open System.Net.Http.Headers
open Application.Types

let root = getEnvironmentVariables["API_URL"]

let buildUrl (suffix:string) = 
    $"{root}{suffix}"

type ClientProxy() = 
    let client = new HttpClient()

    interface IClientProxy with
        member _.Get<'a> (urlSuffix:string) (token:string) = 
            async {
                client.DefaultRequestHeaders.Authorization <- AuthenticationHeaderValue("Bearer", token)

                let! response = client.GetAsync(buildUrl urlSuffix) |> Async.AwaitTask
        
                let! results = response.Content.ReadAsStreamAsync() |> Async.AwaitTask

                return 
                    try
                        Ok(JsonSerializer.Deserialize<'a>(results))
                    with
                        | Failure msg -> Error (msg)
            } 

        member _.Post<'a, 'b> (obj: 'a) (auth:AuthType) (url:string) = 
            async {
                let serialized = JsonSerializer.Serialize(obj)

                let content = new StringContent(serialized, Encoding.UTF8, "application/json")
        
                match auth with
                | Token a -> client.DefaultRequestHeaders.Authorization <- AuthenticationHeaderValue("Bearer", a)
                | Object -> ignore <| client.DefaultRequestHeaders.Remove("Authorization")

                let! response = client.PostAsync(url, content) |> Async.AwaitTask

                let! results = response.Content.ReadAsStreamAsync() |> Async.AwaitTask

                return 
                    try
                        Ok(JsonSerializer.Deserialize<'b>(results))
                    with
                        | Failure msg -> Error (msg)
            }