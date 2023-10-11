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
    ignore <| TcpClientProxy.writeAsync(sprintf "NICK %s\r\n" nick)
    ignore <| TcpClientProxy.writeAsync(sprintf "USER %s 0 * %s\r\n" nick nick) 

let irc_ping (line:string) =
    let cookie = (line.Split ':')[1]
    let output = sprintf "PONG :%s %s" cookie server
    TcpClientProxy.writeAsync(output + "\r\n") 
    ConsoleWriter.writeInputAndOutput (Input line) (Output output)
    

let identify (line:string) =
    let output = sprintf "nickserv identify %s\r\n" password
    writeText <| Input line
    TcpClientProxy.writeAsync(output) 

let joinChannel (line:string) =
    let output = sprintf "JOIN %s" channel
    writeText <| Input line
    writeText <| Output output
    TcpClientProxy.writeAsync(output + "\r\n") 

let irc_privmsg (input : string) (message : string) =
    TcpClientProxy.writeAsync(sprintf "PRIVMSG %s %s\r\n" channel message) 
    //this is the actual command someone typed
    writeText <| Command input
    //this is the actual output (shouldn't be in the same function)
    writeText <| Output message

let identifyAndJoin (line:string) = 
    identify line
    joinChannel line