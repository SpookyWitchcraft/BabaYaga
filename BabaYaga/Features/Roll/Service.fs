﻿module Roll.Service

open System

let getDice (message:string) = 
    let rollInfo = message.Split('d')
    let dice = int rollInfo[0]
    let sides = int rollInfo[1]
    let rand = Random()
    let values = List.init dice (fun _ -> rand.Next(1, sides + 1))
    let sum = List.sum values
    let agg = String.Join(",", values)
    $"You rolled {agg} for a total of {sum}"

let handleRollCommand (ircCommand : string -> Async<unit>) (message:string) = 
    async {
        do! ircCommand <| getDice message
    }