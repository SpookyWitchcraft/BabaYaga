module Infrastructure.Requests.Auth0.Auth0Token

open Newtonsoft.Json
open System.Net.Http
open Infrastructure.Modules.ClientProxy
open System.Text
open Domain.Contracts.Auth0TokenRequest
open Domain.Contracts.Auth0TokenResponse
open System

let post (issue:Auth0TokenRequest) (authUrl:string) = 
    task {
        let serialized = JsonConvert.SerializeObject(issue)

        let content = new StringContent(serialized, Encoding.UTF8, "application/json")

        let! response = client.PostAsync(authUrl, content)
        
        let! results = response.Content.ReadAsStringAsync()

        if results = "" then
            return { AccessToken = ""; ExpiresIn = 86400; TokenType = "" }
        else
            let tq = JsonConvert.DeserializeObject<Auth0TokenResponse>(results)

            return tq
    } 
    |> Async.AwaitTask
    |> Async.RunSynchronously

let mutable authResponse = ({ AccessToken = ""; ExpiresIn = 86400; TokenType = "" }, DateTime.UtcNow)

let buildRequest(id:string) (secret:string) (audience:string) =
  { GrantType = "client_credentials"; ClientId = id; ClientSecret = secret; Audience = audience }

let getNewAuthToken (authUrl:string) (id:string) (secret:string) (audience:string) = 
    let response = post (buildRequest id secret audience) authUrl
    (response, DateTime.UtcNow)

let expired (response:Auth0TokenResponse * DateTime) = 
    let (a, b) = response
    let now = DateTime.UtcNow
    let seconds = (now - b).TotalSeconds
    a.AccessToken = "" || a.ExpiresIn <= int seconds

let getToken (authUrl:string) (id:string) (secret:string) (audience:string) = 
    match authResponse with
    | ar when expired ar ->
        let (a, b) = getNewAuthToken authUrl id secret audience
        authResponse <- (a, b)
        a.AccessToken
    | (a, _) -> a.AccessToken