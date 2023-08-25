open System
open System.IO
open System.Net.Sockets
open Infrastructure

//!trivia {rounds}
//!coinflip
//!roll 1d6
//!chatgpt {question}
//!marvel {superheroname}

type ChannelMessage = 
    { UserInfo: string; Channel: string; Message: string }

type ConsoleMessage = 
    | Command of string
    | Input of string
    | Output of string

let emptyQuestion = { Question = ""; Category = ""; Answer = "" }

let mutable currentQuestion = emptyQuestion

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
    writer.WriteLine(output)

let irc_privmsg (input : string) (message : string) =
    irc_writer.WriteLine(sprintf "PRIVMSG %s %s\r\n" channel message)
    writeText <| Command input
    writeText <| Output message

let getSomeInfo (line:string) = 
    let split = line.Split(':')
    
    if split.Length < 3 then
        Some({ UserInfo = ""; Channel = ""; Message = line})
    else
        let messageDetails = split[1].Split(' ')
        if messageDetails.Length < 4 then
            None
        else
            Some({ UserInfo = split[1]; Channel = messageDetails[2]; Message = split[2]})

let coinFlip () = 
    let rand = new Random()
    let results = rand.Next(0, 2)
    match results with
    | 0 -> "tails"
    | _ -> "heads"
  
let getDice (message:string) = 
    let rollInfo = message.Split('d')
    let dice = int rollInfo[0]
    let sides = int rollInfo[1]
    let rand = new Random()
    let values = List.init dice (fun _ -> rand.Next(1, sides + 1))
    let sum = List.sum values
    let agg = String.Join(",", values)
    $"You rolled {agg} for a total of {sum}"


let getTriviaQuestion () = 
    let triviaQuestion = Infrastructure.getTriviaQuestion()
    currentQuestion <- triviaQuestion
    triviaQuestion.Question

let (=?) left right = 
    System.String.Equals(left, right, System.StringComparison.CurrentCultureIgnoreCase)

let checkAnswer (message:string) = 
    let results = currentQuestion.Answer =? message
    
    if results then 
        irc_writer.WriteLine(sprintf "PRIVMSG %s %s\r\n" channel $"Correct!  The answer was {currentQuestion.Answer}.")

        currentQuestion <- emptyQuestion
    else
        ()

let handleCommand (input:string) (message:string) = 
    let split = message.Split(' ')
    let command = split[0]

    let out = irc_privmsg input

    match command with
    | "!coinflip" -> out <| coinFlip ()
    | "!roll" -> out <| getDice split[1]
    | "!trivia" -> out <| getTriviaQuestion()
    | "!chatgpt" -> out "not implemented, usage = !chatgpt {question}"
    | "!marvel" -> out "not implemented, usage = !marvel {superhero}"
    | _ -> out "command not found 👻"

while(irc_reader.EndOfStream = false) do
    let line = irc_reader.ReadLine()

    let x = getSomeInfo line


    match x with
    | Some a -> (checkAnswer a.Message)
                match a with
                | y when a.Message.StartsWith("!") -> handleCommand line y.Message
                | _ when a.Message.Contains("PING") -> irc_ping irc_writer line
                | _ when a.Message.Contains("+iwx") -> joinChannel irc_writer
                | _ -> writeText <| Input line
    | _ -> Console.WriteLine(line)