module Domain.Contracts.GitHubResponse

open System.Runtime.Serialization

[<DataContract>]
type GitHubResponse = 
    {[<field:DataMember(Name = "htmlUrl")>]
        HtmlUrl: string }