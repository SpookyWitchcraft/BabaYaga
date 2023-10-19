module TcpClientProxy

open Modules.Environment

open System.Net.Sockets
open System.IO

let server = getEnvironmentVariables["SERVER"]
let port  = int (getEnvironmentVariables["PORT"])

let client = new TcpClient();
client.Connect(server, port)

let reader = new StreamReader(client.GetStream())
let writer = new StreamWriter(client.GetStream())
writer.AutoFlush <- true

let writeAsync (message:string) =
    async {
        do! writer.WriteAsync(message) |> Async.AwaitTask
    }

let readAsync () = 
    async {
        let! line = reader.ReadLineAsync() |> Async.AwaitTask

        return line
    }