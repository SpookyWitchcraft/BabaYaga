module EnvironmentMock

open Application.Types

type EnvironmentMock() = 
    interface IEnvironment with
        member _.GetSecrets = 
            dict [
                "by-auth-url", "http://auth.com"
                "by-client-id", "123"
                "by-client-secret", "123"
                "by-audience", "123"
                "by-api-url", "http://apiurl.com"
                "by-server", "123"
                "by-port", "123"
                "by-channel", "#123"
                "by-nick", "123"
                "by-password", "123"
            ]