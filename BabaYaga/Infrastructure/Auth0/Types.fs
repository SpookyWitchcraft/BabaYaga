module Auth0.Types

open Newtonsoft.Json

type Auth0TokenResponse = 
    {
        [<JsonProperty(PropertyName = "access_token")>]
        AccessToken: string 
        [<JsonProperty(PropertyName = "expires_in")>]
        ExpiresIn: int
        [<JsonProperty(PropertyName = "token_type")>]
        TokenType: string }

type Auth0TokenRequest = 
    {
        [<JsonProperty(PropertyName = "grant_type")>]
        GrantType: string
        [<JsonProperty(PropertyName = "client_id")>]
        ClientId: string
        [<JsonProperty(PropertyName = "client_secret")>]
        ClientSecret: string
        [<JsonProperty(PropertyName = "audience")>]
        Audience: string }