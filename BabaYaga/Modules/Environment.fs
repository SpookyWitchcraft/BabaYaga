module Modules.Environment

open Azure.Security.KeyVault.Secrets;
open System
open Azure.Core
open Azure.Identity
open Application.Types

let options = 
    let options = SecretClientOptions()
    options.Retry.Delay <- TimeSpan.FromSeconds(2)
    options.Retry.MaxDelay <- TimeSpan.FromSeconds(16)
    options.Retry.MaxRetries <- 5
    options.Retry.Mode <- RetryMode.Exponential
    options

let client = 
    SecretClient(new Uri(Environment.GetEnvironmentVariable("by-vault")), new DefaultAzureCredential(), options)

let getValue key = 
    let response = client.GetSecret key
    let kvs = response.Value
    kvs.Value

type Environment() = 
    interface IEnvironment with
        member _.GetSecrets = 
            #if DEBUG
                let text = System.IO.File.ReadAllLines(".env")
                text
                |> Seq.map (fun x -> x.Split('='))
                |> Seq.map (fun y -> y[0], y[1])
                |> dict
            #else
                dict [
                "by-auth-url", getValue "by-auth-url"
                "by-client-id", getValue "by-client-id"
                "by-client-secret", getValue "by-client-secret"
                "by-audience", getValue "by-audience"
                "by-api-url", getValue "by-api-url"
                "by-server", getValue "by-server"
                "by-port", getValue "by-port"
                "by-channel", getValue "by-channel"
                "by-nick", getValue "by-nick"
                "by-password", getValue "by-password"
            ]
            #endif

let (=?) left right = 
    System.String.Equals(left, right, System.StringComparison.CurrentCultureIgnoreCase)