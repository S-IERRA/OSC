using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using ChatServer.Objects;
using Google.Protobuf.WellKnownTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;

#pragma warning disable CS0649
#pragma warning disable CS8618

namespace ChatServer.Handlers;

//Try to implement this into an interface in the future
//Todo: remove the socketuser erroring, move that else where
public interface ITestInterface
{
    public void Wrapper()
    {
        Test();
    }
    
    public abstract void Test();
}

//Todo: Move this to a different file
public class CreateServerEvent
{
    [JsonProperty("name")] public string Name { get; set; }
}

//Todo: move these into partials
//Todo: Switch to System.Text.Json
public class EntityFramework2 : DbContext
{
    public EntityFramework2(DbContextOptions options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Server> Servers { get; set; }

    [Obsolete]
    public async Task CreateUser()
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

    public async Task Register(LoginRegisterEvent registerEvent, SocketUser socketUser)
    {
        if (registerEvent.Username is null || registerEvent.Email is null)
        {
            await socketUser.Send(OpCodes.InvalidRequest, "Fields are missing.");
            return;
        }

        if (!Regex.IsMatch(registerEvent.Email, @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$",
                RegexOptions.NonBacktracking | RegexOptions.Compiled)
            || registerEvent.Email.Length > 254)
        {
            await socketUser.Send(OpCodes.InvalidRequest, "Invalid email.");
            return;
        }

        if (registerEvent.Password.Length is < 6 or > 27)
        {
            await socketUser.Send(OpCodes.InvalidRequest, "Invalid password length.");
            return;
        }

        if (Users.Any(user => user.Email == registerEvent.Email))
        {
            await socketUser.Send(OpCodes.InvalidRequest, "Email already exists.");
            return;
        }

        Users.Add(new User()
        {
            Email = registerEvent.Email,
            Password = registerEvent.Password,
            Username = registerEvent.Username,
        });

        await SaveChangesAsync();

        await socketUser.Send(Events.Registered);
    }
    
    public async Task Login(LoginRegisterEvent loginEvent, SocketUser user)
    {
        if (await Users.SingleOrDefaultAsync(x => x.Email == loginEvent.Email && x.Password == loginEvent.Password) 
            is not { } userSession)
        {
            await user.Send(OpCodes.InvalidRequest, "Invalid username or password");

            return;
        }

        userSession.Session = RandomImpl.RandomString(24);

        await SaveChangesAsync();

        user.IsIdentified = true;

        await user.Send(Events.Identified, JsonConvert.SerializeObject(userSession));
    }

    public async Task Login(string session, SocketUser socketUser)
    {
        if(socketUser.IsIdentified)
            return;

        if (await Users.SingleOrDefaultAsync(x => x.Session == session) is
            not { } userSession)
        {
            await socketUser.Send(OpCodes.InvalidRequest);

            return;
        }
        
        socketUser.IsIdentified = true;

        await socketUser.Send(Events.Identified, JsonConvert.SerializeObject(userSession));
    }

    public async Task LogOut(User user, SocketUser socketUser)
    {
        if (!socketUser.IsIdentified)
            return;
        
        user.Session = null;

        await SaveChangesAsync();

        socketUser.IsIdentified = false;

        await socketUser.Send(Events.LoggedOut);
    }
    
    public async Task CreateServer(CreateServerEvent createServerEvent, User user, SocketUser socketUser)
    {
        
        if (createServerEvent.Name.Length is < 3 or > 27)
        {
            await socketUser.Send(OpCodes.InvalidRequest, "Invalid server name length.");

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

        Servers.Add(server);

        await SaveChangesAsync();

        await socketUser.Send(Events.ServerCreated, JsonConvert.SerializeObject(server));
    }

    public async Task DeleteServer(Member owner, Server server, SocketUser socketUser)
    {
        if(server.Owner != owner.User)
        {
            await socketUser.Send(OpCodes.InvalidRequest, "You are not the owner of this server.");

            return;
        }
        
        Servers.Remove(server);
        
        await SaveChangesAsync();
        
        await socketUser.Send(Events.ServerLeft, JsonConvert.SerializeObject(server));
    }

    public async Task JoinServer(string inviteCode, Member member, SocketUser socketUser)
    {
        //Todo: iterate thru the invite codes
        if (await Servers.FirstOrDefaultAsync(x=>x.InviteCodes[0] != inviteCode) is not {} server)
        {
            await socketUser.Send(OpCodes.InvalidRequest, "Invalid invite code.");

            return;
        }

        if (server.Members.Any(x => x.User == member.User))
        {
            await socketUser.Send(OpCodes.InvalidRequest, "You are already a member of this server.");

            return;
        }

        server.Members.Add(member);

        await SaveChangesAsync();

        await socketUser.Send(Events.ServerJoined, JsonConvert.SerializeObject(server));
    }
    
    //For the love of god rework these
    public async Task JoinServerLazy(string inviteCode, Member member, Range range, SocketUser socketUser)
    {
        if (Servers.Any(x=>x.InviteCodes[0] != inviteCode))
        {
            await socketUser.Send(OpCodes.InvalidRequest, "Invalid invite code.");

            return;
        }

        Server server = await Servers.Where(x => x.InviteCodes[0] == inviteCode)
            .Include(x => x.Members)
            .Take(range)
            .FirstOrDefaultAsync();
        
        server.Members.Add(member);
        
        await SaveChangesAsync();

        await socketUser.Send(Events.ServerJoined, JsonConvert.SerializeObject(server));
    }
    
    //this has to be done here as we don't include the servers in the member
    public async Task GetServers(SocketUser socketUser)
    {
        if (await Users.Where(x => x.Session == socketUser.SessionId).Include(x=>x.Servers).FirstOrDefaultAsync() is
            not { } userSession)
        {
            await socketUser.Send(OpCodes.InvalidRequest);

            return;
        }

        await socketUser.Send(OpCodes.RequestServers, JsonConvert.SerializeObject(userSession.Servers));
    }
    
    public async Task GetChannelMessages(Channel channel, Range range, SocketUser socketUser)
    {
        /*
        if (await Channels.Where(x => x.Id == channel.Id).Include(x => x.Messages).FirstOrDefaultAsync() is
            not { } channelSession)
        {
            await socketUser.Send(OpCodes.InvalidRequest);

            return;
        }

        await socketUser.Send(OpCodes.RequestChannelMessages, JsonConvert.SerializeObject(channelSession.Messages));
        */
        
    }
    
    public async Task GetChannelMessagesLazy(Channel channel, Range range, SocketUser socketUser)
    {
        /* why the fuck is this purple
        if (await Channels.Where(x => x.Id == channel.Id).Include(x => x.Messages).FirstOrDefaultAsync() is
            not { } channelSession)
        {
            await socketUser.Send(OpCodes.InvalidRequest);

            return;
        }

        await socketUser.Send(OpCodes.RequestChannelMessages, JsonConvert.SerializeObject(channelSession.Messages));
        */
    }

    public async Task SendMessage()
    {
        
    }
}

//Todo: use hashmaps on collections
public class Factory : IDesignTimeDbContextFactory<EntityFramework2>
{
    public EntityFramework2 CreateDbContext(string[]? args = null)
    {
        DbContextOptionsBuilder builder = new DbContextOptionsBuilder<EntityFramework2>();
        builder.UseNpgsql("Host=localhost;Port=5432;Username=postgres;Password=root;Database=chat");
        
        return new EntityFramework2(builder.Options);
    }
}

public enum Status
{
    Online,
    Offline,
}

public class User : IEntityTypeConfiguration<User>
{
    [NotMapped]
    private static readonly Timestamp Now = Timestamp.FromDateTime(DateTime.UtcNow);
    
    public int Id { get; set; }
    
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required string Email    { get; set; }
    public string? Session { get; set; }
    
    public string? Icon { get; set; }
    public string? Bio  { get; set; }
    
    public Status Status { get; set; }
    
    public DateTime LastOnline { get; set; } = DateTime.UtcNow;
    public DateTime Created    { get; set; } = DateTime.UtcNow;
    
    public virtual List<Server> Servers { get; set; }

    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(e => e.Id).ValueGeneratedOnAdd();
        
        builder.Property(e => e.Username).IsRequired();
        builder.Property(e => e.Password).IsRequired();
        builder.Property(e => e.Email).IsRequired();

        builder.Property(e => e.Status).HasDefaultValue(Status.Offline);
    }
}

public class Role : IEntityTypeConfiguration<Role>
{
    public int Id { get; set; }
    
