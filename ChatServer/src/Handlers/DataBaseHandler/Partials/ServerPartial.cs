using ChatServer.Objects;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace ChatServer.Handlers;

public partial class EntityFrameworkOrm
{
    public async Task<ServerObject?> GetServerMembers(int serverId) => await Servers.Where(x 
            => x.Id == serverId)
        .Include(server => server.Users)
        .FirstOrDefaultAsync();
    
    public async Task CreateServer(UserProperties user, ServerObject server)
    {
        server.Users.Add(user);
        Servers.Add(server);
            
        await SaveChangesAsync();
    }

    //TODO: Create the possibility to join via invites not ids
    public async Task JoinServer(int serverId, UserProperties user, SocketUser socketUser)
    {
        if (await Servers.Where(x => x == serverId).Include(s => s.Users).FirstOrDefaultAsync() is not { } server)
        {
            await socketUser.Send(OpCodes.InvalidRequest, "Invalid server Id.");
            return;
        }

        if (server.BannedUsers.Any(x => x == user.UserId))
        {
            await socketUser.Send(OpCodes.Unauthorized);
            return;
        }
        
        server.Users.Add(user);
            
        await SaveChangesAsync();

        await socketUser.Send(Events.JoinedServer, JsonConvert.SerializeObject(server));
    } 
    
    //Todo: May need to remove the server from the user side, if there's no need, swap the server search for a user search due to higher performance
    public async Task KickUser(int serverId, Subscriber mod, int targetId, SocketUser socketUser)
    {
        if (!mod.Permissions.HasFlag(Permissions.CanKick))
        {
            await socketUser.Send(OpCodes.Unauthorized);
            return;
        }

        ServerObject? server = await Servers.Where(x => x.Id == serverId)
            .Include(s => s.Users)
            .FirstOrDefaultAsync();

        UserProperties? firstOrDefault = server.Users.FirstOrDefault(x => x == targetId);
        if (firstOrDefault is null)
        {
            await socketUser.Send(OpCodes.InvalidRequest, "Invalid target id.");
            return;
        }

        if (mod == targetId)
        {
            await socketUser.Send(OpCodes.InvalidRequest, "Can't self-kick");
            return;
        }
        
        server.Users.Remove(firstOrDefault);
    }

    public async Task BanUser(int serverId, Subscriber mod, int targetId, SocketUser socketUser)
    {
        //Check user permissions
        if (!mod.Permissions.HasFlag(Permissions.Administrator))
        {
            await socketUser.Send(OpCodes.Unauthorized);
            return;
        }

        var server = await Servers.Where(x => x.Id == serverId)
            .Include(s => s.Users)
            .Include(s => s.BannedUsers)
            .FirstOrDefaultAsync();

        if (server is null)
        {
            await socketUser.Send(OpCodes.InvalidRequest, "Invalid server Id.");
            return;
        }
        
        if(server.Creator == targetId)
        {
            await socketUser.Send(OpCodes.InvalidRequest, "Can't self-ban");
            return;
        }

        UserProperties? user = server.Users.FirstOrDefault(x => x == targetId);
        if (user is not null)
            server.Users.Remove(user);

        server.BannedUsers.Add(targetId);
    }
}