using ChatServer.Objects;
using Microsoft.EntityFrameworkCore;

namespace ChatServer.Handlers;

public partial record AccountService
{
     public async Task CreateServer(CreateServerEvent createServerEvent, User user)
    {
        if (createServerEvent.Name.Length is < 3 or > 27)
        {
            await SocketUser.Send(OpCodes.InvalidRequest, "Invalid server name length.");

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
    }

    public async Task DeleteServer(Member owner, Server server)
    {
        if(server.Owner != owner.User)
        {
            await SocketUser.Send(OpCodes.InvalidRequest, "You are not the owner of this server.");

            return;
        }
        
        Context.Servers.Remove(server);
        
        await Context.SaveChangesAsync();
        
        await SocketUser.Send(Events.ServerLeft, server);
    }

    public async Task JoinServer(string inviteCode, Member member)
    {
        if (await Context.Servers.FindAsync(inviteCode) is not { } server)
        {
            await SocketUser.Send(OpCodes.InvalidRequest, "Invalid invite code.");

            return;
        }

        if (server.Members.Any(x => x.User == member.User))
        {
            await SocketUser.Send(OpCodes.InvalidRequest, "You are already a member of this server.");

            return;
        }

        server.Members.Add(member);

        await Context.SaveChangesAsync();

        await SocketUser.Send(Events.ServerJoined, server);
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