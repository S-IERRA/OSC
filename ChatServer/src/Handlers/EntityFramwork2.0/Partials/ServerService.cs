using System.Net;
using System.Text.RegularExpressions;
using ChatShared;
using ChatShared.Json;
using ChatShared.Types;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace ChatServer.Handlers;

//Todo: Clean this up
public partial record AccountService
{
    //todo: Join as member
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
        
        Guid serverId = Guid.NewGuid();

        Server server  = new Server
        {
            Id = serverId,
            Name = createServerEvent.Name,
            OwnerId = user.Id
        };
        
        Context.Servers.Add(server);
        Context.Channels.Add(new Channel {
            Id = Guid.NewGuid(),
            ServerId = serverId,
            Name = "General",
            ViewPermissions = Permissions.Member,
        });
        
        await Context.Members.AddAsync(new Member()
        {
            UserId = user.Id,
            ServerId = server.Id,
                
            Permissions = Permissions.Member
        });

        await Context.SaveChangesAsync();
        await SocketUser.Send(Events.ServerCreated, server);
        
        Log.Information($"Server {server.Name} created by {user.Username}");
    }

     public async Task DeleteServer(string? message, User user)
    {
        if (!JsonHelper.TryDeserialize<ServerEvent>(message, out var deleteServerEvent))
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
        try
        {
            if (!JsonHelper.TryDeserialize<JoinServerEvent>(message, out var joinServerEvent))
            {
                await SocketUser.Send(OpCodes.InvalidRequest);
                return;
            }
            
            if (await Context.Servers.Where(y => y.InviteCodes.Any(z => z.InviteCode == joinServerEvent.InviteCode)).Include(x=>x.Members).FirstOrDefaultAsync() is not { } server)
            {
                await SocketUser.Send(OpCodes.InvalidRequest, ErrorMessages.ServerDoesNotExist);
                return;
            }

            if (server.Members.Where(x => x.User.Id == user.Id) is not { })
            {
                await SocketUser.Send(OpCodes.InvalidRequest, ErrorMessages.AlreadyAMember);
                return;
            }

            await Context.Members.AddAsync(new Member()
            {
                UserId = user.Id,
                ServerId = server.Id,
                
                Permissions = Permissions.Member
            });
            
            await Context.SaveChangesAsync();
            await SocketUser.Send(Events.ServerJoined, server);

            Log.Information($"User {user.Username} joined server {server.Name}");
        }
        catch (Exception e)
        {
            Log.Fatal(e, "Error while joining server");
        }
    }

    public async Task LeaveServer(string? message, User user)
    {
        if (!JsonHelper.TryDeserialize<ServerEvent>(message, out var leaveServerEvent))
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
   
    //Fix
    public async Task CreateInvite(string? data, User user)
    {
        try
        {
            if (!JsonHelper.TryDeserialize<CreateInvite>(data, out var createInvite))
            {
                await SocketUser.Send(OpCodes.InvalidRequest);
                return;
            }

            if (await Context.Servers.Where(x=>x.Id == createInvite.ServerId).Include(y=>y.InviteCodes).FirstOrDefaultAsync() is not { } server)
            {
                await SocketUser.Send(OpCodes.InvalidRequest, ErrorMessages.ServerDoesNotExist);
                return;
            }

            /*
            if (server.Owner.Id != SocketUser.UserId)
            {
                await SocketUser.Send(OpCodes.InvalidRequest, ErrorMessages.NotOwner);
                return;
            }*/

            //check if invite already exists
            if (server.InviteCodes.Any(x => x.InviteCode == createInvite.InviteCode))
            {
                await SocketUser.Send(OpCodes.InvalidRequest, ErrorMessages.InvalidInvite);
                return;
            }

            Invite invite = new()
            {
                Id = Guid.NewGuid(),
                ServerId = server.Id,
                InviteCode = createInvite.InviteCode
            };

            server.InviteCodes.Add(invite);
            await Context.SaveChangesAsync();

            await SocketUser.Send(Events.ServerInviteCreated, invite);
            Log.Information($"User {user.Username} created invite {invite.InviteCode} for server {server.Name}");
        }
        catch (Exception e)
        {
            Log.Fatal(e, "Error creating invite");
        }
    }

    //Todo: Permissions vuln, must check if server has this user
    public async Task SubscribeServer(string? message, IPEndPoint endpoint)
    {
		if (!JsonHelper.TryDeserialize<ServerEvent>(message, out var serverEvent))
		{
			await SocketUser.Send(OpCodes.InvalidRequest);
			return;
		}

		if (await Context.Servers.FirstOrDefaultAsync(x => x.Id == serverEvent.ServerId) is not { } server)
        {
            await SocketUser.Send(OpCodes.InvalidRequest, ErrorMessages.ServerDoesNotExist);
            return;
        }

        ServerSubscriber subscriber = new()
        {
            Id = Guid.NewGuid(),
            ServerId = serverEvent.ServerId,

            AddressBytes = endpoint.Address.GetAddressBytes(),
            Port = endpoint.Port,
        };

        await Context.Subscribers.AddAsync(subscriber);
        await Context.SaveChangesAsync();
    }

    //8regjoeiasdz3q5grwadjs:::::: TOdo: probably requires a rewrite of the POCO because idk how else im gonna find the user hauwhiuawiehiut4rwq3uhiaqwrgthuirqwhuirgqweiughtewgrtwseuahi9greuaswhifdeswauhio
	public async Task UnsubscribeServer(string? message, User user)
	{
        
	}
}