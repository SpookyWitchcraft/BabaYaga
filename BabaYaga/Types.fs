module Application.Types

open System.IO
open System.Collections.Generic
open Auth0.Types

type AuthType = 
    | Object
    | Token of string

type ITcpProxy = 
    abstract member Reader : StreamReader with get
    abstract member WriteAsync : string -> Async<unit>
    abstract member ReadAsync : unit -> Async<string>

type IIrcBroadcaster = 
    abstract member InitializeCommunication : unit -> Async<unit>
    abstract member Ping : string -> Async<unit>
    abstract member Privmsg : string -> Async<unit>
    abstract member IdentifyAndJoin : string -> Async<unit>

type HttpResult<'a> = Async<Result<'a, string>>

type IClientProxy = 
    abstract member Get<'a> : string -> string -> HttpResult<'a>
    abstract member Post<'a, 'b> : 'a -> AuthType -> string -> HttpResult<'b>

type IAuth0Service = 
    abstract member GetToken : unit -> Async<Result<Auth0TokenResponse, string>>

type IMessageHandler = 
    abstract member Handle : string array -> Async<string option>

type BotState = Unidentified | Identified

type ChannelMessage = 
    { UserInfo: string; Channel: string; Message: string }

type Application = 
    { TcpProxy : ITcpProxy; 
    Irc : IIrcBroadcaster;
    Handlers : IDictionary<string, IMessageHandler> }