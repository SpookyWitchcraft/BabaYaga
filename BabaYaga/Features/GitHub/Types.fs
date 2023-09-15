module GitHub.Types

open System.Runtime.Serialization

[<DataContract>]
type GitHubResponse = 
    {[<field:DataMember(Name = "htmlUrl")>]
        HtmlUrl: string }

type GitHubRequest = 
    {
        Title: string
        Body: string
        Labels: string array }