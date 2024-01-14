module Auth0.Service

open Modules.Environment
open Types
open System.Diagnostics
open Infrastructure.ClientProxy

let post (request:Auth0TokenRequest) (authUrl:string) = 
    async {
        let! response = post request Object authUrl

        return response
    } 

let mutable authResponse = ({ AccessToken = ""; ExpiresIn = 86400; TokenType = "" }, Stopwatch.GetTimestamp())

let buildRequest(id:string) (secret:string) (audience:string) =
  { GrantType = "client_credentials"; ClientId = id; ClientSecret = secret; Audience = audience }

let getNewAuthToken () = 
    async {
        let au = getEnvironmentVariables["AUTH_URL"]
        let cid = getEnvironmentVariables["CLIENT_ID"]
        let cs = getEnvironmentVariables["CLIENT_SECRET"]
        let aud = getEnvironmentVariables["AUDIENCE"]

        let! response = post (buildRequest cid cs aud) au 
        return (response, Stopwatch.GetTimestamp())
    }

let expired (response:Auth0TokenResponse * int64) = 
    let (a, b) = response
    let now = Stopwatch.GetTimestamp()
    let seconds = (now - b) / Stopwatch.Frequency
    a.AccessToken = "" || a.ExpiresIn <= int seconds

let getToken () = 
    async {
        match authResponse with
        | ar when expired ar ->
            let! (a, b) = getNewAuthToken()

            match a with
            | Error _ -> return a
            | Ok o -> 
                authResponse <- (o, b)
                return Ok (o)
        | (a, _) -> return Ok(a)
    }