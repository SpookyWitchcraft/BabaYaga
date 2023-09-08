module Domain.Contracts.TriviaQuestion

open System.Runtime.Serialization

[<DataContract>]
type TriviaQuestion = 
    { 
        [<field:DataMember(Name = "id")>]
        Id: string
        [<field:DataMember(Name = "question")>]
        Question: string
        [<field:DataMember(Name = "answer")>]
        Answer: string
        [<field:DataMember(Name = "category")>]
        Category: string }