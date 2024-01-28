module TcpProxyMocks

open Application.Types
open System.IO

type TcpProxyMock() = 
    let reader = new StreamReader(new MemoryStream())

    interface ITcpProxy with
        member _.Reader = reader
        member _.WriteAsync (message:string) =
            async {
                ()
            }

        member _.ReadAsync() = 
            async {
                return "Mock"
            }




    