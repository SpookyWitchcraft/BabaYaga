module Application.Types

open System.Collections.Generic

type BotState = Unidentified | Identified

type ApplicationState = {
    question : option<Trivia.Types.QuestionStatus>
    rounds : int
    scores : Dictionary<string, int>
    botState : BotState
}