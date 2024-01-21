module Trivia.Types

open System.Text.Json.Serialization
open System.Collections.Generic

type TriviaQuestion = 
    { 
        [<JsonPropertyName("id")>]
        Id: int
        [<JsonPropertyName("question")>]
        Question: string
        [<JsonPropertyName("answer")>]
        Answer: string
        [<JsonPropertyName("category")>]
        Category: string }

type QuestionStatus = 
    | TimesUp of TriviaQuestion
    | NeedsHint of TriviaQuestion
    | HasHint of TriviaQuestion
    | Disabled
    | Answered
    | NewQuestion

type ApplicationState = {
    questionStatus : QuestionStatus
    rounds : int
    scores : Dictionary<string, int>
    timestamp : int64
}