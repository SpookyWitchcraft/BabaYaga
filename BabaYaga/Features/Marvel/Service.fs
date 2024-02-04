module Marvel.Service

open Application.Types
open Types

type MarvelService(client:IClientProxy, auth:IAuth0Service) = 
    member _.GetMarvelCharacter (name:string) : Async<Result<MarvelCharacter, string>> = 
        async {
            let! token = auth.GetToken ()

            match token with 
            | Error e -> return Error(e)
            | Ok a -> 
                let! results = client.Get $"/api/marvel/{name}" a.AccessToken

                return results
        }

type MarvelHandler(service:MarvelService, irc:IIrcBroadcaster) = 
    
    interface IMessageHandler with
        member _.Handle (inputs:string array) = 
            async {
                let! results = service.GetMarvelCharacter inputs[1]

                match results with
                | Error e -> do! irc.Privmsg e
                | Ok a -> do! irc.Privmsg a.Description
            }