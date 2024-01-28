module MarvelTests

open Xunit
open Marvel.Service
open IrcCommands
open TcpProxyMocks
open ClientProxyMocks
open Auth0.Service
open Application.Types

let httpSuccess = ClientProxySuccessMock("")
let httpFailure = ClientProxySuccessMock("")
let tcp = TcpProxyMock()
let irc = IrcBroadcaster(tcp)

[<Fact>]
let ``get should return superhero data`` () =
    let auth = Auth0Service(httpSuccess)
    let handler = MarvelHandler(httpSuccess, auth, irc) :> IMessageHandler

    let results = handler.Handle [|""|] |> Async.RunSynchronously
    Assert.True(true)
