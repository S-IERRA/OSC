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
        //ServerMember member = new ServerMember(){Server = server, Permissions = Permissions.Administrator,};
        
        //server.Users.Add(member);
        Servers.Add(server);
            
        await SaveChangesAsync();
    }

    //TODO: Create the possibility to join via invites not ids
    public async Task JoinServer(int serverId, UserProperties user, SocketUser socketUser = null)
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
        
        //server.Users.Add(user);
            
        await SaveChangesAsync();

        await socketUser.Send(Events.JoinedServer, JsonConvert.SerializeObject(server));
    }
}