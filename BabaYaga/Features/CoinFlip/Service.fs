module CoinFlip.Service

open System

let flip () = 
    let rand = new Random()
    let results = rand.Next(0, 2)
    match results with
    | 0 -> "tails"
    | _ -> "heads"

let handleFlipCommand () = 
    async {
        do! IrcCommands.privmsg <| flip ()
    }