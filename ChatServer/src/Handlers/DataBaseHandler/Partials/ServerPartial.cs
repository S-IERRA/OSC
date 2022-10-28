using ChatServer.Objects;
using Microsoft.EntityFrameworkCore;

namespace ChatServer.Handlers;

public partial class TestOrm
{
    public async Task CreateServer(BasicUser user)
    {
        ServerObject server = new ServerObject
        {
            Creator = user,// Users.SingleOrDefault(x => x.Email == "a@gmail.com")
            Name = "test12",
        };

        Servers.Add(server);
            
        await SaveChangesAsync();
    }

    public async Task JoinServer(int serverId, UserProperties user)
    {
        ServerObject? a = await Servers.Where(x => x.Id == serverId).Include(server=> server.Users).FirstOrDefaultAsync();
        if (a is null)
        {
            //await socketUser.Send(OpCodes.InvalidRequest, "Invalid password.");
            return;
        }
        
        a.Users.Add(user);
            
        await SaveChangesAsync();
    }
}