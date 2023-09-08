module Domain.Contracts.Auth0TokenRequest

open Newtonsoft.Json

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