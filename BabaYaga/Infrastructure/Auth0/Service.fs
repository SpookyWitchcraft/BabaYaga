module Auth0.Service

open Types
open Application.Types
open System.Diagnostics



type Auth0Service(environment:IEnvironment, client:IClientProxy) = 
    let post (request:Auth0TokenRequest) (authUrl:string) = 
        async {
            let! response = client.Post request Object authUrl

            return response
        } 

    let mutable authResponse = ({ AccessToken = ""; ExpiresIn = 86400; TokenType = "" }, Stopwatch.GetTimestamp())

    let buildRequest(id:string) (secret:string) (audience:string) =
      { GrantType = "client_credentials"; ClientId = id; ClientSecret = secret; Audience = audience }

    let getNewAuthToken () = 
        async {
            let au = environment.GetSecrets["by-auth-url"]
            let cid = environment.GetSecrets["by-client-id"]
            let cs = environment.GetSecrets["by-client-secret"]
            let aud = environment.GetSecrets["by-audience"]

            let! response = post (buildRequest cid cs aud) au 

            return (response, Stopwatch.GetTimestamp())
        }

    let expired (response:Auth0TokenResponse * int64) = 
        let (a, b) = response
        let now = Stopwatch.GetTimestamp()
        let seconds = (now - b) / Stopwatch.Frequency
        a.AccessToken = "" || a.ExpiresIn <= int seconds
        

    interface IAuth0Service with
        member _.GetToken () = 
            async {
                match authResponse with
                | ar when expired ar ->
                    let! (a, b) = getNewAuthToken ()

                    match a with
                    | Error _ -> return a
                    | Ok o -> 
                        authResponse <- (o, b)
                        return Ok (o)
                | (a, _) -> return Ok(a)
            }