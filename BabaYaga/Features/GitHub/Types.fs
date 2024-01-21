module GitHub.Types

open System.Text.Json.Serialization

type GitHubResponse = 
    {[<JsonPropertyName("htmlUrl")>]
        HtmlUrl: string }

type GitHubRequest = 
    {
        Title: string
        Body: string
        Labels: string array }