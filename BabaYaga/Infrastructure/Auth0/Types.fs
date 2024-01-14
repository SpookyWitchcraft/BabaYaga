module Auth0.Types

open System.Text.Json.Serialization


type Auth0TokenResponse = 
    {
        [<JsonPropertyName("access_token")>]
        AccessToken: string 
        [<JsonPropertyName("expires_in")>]
        ExpiresIn: int
        [<JsonPropertyName("token_type")>]
        TokenType: string }

type Auth0TokenRequest = 
    {
        [<JsonPropertyName("grant_type")>]
        GrantType: string
        [<JsonPropertyName("client_id")>]
        ClientId: string
        [<JsonPropertyName("client_secret")>]
        ClientSecret: string
        [<JsonPropertyName("audience")>]
        Audience: string }