    private int userId;
    private int serverId;
    
    public required Server Server { get; set; }
    public Member   User   { get; set; }

    public required string Name   { get; set; }
    public required int    Color  { get; set; } //Hex
    
    public required Permissions Permissions { get; set; }

    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.Property(e => e.Id).ValueGeneratedOnAdd();
        
        builder.Property(e => e.Name).IsRequired();
        builder.Property(e => e.Color).IsRequired();
        builder.Property(e => e.Permissions).IsRequired();
        
        builder.HasOne(e => e.Server)
            .WithMany(e => e.Roles)
            .HasForeignKey(e => e.serverId);

        builder.HasOne(e=>e.User)
            .WithMany(e=>e.Roles)
            .HasForeignKey(e=>e.userId);
    }
}

public class Member : IEntityTypeConfiguration<Member>
{
    public int Id { get; set; }
    
    private int userId;
    private int serverId;
    
    public Permissions Permissions { get; set; }
    public DateTime    Joined      { get; set; } = DateTime.UtcNow;
    
    public Server Server { get; set; }
    public User   User   { get; set; }
    
    public virtual List<Role> Roles { get; set; }

    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.Property(e => e.Id).ValueGeneratedOnAdd();
        builder.Property(e => e.serverId).IsRequired();
        
        builder.HasKey(e => new { e.User.Id, e.userId });
        
        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.userId)
            .IsRequired();
        
        builder.HasOne(e => e.Server)
            .WithMany(e => e.Members)
            .HasForeignKey(e => e.serverId)
            .IsRequired();
    }
}

