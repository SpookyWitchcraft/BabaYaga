module ClientProxyMocks

open Application.Types
open System.Text.Json

type ClientProxySuccessMock(js:string) = 
    interface IClientProxy with
        member _.Get<'a> (urlSuffix:string) (token:string) = 
            async {
                let obj = JsonSerializer.Deserialize<'a>(js)

                return Ok(obj)
            }

        member _.Post<'a, 'b> (obj: 'a) (auth:AuthType) (url:string) = 
            async {
                let obj = JsonSerializer.Deserialize<'b>(js)

                return Ok(obj)
            }

type ClientProxyFailureMock(js:string) = 
    interface IClientProxy with
        member _.Get<'a> (urlSuffix:string) (token:string) : Async<Result<'a, string>> = 
            async {
                return Error js
            }

        member _.Post<'a, 'b> (obj: 'a) (auth:AuthType) (url:string) : Async<Result<'b, string>> = 
            async {
                return Error js
            }