module Modules.ConsoleWriter

open System

type ConsoleMessage = 
    | Command of string
    | Input of string
    | Output of string

let writeText (input:ConsoleMessage) = 
    match input with
    | Command message -> 
        Console.ForegroundColor <- ConsoleColor.Red
        Console.WriteLine(message)
        Console.ForegroundColor <- ConsoleColor.DarkRed
    | Input message -> 
        Console.ForegroundColor <- ConsoleColor.DarkRed
        Console.WriteLine(message)
    | Output message -> 
        Console.ForegroundColor <- ConsoleColor.DarkYellow
        Console.WriteLine(message)
        Console.ForegroundColor <- ConsoleColor.DarkRed