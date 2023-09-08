module Infrastructure.Modules.ClientProxy

open System.Net.Http

let client = new HttpClient()

let root = ""

let buildUrl (suffix:string) = 
    $"{root}{suffix}"