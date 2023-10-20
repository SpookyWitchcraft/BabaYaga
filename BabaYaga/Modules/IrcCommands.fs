module IrcCommands

open Modules.ConsoleWriter
open Modules.Environment
open Modules

let server = getEnvironmentVariables["SERVER"]
let port  = int (getEnvironmentVariables["PORT"])
let channel = getEnvironmentVariables["CHANNEL"]
let nick = getEnvironmentVariables["NICK"]
let password = getEnvironmentVariables["PASSWORD"]

let initializeCommunication =
    async {
        do! TcpClientProxy.writeAsync(sprintf "NICK %s\r\n" nick)
        do! TcpClientProxy.writeAsync(sprintf "USER %s 0 * %s\r\n" nick nick) 
    }

let ping (line:string) =
    async {
        let cookie = (line.Split ':')[1]
        let output = sprintf "PONG :%s %s" cookie server
        do! TcpClientProxy.writeAsync(output + "\r\n") 

        ConsoleWriter.writeInputAndOutput (Input line) (Output output)
    }
    

let identify (line:string) =
    async {
        let output = sprintf "nickserv identify %s\r\n" password
        writeText <| Input line
        do! TcpClientProxy.writeAsync(output) 
    }

let joinChannel (line:string) =
    async {
        let output = sprintf "JOIN %s" channel
        writeText <| Input line
        writeText <| Output output
        do! TcpClientProxy.writeAsync(output + "\r\n") 
    }

let privmsg (message : string) =
    async {
        do! TcpClientProxy.writeAsync(sprintf "PRIVMSG %s %s\r\n" channel message)

        writeText <| Output message
    }

let identifyAndJoin (line:string) = 
    async {
        do! identify line
        do! joinChannel line
    }