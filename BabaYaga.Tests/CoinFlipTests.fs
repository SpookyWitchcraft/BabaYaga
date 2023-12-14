module CoinFlipTests

open Xunit
open CoinFlip.Service

[<Fact>]
let ``flip should return heads or tails`` () =
    let flipped = flip ()
    let results = (flipped = "tails" || flipped = "heads")
    Assert.True(results)