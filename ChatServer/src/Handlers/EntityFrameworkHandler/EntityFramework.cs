using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using ChatServer.Objects;
using Google.Protobuf.WellKnownTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

#pragma warning disable CS0649
#pragma warning disable CS8618

namespace ChatServer.Handlers;

//Todo: Move this to a different file
public class CreateServerEvent
{ 
    public string Name { get; set; }
}

//Todo: move these into partials
//Todo: Switch to System.Text.Json
public class EntityFramework2 : DbContext
{
    public EntityFramework2(DbContextOptions options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Server> Servers { get; set; }

    [Obsolete]
    public async Task TestMethod()
    {
        User user = new User
        {
            Username = "Test",
            Password = "Test",
            Email = "Test",
        };

        Server server = new Server
        {
            Name = "Test",
            Owner = user,
            Members = new List<Member>(),
            Channels = new List<Channel>(),
        };

        Channel channel = new Channel
        {
            Name = "Test",
            Server = server, 
            ViewPermission = Permissions.Administrator,
            Messages = new List<Message>()
        };

        Message message = new Message
        {
            Author = user,
            Server = server,
            Channel = channel,
            Content = "Test",
        };

        channel.Messages.Add(message);
        
        server.Members.Add(new Member() {User = user, Server = server});
        server.Channels.Add(channel);

        Users.Add(user);
        Servers.Add(server);

        await SaveChangesAsync();
    }
}

public class Factory : IDesignTimeDbContextFactory<EntityFramework2>
{
    public EntityFramework2 CreateDbContext(string[]? args = null)
    {
        DbContextOptionsBuilder builder = new DbContextOptionsBuilder<EntityFramework2>();
        builder.UseNpgsql("Host=localhost;Port=5432;Username=postgres;Password=root;Database=chat");
        
        return new EntityFramework2(builder.Options);
    }
}