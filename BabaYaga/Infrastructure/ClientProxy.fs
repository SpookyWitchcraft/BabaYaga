module Infrastructure.ClientProxy

open System.Net.Http
open Modules.Environment

let client = new HttpClient()

let root = getEnvironmentVariables["API_URL"]

let buildUrl (suffix:string) = 
    $"{root}{suffix}"