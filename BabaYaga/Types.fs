module Application.Types

type BotState = Unidentified | Identified

type ChannelMessage = 
    { UserInfo: string; Channel: string; Message: string }