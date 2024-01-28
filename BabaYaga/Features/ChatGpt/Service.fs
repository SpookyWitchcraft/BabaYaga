module ChatGpt.Service

//open Types
//open Application.Types

//type ChatGptHandler(client:IClientProxy, auth:IAuth0Service, irc:IIrcBroadcaster) = 
//    let get (question:string) = 
//        async {
//            let! token = auth.GetToken ()

//            match token with 
//            | Error e -> return Error(e)
//            | Ok a -> 
//                let! results = client.Get $"/api/chatgpt/{question}" a.AccessToken

//                return results
//        } 
    
//    interface IMessageHandler with
//        member _.Handle (splitMessage:string array) = 
//            async {
//                let! answer = get splitMessage[1]

//                match answer with
//                | Error e -> do! irc.Privmsg $"There was an error, {e}"
//                | Ok a -> do! 
//                    a.Lines 
//                    |> List.map irc.Privmsg
//                    |> Async.Sequential
//                    |> Async.Ignore
//            }