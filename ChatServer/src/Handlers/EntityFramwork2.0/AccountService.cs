namespace ChatServer.Handlers;

//Possibly inject the socket message in here?
public partial record AccountService(EntityFramework Context, SocketUser SocketUser);