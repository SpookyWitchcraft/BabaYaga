module Domain.Contracts.MarvelCharacter

open System.Runtime.Serialization

[<DataContract>]
type MarvelCharacter = 
    { 
        [<field:DataMember(Name = "id")>]
        Id: string
        [<field:DataMember(Name = "name")>]
        Name: string
        [<field:DataMember(Name = "description")>]
        Description: string }