open System
open System.IO
open System.Net.Sockets

let server = "####"
let port  = 6667
let channel = "#####"
let nick = "####"

Console.ForegroundColor <- ConsoleColor.DarkRed

let irc_client = new TcpClient();
irc_client.Connect(server, port)

let irc_reader = new StreamReader(irc_client.GetStream())
let irc_writer = new StreamWriter(irc_client.GetStream())
irc_writer.AutoFlush <- true

irc_writer.WriteLine(sprintf "NICK %s\r\n" nick)
irc_writer.WriteLine(sprintf "USER %s %s %s %s\r\n" nick nick nick nick)

let irc_ping (writer : StreamWriter) (line:string) =
    let cookie = (line.Split ':')[1]
    let output = sprintf "PONG :%s %s\r\n" cookie server

    writer.WriteLine(output)

let joinChannel (writer : StreamWriter) =
    let output = sprintf "JOIN %s\r\n" channel
    irc_writer.WriteLine(output)
    writer.WriteLine(output)

let irc_privmsg (writer : StreamWriter) (phrase : string) =
    writer.WriteLine(sprintf "PRIVMSG %s %s\r\n" channel phrase)

let (|Contains|_|) (ircResponse:string) (serverInput:string) = 
    if serverInput.Contains(ircResponse) then
        Some (ircResponse)
    else
        None

let (|Command|_|) (command:string) (serverInput:string) =
    if serverInput.Substring(serverInput.Substring(1).IndexOf(":") + 2).StartsWith(command) then
        Some(command)
    else
        None

while(irc_reader.EndOfStream = false) do
    let line = irc_reader.ReadLine()

    match line with
    | Command "!date" input -> irc_privmsg irc_writer (sprintf "%A" System.DateTime.Now)
    | Contains "PING" input -> irc_ping irc_writer line
    | Contains "+iwx" input -> joinChannel irc_writer
    | _ -> Console.WriteLine(line)