module Auth0.Service

open Modules.Environment
open Types
open Application.Types
open System.Diagnostics

let post (proxy:IClientProxy) (request:Auth0TokenRequest) (authUrl:string) = 
    async {
        let! response = proxy.Post request Object authUrl

        return response
    } 

let mutable authResponse = ({ AccessToken = ""; ExpiresIn = 86400; TokenType = "" }, Stopwatch.GetTimestamp())

let buildRequest(id:string) (secret:string) (audience:string) =
  { GrantType = "client_credentials"; ClientId = id; ClientSecret = secret; Audience = audience }

let getNewAuthToken (proxy:IClientProxy) = 
    async {
        let au = getEnvironmentVariables["AUTH_URL"]
        let cid = getEnvironmentVariables["CLIENT_ID"]
        let cs = getEnvironmentVariables["CLIENT_SECRET"]
        let aud = getEnvironmentVariables["AUDIENCE"]

        let! response = post proxy (buildRequest cid cs aud) au 
        return (response, Stopwatch.GetTimestamp())
    }

let expired (response:Auth0TokenResponse * int64) = 
    let (a, b) = response
    let now = Stopwatch.GetTimestamp()
    let seconds = (now - b) / Stopwatch.Frequency
    a.AccessToken = "" || a.ExpiresIn <= int seconds

let getToken (proxy:IClientProxy) = 
    async {
        match authResponse with
        | ar when expired ar ->
            let! (a, b) = getNewAuthToken proxy

            match a with
            | Error _ -> return a
            | Ok o -> 
                authResponse <- (o, b)
                return Ok (o)
        | (a, _) -> return Ok(a)
    }