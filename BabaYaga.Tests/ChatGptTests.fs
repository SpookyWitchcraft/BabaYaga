module ChatGptTests

open Xunit
open System.Text.Json
open ChatGpt.Service
open Types
open IrcCommands
open TcpProxyMocks
open ClientProxyMocks
open Auth0.Service

let lines = ["this"; "is"; "an"; "answer"]
let response = { Lines = lines }
let js = JsonSerializer.Serialize(response)
let httpSuccess = ClientProxySuccessMock(js)
let httpFailure = ClientProxyFailureMock("No Good!")
let tcp = TcpProxyMock()
let irc = IrcBroadcaster(tcp)

[<Fact>]
let ``get should return answer data`` () =
    let auth = Auth0Service(httpSuccess)
    let service = ChatGptService(httpSuccess, auth)

    let result = service.Get "My Question" |> Async.RunSynchronously

    Assert.True(Result.isOk result)
    Assert.True((Result.defaultValue { Lines = [] } result).Lines.Length = 4)

[<Fact>]
let ``get should fail with an error message`` () =
    let auth = Auth0Service(httpSuccess)
    let service = ChatGptService(httpFailure, auth)

    let result = service.Get "My Question" |> Async.RunSynchronously

    let e = match result with | Ok _ -> "incorrect" | Error e -> e

    Assert.True(Result.isError result)
    Assert.True((e = "No Good!"))