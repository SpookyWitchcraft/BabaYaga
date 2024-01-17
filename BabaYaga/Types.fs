module Application.Types

type AuthType = 
    | Object
    | Token of string

type IClientProxy = 
    abstract member Get<'a> : string -> string -> Async<Result<'a, string>>
    abstract member Post<'a, 'b> : 'a -> AuthType -> string -> Async<Result<'b, string>>

type BotState = Unidentified | Identified

type ChannelMessage = 
    { UserInfo: string; Channel: string; Message: string }