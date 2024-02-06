module TriviaTests

open Xunit
open System.Text.Json
open Trivia.Service
open IrcCommands
open TcpProxyMocks
open ClientProxyMocks
open Auth0.Service
open Trivia.Types

let tq = { Id = 1; Question = "Yes?"; Answer = "No."; Category = "General" }
let js = JsonSerializer.Serialize(tq)
let httpSuccess = ClientProxySuccessMock(js)
let httpFailure = ClientProxyFailureMock("No Good!")
let tcp = TcpProxyMock()
let irc = IrcBroadcaster(tcp)

[<Fact>]
let ``get should return superhero data`` () =
    let auth = Auth0Service(httpSuccess)
    let service = TriviaService(httpSuccess, auth)
    let defaultTq = { Id = 0; Question = ""; Answer = ""; Category = "" }

    let result = service.Get () |> Async.RunSynchronously

    Assert.True(Result.isOk result)
    Assert.True((Result.defaultValue defaultTq result).Id = 1)
    Assert.True((Result.defaultValue defaultTq result).Question = "Yes?")
    Assert.True((Result.defaultValue defaultTq result).Answer = "No.")
    Assert.True((Result.defaultValue defaultTq result).Category = "General")

[<Fact>]
let ``get should fail with an error message`` () =
    let auth = Auth0Service(httpSuccess)
    let service = TriviaService(httpFailure, auth)

    let result = service.Get () |> Async.RunSynchronously

    let e = match result with | Ok _ -> "incorrect" | Error e -> e

    Assert.True(Result.isError result)
    Assert.True((e = "No Good!"))