module Infrastructure.ClientProxy

open System.Net.Http
open Newtonsoft.Json
open Modules.Environment
open System.Text
open System.Net.Http.Headers

let client = new HttpClient()

let root = getEnvironmentVariables["API_URL"]

type AuthType = 
    | Object
    | Token of string

let buildUrl (suffix:string) = 
    $"{root}{suffix}"

let post<'a, 'b> (obj: 'a) (auth:AuthType) (url:string) = 
    async {
        let serialized = JsonConvert.SerializeObject(obj)

        let content = new StringContent(serialized, Encoding.UTF8, "application/json")
        
        match auth with
        | Token a -> client.DefaultRequestHeaders.Authorization <- new AuthenticationHeaderValue("Bearer", a)
        | Object -> ignore <| client.DefaultRequestHeaders.Remove("Authorization")

        let! response = client.PostAsync(url, content) |> Async.AwaitTask

        let! results = response.Content.ReadAsStringAsync() |> Async.AwaitTask

        return JsonConvert.DeserializeObject<'b>(results)
    }

let get<'a> (urlSuffix:string) (token:string) = 
    async {
        client.DefaultRequestHeaders.Authorization <- new AuthenticationHeaderValue("Bearer", token)

        let! response = client.GetAsync(buildUrl urlSuffix) |> Async.AwaitTask
        
        let! results = response.Content.ReadAsStringAsync() |> Async.AwaitTask

        return JsonConvert.DeserializeObject<'a>(results)
    }