namespace ChatServer.Objects
{
    //Move some of these to error-codes and response codes
    //Move these to differnet files
    //Add error messages into enums, i.e ErrorMessage.InvalidServerId (Server does not exist.)
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

    //Todo: Organise these
    public static class ErrorMessages
    {
        public const string InvalidServerNameLength = "Invalid server name length.";
        public const string NotOwner = "You need to be the owner of this server to delete it.";
        public const string InvalidInvite = "Invalid server invite code.";
        public const string ServerDoesNotExist = "The specified server does not exist.";
        public const string NotAMember = "You are not a member of this server.";
        public const string AlreadyAMember = "You are already a member of this server.";

        public const string MissingFields = "Missing fields.";
        public const string InvalidEmail = "Invalid Email.";
        public const string EmailAlreadyExists = "Email is already registered.";
        public const string InvalidUserOrPass = "Invalid username or password.";
        public const string InvalidPasswordLength = "Invalid password length, password must be 6-27 characters long.";
    }
    
    public enum ErrorCode
    {
        
    }
    
    public enum ResponseCode
    {
        
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