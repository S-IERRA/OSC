namespace ChatClient.Handlers
{
    public enum OpCodes
    {
        //Server
        Event = 0,
        Hello = 1,
        HeartBeat = 3,
        SessionUpdate = 9,

        //Client
        Identify = 2,
        Register = 5,
        HeartBeatAck = 4,
        SendMessage = 6,
        
        CreateServer = 7,
        DeleteServer = 14,
        JoinServer = 8,
        LeaveServer = 13,
        RequestServers = 12,
        GetServerMembers = 9,
        BanUser = 10,
        KickUser = 11,

        //Errors
        InvalidRequest = 400,
        Unauthorized = 401,
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
        
        ServerCreated = 5,
        ServerUpdated = 7,
        ServerJoined = 11,
        ServerLeft = 8,
        
        JoinedServer = 6,
        UserIsBanned = 10,

        MessageReceived = 4,
        MessageUpdated = 8,
        MessageDeleted = 9,
    }
}