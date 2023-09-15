module Auth0.Service

open Modules.Environment
open Types
open Newtonsoft.Json
open System.Net.Http
open System.Text
open System.Diagnostics
open Infrastructure.ClientProxy

let post (issue:Auth0TokenRequest) (authUrl:string) = 
    async {
        let serialized = JsonConvert.SerializeObject(issue)

        let content = new StringContent(serialized, Encoding.UTF8, "application/json")

        let! response = client.PostAsync(authUrl, content) |> Async.AwaitTask
        
        let! results = response.Content.ReadAsStringAsync() |> Async.AwaitTask

        if results = "" then
            return { AccessToken = ""; ExpiresIn = 86400; TokenType = "" }
        else
            let tq = JsonConvert.DeserializeObject<Auth0TokenResponse>(results)

            return tq
    } 

let mutable authResponse = ({ AccessToken = ""; ExpiresIn = 86400; TokenType = "" }, Stopwatch.GetTimestamp())

let buildRequest(id:string) (secret:string) (audience:string) =
  { GrantType = "client_credentials"; ClientId = id; ClientSecret = secret; Audience = audience }

let getNewAuthToken = 
    let au = getEnvironmentVariables["AUTH_URL"]
    let cid = getEnvironmentVariables["CLIENT_ID"]
    let cs = getEnvironmentVariables["CLIENT_SECRET"]
    let aud = getEnvironmentVariables["AUDIENCE"]

    let response = post (buildRequest cid cs aud) au |> Async.RunSynchronously
    (response, Stopwatch.GetTimestamp())

let expired (response:Auth0TokenResponse * int64) = 
    let (a, b) = response
    let now = Stopwatch.GetTimestamp()
    let seconds = now - b
    a.AccessToken = "" || a.ExpiresIn <= int seconds

let getToken () = 
    match authResponse with
    | ar when expired ar ->
        let (a, b) = getNewAuthToken
        authResponse <- (a, b)
        a.AccessToken
    | (a, _) -> a.AccessToken