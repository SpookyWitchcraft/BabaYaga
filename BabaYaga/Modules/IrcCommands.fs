module IrcCommands

open Modules.ConsoleWriter
open Modules
open Application.Types

let getMessageInfo (line:string) = 
    let split = line.Split(':')
    
    match split.Length with
    | a when a < 3 -> Some({ UserInfo = ""; Channel = ""; Message = line})
    | _ -> 
        let messageDetails = split[1].Split(' ')
        match messageDetails with
        | a when a.Length < 4 -> 
            None
        | _ -> 
            Some({ UserInfo = split[1]; Channel = messageDetails[1]; Message = split[2]})

type IrcBroadcaster (environment:IEnvironment, tcp:ITcpProxy) = 
    let server = environment.GetSecrets["by-server"]
    let port  = int (environment.GetSecrets["by-port"])
    let channel = environment.GetSecrets["by-channel"]
    let nick = environment.GetSecrets["by-nick"]
    let password = environment.GetSecrets["by-password"]
    
    let identify line = 
        async {
            let output = sprintf "nickserv identify %s\r\n" password
            writeText <| Input line
            do! tcp.WriteAsync(output) 
        }

    let joinChannel line = 
        async {
            let output = sprintf "JOIN %s" channel
            writeText <| Input line
            writeText <| Output output
            do! tcp.WriteAsync(output + "\r\n") 
        }

    interface IIrcBroadcaster with
        member _.InitializeCommunication () = 
            async {
                do! tcp.WriteAsync(sprintf "NICK %s\r\n" nick)
                do! tcp.WriteAsync(sprintf "USER %s 0 * %s\r\n" nick nick) 
            }

        member _.Ping line = 
            async {
                let cookie = (line.Split ':')[1]
                let output = sprintf "PONG :%s %s" cookie server
                do! tcp.WriteAsync(output + "\r\n") 

                writeInputAndOutput (Input line) (Output output)
            }

        member _.Privmsg message = 
            async {
                do! tcp.WriteAsync(sprintf "PRIVMSG %s %s\r\n" channel message)

                writeText <| Output message
            }

        member _.IdentifyAndJoin line = 
            async {
                do! identify line
                do! joinChannel line
            }