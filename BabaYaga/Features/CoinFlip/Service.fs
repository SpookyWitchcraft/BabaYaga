module CoinFlip.Service

open System

let flip () = 
    let rand = Random()
    let results = rand.Next(0, 2)
    match results with
    | 0 -> "tails"
    | _ -> "heads"

let handleFlipCommand (ircCommand : string -> Async<unit>) = 
    async {
        do! ircCommand <| flip ()
    }