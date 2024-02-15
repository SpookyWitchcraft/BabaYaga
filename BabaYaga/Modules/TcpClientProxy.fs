module TcpClientProxy

open System.Net.Sockets
open System.IO
open Application.Types

type TcpProxy(environment:IEnvironment) = 
    let server = environment.GetSecrets["by-server"]
    let port  = int (environment.GetSecrets["by-port"])

    let client = new TcpClient();
    do client.Connect(server, port)

    let reader = new StreamReader(client.GetStream())
    let writer = new StreamWriter(client.GetStream())
    do writer.AutoFlush <- true

    interface ITcpProxy with
        member _.Reader = reader
        member _.WriteAsync (message:string) =
            async {
                do! writer.WriteAsync(message) |> Async.AwaitTask
            }

        member _.ReadAsync() = 
            async {
                let! line = reader.ReadLineAsync() |> Async.AwaitTask

                return line
            }