module Domain.Contracts.Auth0TokenResponse

open Newtonsoft.Json

type Auth0TokenResponse = 
    {
        [<JsonProperty(PropertyName = "access_token")>]
        AccessToken: string 
        [<JsonProperty(PropertyName = "expires_in")>]
        ExpiresIn: int
        [<JsonProperty(PropertyName = "token_type")>]
        TokenType: string }
