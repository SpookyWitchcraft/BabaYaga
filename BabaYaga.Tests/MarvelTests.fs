module MarvelTests

open Xunit
open System.Text.Json
open Marvel.Service
open IrcCommands
open TcpProxyMocks
open ClientProxyMocks
open Auth0.Service
open Marvel.Types
open EnvironmentMock

let hero = {Id = "1"; Name = "Juggernaut"; Description = "Big"}
let js = JsonSerializer.Serialize(hero)
let env = EnvironmentMock()
let httpSuccess = ClientProxySuccessMock(js)
let httpFailure = ClientProxyFailureMock("No Good!")
let tcp = TcpProxyMock()
let irc = IrcBroadcaster(env, tcp)

[<Fact>]
let ``get should return superhero data`` () =
    let auth = Auth0Service(env, httpSuccess)
    let service = MarvelService(httpSuccess, auth)
    let defaultHero = {Id = "0"; Name = "NoOne"; Description = "Burger"}

    let result = service.GetMarvelCharacter "Juggernaut" |> Async.RunSynchronously

    Assert.True(Result.isOk result)
    Assert.True((Result.defaultValue defaultHero result).Description = "Big")

[<Fact>]
let ``get should fail with an error message`` () =
    let auth = Auth0Service(env, httpSuccess)
    let service = MarvelService(httpFailure, auth)

    let result = service.GetMarvelCharacter "Juggernaut" |> Async.RunSynchronously

    let e = match result with | Ok _ -> "incorrect" | Error e -> e

    Assert.True(Result.isError result)
    Assert.True((e = "No Good!"))