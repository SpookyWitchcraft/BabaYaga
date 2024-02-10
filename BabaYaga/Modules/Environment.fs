module Modules.Environment

let testDict = 
    dict [
        "AUTH_URL", "http://auth.com"
        "CLIENT_ID", "123"
        "CLIENT_SECRET", "123"
        "AUDIENCE", "123"
        "API_URL", "http://apiurl.com"
        "SERVER", "123"
        "PORT", "123"
        "CHANNEL", "#123"
        "NICK", "123"
        "PASSWORD", "123"
    ]

let getEnvironmentVariables = 
    if not <| System.IO.File.Exists(".env") then
        testDict
    else
        let text = System.IO.File.ReadAllLines(".env")
        text
        |> Seq.map (fun x -> x.Split('='))
        |> Seq.map (fun y -> y[0], y[1])
        |> dict

let (=?) left right = 
    System.String.Equals(left, right, System.StringComparison.CurrentCultureIgnoreCase)