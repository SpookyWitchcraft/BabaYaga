module RollTests

open Xunit
open Roll.Service

[<Fact>]
let ``getDice should return the correct number of dice`` () =
    let results = getDice "3d11"
    let split = results.Split(',')
    Assert.True(split.Length = 3)