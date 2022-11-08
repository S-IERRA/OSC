using ChatServer.Objects;
using Microsoft.EntityFrameworkCore;

namespace ChatServer.Handlers;

public partial class EntityFrameworkOrm
{
    //Todo: May need to remove the server from the user side, if there's no need, swap the server search for a user search due to higher performance
    /*
    public async Task KickUser(int serverId, ServerMember mod, int targetId, SocketUser socketUser)
    {
        if (!mod.Permissions.HasFlag(Permissions.CanKick))
        {
            await socketUser.Send(OpCodes.Unauthorized);
            return;
        }

        ServerObject? server = await Servers.Where(x => x.Id == serverId)
            .Include(s => s.Users)
            .FirstOrDefaultAsync();

        ServerMember? firstOrDefault = server.Users.FirstOrDefault(x => x == targetId);
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

    public async Task BanUser(int serverId, ServerMember mod, int targetId, SocketUser socketUser)
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

        ServerMember? user = server.Users.FirstOrDefault(x => x == targetId);
        if (user is not null)
            server.Users.Remove(user);

        server.BannedUsers.Add(targetId);
    }    */
}