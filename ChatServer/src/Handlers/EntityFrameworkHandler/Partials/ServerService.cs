using ChatServer.Extensions;
using ChatServer.Objects;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace ChatServer.Handlers;

public partial record AccountService
{
     public async Task CreateServer(string? message, User user)
    {
        if (!JsonHelper.TryDeserialize<CreateServerEvent>(message, out var createServerEvent))
        {
            await SocketUser.Send(OpCodes.InvalidRequest);
            return;
        }
        
        if (createServerEvent.Name.Length is < 3 or > 27)
        {
            await SocketUser.Send(OpCodes.InvalidRequest, ErrorMessages.InvalidServerNameLength);

            return;
        }

        Server server = new Server
        {
            Name = createServerEvent.Name,
            Owner = user,
            Members = new List<Member>(),
            Channels = new List<Channel>(),
        };

        Channel channel = new Channel
        {
            Name = "General",
            Server = server, 
            ViewPermission = Permissions.Member,
            Messages = new List<Message>()
        };

        server.Members.Add(new Member() {User = user, Server = server});
        server.Channels.Add(channel);

        Context.Servers.Add(server);

        await Context.SaveChangesAsync();
        await SocketUser.Send(Events.ServerCreated, server);
        
        Log.Information($"Server {server.Name} created by {user.Username}");
    }

     public async Task DeleteServer(string? message, User user)
    {
        if (!JsonHelper.TryDeserialize<DeleteServerEvent>(message, out var deleteServerEvent))
        {
            await SocketUser.Send(OpCodes.InvalidRequest);
            return;
        }
        
        if (await Context.Servers.FirstOrDefaultAsync(x => x.Id == deleteServerEvent.ServerId) is not { } server)
        {
            await SocketUser.Send(OpCodes.InvalidRequest, ErrorMessages.ServerDoesNotExist);
            return;
        }

        if(server.Owner.Id != user.Id)
        {
            await SocketUser.Send(OpCodes.InvalidRequest, ErrorMessages.NotOwner);
            return;
        }
        
        Context.Servers.Remove(server);

        await Context.SaveChangesAsync();
        await SocketUser.Send(Events.ServerLeft, server);
        
        Log.Information($"Server {server.Name} deleted by {user.Username}");
    }

    public async Task JoinServer(string? message, User user)
    {
        if (!JsonHelper.TryDeserialize<JoinServerEvent>(message, out var joinServerEvent))
        {
            await SocketUser.Send(OpCodes.InvalidRequest);
            return;
        }
        
        if (await Context.Servers.FirstOrDefaultAsync(x => x.InviteCodes.Any(h => h.InviteCode == joinServerEvent.InviteCode)) is not { } server)
        {
            await SocketUser.Send(OpCodes.InvalidRequest, ErrorMessages.ServerDoesNotExist);
            return;
        }

        if (server.Members.Any(x => x.User.Id == user.Id))
        {
            await SocketUser.Send(OpCodes.InvalidRequest, ErrorMessages.AlreadyAMember);
            return;
        }

        Member member = new() { Server = server, User = user };
        server.Members.Add(member);

        await Context.SaveChangesAsync();
        await SocketUser.Send(Events.ServerJoined, server);
        
        Log.Information($"User {user.Username} joined server {server.Name}");
    }

    public async Task LeaveServer(string? message, User user)
    {
        if (!JsonHelper.TryDeserialize<LeaveServerEvent>(message, out var leaveServerEvent))
        {
            await SocketUser.Send(OpCodes.InvalidRequest);
            return;
        }
        
        if (await Context.Servers.FirstOrDefaultAsync(x=>x.Id == leaveServerEvent.ServerId) is not { } server)
        {
            await SocketUser.Send(OpCodes.InvalidRequest, ErrorMessages.ServerDoesNotExist);
            return;
        }

        if (server.Members.FirstOrDefault(x => x.User.Id == user.Id) is not { } member)
        {
            await SocketUser.Send(OpCodes.InvalidRequest, ErrorMessages.NotAMember);
            return;
        }

        server.Members.Remove(member);
        
        await Context.SaveChangesAsync();
        await SocketUser.Send(Events.ServerLeft, server.Id);
        
        Log.Information($"User {user.Username} left server {server.Name}");
    }
    
    //For the love of god rework these
    //Range inf load exploit
    public async Task JoinServerLazy(string inviteCode, Member member, Range range)
    {
    }
    
    //this has to be done here as we don't include the servers in the member
    public async Task GetServers()
    {
        if (await Context.Users.Where(x => x.Session == SocketUser.SessionId).Include(x=>x.Servers).FirstOrDefaultAsync() is
            not { } userSession)
        {
            await SocketUser.Send(OpCodes.InvalidRequest);

            return;
        }

        await SocketUser.Send(OpCodes.RequestServers, userSession.Servers);
    }
}