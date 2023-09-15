module Application.Types

open System.Net.Sockets
open System.IO
open System.Collections.Generic

type ApplicationState = {
    client : TcpClient
    reader : StreamReader
    writer : StreamWriter
    question : option<Trivia.Types.QuestionStatus>
    rounds : int
    scores : Dictionary<string, int>
}