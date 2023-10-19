module Types

open System.Runtime.Serialization

[<DataContract>]
type GptResponse = 
    { 
        [<field:DataMember(Name = "lines")>]
        Lines: string list }