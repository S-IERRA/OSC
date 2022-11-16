namespace ChatServer.Handlers;

//Todo: Add logging in the future
public partial record AccountService(EntityFramework2 Context, SocketUser SocketUser);