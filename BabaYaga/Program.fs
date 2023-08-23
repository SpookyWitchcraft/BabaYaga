open System
open System.IO
open System.Net.Sockets

type ChannelMessage = 
    { UserInfo: string; Channel: string; Message: string }

type ConsoleMessage = 
    | Command of string
    | Input of string
    | Output of string

let server = Environment.GetEnvironmentVariable("SERVER")
let port  = int (Environment.GetEnvironmentVariable("PORT"))
let channel = Environment.GetEnvironmentVariable("CHANNEL")
let nick = Environment.GetEnvironmentVariable("NICK")

Console.ForegroundColor <- ConsoleColor.DarkRed

let irc_client = new TcpClient();
irc_client.Connect(server, port)

let irc_reader = new StreamReader(irc_client.GetStream())
let irc_writer = new StreamWriter(irc_client.GetStream())
irc_writer.AutoFlush <- true

irc_writer.WriteLine(sprintf "NICK %s\r\n" nick)
irc_writer.WriteLine(sprintf "USER %s %s %s %s\r\n" nick nick nick nick)

let writeText (input:ConsoleMessage) = 
    match input with
    | Command message -> 
        Console.ForegroundColor <- ConsoleColor.Red
        Console.WriteLine(message)
    | Input message -> 
        Console.ForegroundColor <- ConsoleColor.DarkRed
        Console.WriteLine(message)
    | Output message -> 
        Console.ForegroundColor <- ConsoleColor.DarkYellow
        Console.WriteLine(message)
    

let irc_ping (writer : StreamWriter) (line:string) =
    let cookie = (line.Split ':')[1]
    let output = sprintf "PONG :%s %s\r\n" cookie server

    writer.WriteLine(output)

let joinChannel (writer : StreamWriter) =
    let output = sprintf "JOIN %s\r\n" channel
    irc_writer.WriteLine(output)
    writer.WriteLine(output)

let irc_privmsg (writer : StreamWriter) (message : string) =
    writer.WriteLine(sprintf "PRIVMSG %s %s\r\n" channel message)
    writeText <| Output message

let (|Contains|_|) (ircResponse:string) (serverInput:string) = 
    if serverInput.Contains(ircResponse) then
        Some (ircResponse)
    else
        None

let (|Commandz|_|) (command:string) (serverInput:string) =
    if serverInput.Substring(serverInput.Substring(1).IndexOf(":") + 2).StartsWith(command) then
        writeText <| Command serverInput
        Some(command)
    else
        None

let getSomeInfo (line:string) = 
    let split = line.Split(':')
    
    if split.Length < 3 then
        None
    else
        let messageDetails = split[1].Split(' ')
        if messageDetails.Length < 4 then
            None
        else
            Some({ UserInfo = split[1]; Channel = messageDetails[2]; Message = split[2]})
    

while(irc_reader.EndOfStream = false) do
    let line = irc_reader.ReadLine()

    let x = getSomeInfo line

    //match x with
    //| Some a -> Console.WriteLine(line)
    //| _ -> Console.WriteLine(line)

    match line with
    | Commandz "!date" input -> irc_privmsg irc_writer (sprintf "%A" System.DateTime.Now)
    | Contains "PING" input -> irc_ping irc_writer line
    | Contains "+iwx" input -> joinChannel irc_writer
    | _ -> writeText <| Input line