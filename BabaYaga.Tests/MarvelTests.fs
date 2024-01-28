module MarvelTests

open Xunit
open System.Text.Json
open Marvel.Service
open IrcCommands
open TcpProxyMocks
open ClientProxyMocks
open Auth0.Service
open Application.Types
open Marvel.Types

let hero = {Id = "1"; Name = "Juggernaut"; Description = "Big"}
let js = JsonSerializer.Serialize(hero)
let httpSuccess = ClientProxySuccessMock(js)
let httpFailure = ClientProxyFailureMock("No Good!")
let tcp = TcpProxyMock()
let irc = IrcBroadcaster(tcp)

[<Fact>]
let ``get should return superhero data`` () =
    let auth = Auth0Service(httpSuccess)
    let handler = MarvelHandler(httpSuccess, auth, irc) :> IMessageHandler

    let option = handler.Handle [|"m1"; "m2"|] |> Async.RunSynchronously
    let result = option.Value = "Big"

    Assert.True(result)
