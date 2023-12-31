﻿module Modules.Environment

let getEnvironmentVariables = 
    let text = System.IO.File.ReadAllLines(".env")
    text
    |> Seq.map (fun x -> x.Split('='))
    |> Seq.map (fun y -> y[0], y[1])
    |> dict

let (=?) left right = 
    System.String.Equals(left, right, System.StringComparison.CurrentCultureIgnoreCase)