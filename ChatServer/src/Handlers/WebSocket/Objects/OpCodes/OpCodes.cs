namespace ChatServer.Objects
{
    public enum OpCodes
    {
        //Server
        Event = 0,
        Hello = 1,
        HeartBeat = 3,

        //Client
        Identify = 2,
        Register = 5,
        HeartBeatAck = 4,
        SendMessage = 6,
        CreateServer = 7,
        
        //Errors
        InvalidRequest = 400,
        TooManyRequests = 429,
        ConnectionClosed = 444,
        InternalServerError = 500
    }

    public enum Events
    {
        Null = 0,
        
        Identified = 1,
        LoggedOut = 2,
        Registered = 3,
        
        MessageReceived = 4,
    }
}