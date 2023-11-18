module Infrastructure.ClientProxy

open System.Net.Http
open Newtonsoft.Json
open Modules.Environment
open System.Text
open System.Net.Http.Headers
open System.Net

let client = new HttpClient()

let root = getEnvironmentVariables["API_URL"]

type HttpPost = string -> HttpContent -> System.Threading.Tasks.Task<HttpResponseMessage>

type AuthType = 
    | Object
    | Token of string

let buildUrl (suffix:string) = 
    $"{root}{suffix}"


let tester (url:string) (content:HttpContent) = 
    task {
        let x = new HttpResponseMessage(HttpStatusCode.Accepted);
        x.Content <- content

        return x
    }

let post<'a, 'b> (poster : HttpPost) (obj: 'a) (auth:AuthType) (url:string) = 
    async {
        let serialized = JsonConvert.SerializeObject(obj)

        let content = new StringContent(serialized, Encoding.UTF8, "application/json")
        
        match auth with
        | Token a -> client.DefaultRequestHeaders.Authorization <- AuthenticationHeaderValue("Bearer", a)
        | Object -> ignore <| client.DefaultRequestHeaders.Remove("Authorization")

        let! response = poster url content |> Async.AwaitTask

        let! results = response.Content.ReadAsStringAsync() |> Async.AwaitTask

        return JsonConvert.DeserializeObject<'b>(results)
    }

let get<'a> (urlSuffix:string) (token:string) = 
    async {
        client.DefaultRequestHeaders.Authorization <- AuthenticationHeaderValue("Bearer", token)

        let! response = client.GetAsync(buildUrl urlSuffix) |> Async.AwaitTask
        
        let! results = response.Content.ReadAsStringAsync() |> Async.AwaitTask

        return JsonConvert.DeserializeObject<'a>(results)
    }