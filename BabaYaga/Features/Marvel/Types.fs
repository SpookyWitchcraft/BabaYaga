module Marvel.Types

open System.Text.Json.Serialization

type MarvelCharacter = 
    { 
        [<JsonPropertyName("id")>]
        Id: string
        [<JsonPropertyName("name")>]
        Name: string
        [<JsonPropertyName("description")>]
        Description: string }