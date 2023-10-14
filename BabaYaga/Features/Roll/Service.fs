module Roll.Service

open System

let getDice (message:string) = 
    let rollInfo = message.Split('d')
    let dice = int rollInfo[0]
    let sides = int rollInfo[1]
    let rand = new Random()
    let values = List.init dice (fun _ -> rand.Next(1, sides + 1))
    let sum = List.sum values
    let agg = String.Join(",", values)
    $"You rolled {agg} for a total of {sum}"

let handleRollCommand (message:string) = 
    async {
        do! IrcCommands.privmsg <| getDice message
    }