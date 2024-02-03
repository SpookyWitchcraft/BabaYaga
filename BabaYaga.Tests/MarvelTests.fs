module MarvelTests

open Xunit
open System.Text.Json
open Marvel.Service
open IrcCommands
open TcpProxyMocks
open ClientProxyMocks
open Auth0.Service
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
    let service = MarvelService(httpSuccess, auth)
    let defaultHero = {Id = "0"; Name = "NoOne"; Description = "Burger"}

    let result = service.GetMarvelCharacter "Juggernaut" |> Async.RunSynchronously

    Assert.True(Result.isOk result)
    Assert.True((Result.defaultValue defaultHero result).Description = "Big")