public class Server : IEntityTypeConfiguration<Server>
{
    public int Id { get; set; }
    private int _ownerId;
    
    public required User   Owner { get; set; }
    public required string Name  { get; set; }
    
    public string?   Description  { get; set; }
    public string?   Icon         { get; set; }
    public string?   Banner       { get; set; }
    public string[]? InviteCodes  { get; set; }
    
    public DateTime Created { get; set; } = DateTime.UtcNow;

    public virtual List<Member>  Members  { get; set; }
    public virtual List<Role>    Roles    { get; set; }
    public virtual List<Channel> Channels { get; set; }

    public void Configure(EntityTypeBuilder<Server> builder)
    {
        builder.Property(e => e.Id).ValueGeneratedOnAdd();
        builder.Property(e => e.Name).IsRequired();
        builder.Property(e => e.Owner).IsRequired();
        
        builder.HasOne(e => e.Owner)
            .WithMany()
            .HasForeignKey(e => e._ownerId)
            .IsRequired();
    }
}

public class Channel : IEntityTypeConfiguration<Channel>
{
    public int Id { get; set; }

    public required string Name { get; set; }
    public required Permissions ViewPermission { get; set; }
    
    public required Server Server { get; set; }
    
    public virtual List<Message> Messages { get; set; }

    public void Configure(EntityTypeBuilder<Channel> builder)
    {
        builder.Property(e => e.Id).ValueGeneratedOnAdd();
        
        builder.Property(e => e.Name).IsRequired();
        builder.Property(e => e.Server).IsRequired();
        builder.Property(e => e.ViewPermission).IsRequired();
        
        builder.HasOne(e => e.Server)
            .WithMany(e => e.Channels)
            .HasForeignKey(e => e.Id)
            .IsRequired();
    }
}

public class Message : IEntityTypeConfiguration<Message>
{
    public int Id { get; set; }
    
    public required User    Author  { get; set; }
    public required Server  Server  { get; set; }
    public required Channel Channel { get; set; }
    public required string  Content { get; set; }
    
    public DateTime Sent { get; set; } = DateTime.UtcNow;
    
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.Property(e => e.Id).ValueGeneratedOnAdd();
        
        builder.Property(e => e.Author).IsRequired();
        builder.Property(e => e.Server).IsRequired();
        builder.Property(e => e.Channel).IsRequired();
        builder.Property(e => e.Content).IsRequired();

        builder.HasOne(e => e.Author)
            .WithMany()
            .HasForeignKey(e => e.Id)
            .IsRequired();
        
        builder.HasOne(e => e.Channel)
            .WithMany(e => e.Messages)
            .HasForeignKey(e => e.Id)
            .IsRequired();
    }
}