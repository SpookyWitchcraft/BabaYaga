module Types

open System.Text.Json.Serialization

type GptResponse = 
    { 
        [<JsonPropertyName("lines")>]
        Lines: string list }