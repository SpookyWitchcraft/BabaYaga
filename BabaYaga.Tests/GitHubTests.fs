module GitHubTests

open Xunit
open System.Text.Json
open GitHub.Service
open GitHub.Types
open IrcCommands
open TcpProxyMocks
open ClientProxyMocks
open Auth0.Service
open EnvironmentMock

let response = { HtmlUrl = "https://github.com/SpookyWitchcraft/BabaYaga/issues" }
let js = JsonSerializer.Serialize(response)
let env = EnvironmentMock()
let httpSuccess = ClientProxySuccessMock(js)
let httpFailure = ClientProxyFailureMock("No Good!")
let tcp = TcpProxyMock()
let irc = IrcBroadcaster(env, tcp)

[<Fact>]
let ``post should return ticket url`` () =
    let auth = Auth0Service(env, httpSuccess)
    let service = GitHubService(httpSuccess, auth)
    let request = { Title = "Title"; Body = "Body"; Labels = [| "Bug" |] }

    let result = service.Post request |> Async.RunSynchronously

    Assert.True(Result.isOk result)
    Assert.True((Result.defaultValue { HtmlUrl = "nope" } result).HtmlUrl = "https://github.com/SpookyWitchcraft/BabaYaga/issues")

[<Fact>]
let ``post should fail with an error message`` () =
    let auth = Auth0Service(env, httpSuccess)
    let service = GitHubService(httpFailure, auth)
    let request = { Title = "Title"; Body = "Body"; Labels = [| "Bug" |] }

    let result = service.Post request |> Async.RunSynchronously

    let e = match result with | Ok _ -> "incorrect" | Error e -> e

    Assert.True(Result.isError result)
    Assert.True((e = "No Good!"))