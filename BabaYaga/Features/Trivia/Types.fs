module Trivia.Types

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

type QuestionStatus = 
    | TimesUp of int64 * TriviaQuestion
    | NeedsHint of int64 * TriviaQuestion
    | HasHint of int64 * TriviaQuestion