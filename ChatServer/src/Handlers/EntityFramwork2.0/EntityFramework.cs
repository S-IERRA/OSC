using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

#pragma warning disable CS0649
#pragma warning disable CS8618

namespace ChatServer.Handlers;

//Todo: remove "shared" files and replace with DTos
public class EntityFramework : DbContext
{
    public EntityFramework(DbContextOptions options) : base(options) { }
    
    public DbSet<User> Users { get; set; }
    public DbSet<Server> Servers { get; set; }
    public DbSet<Member> Members { get; set; }
    public DbSet<Channel> Channels { get; set; }
    public DbSet<Message> Messages { get; set; }

    public DbSet<ServerSubscriber> Subscribers { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    }
}

public class Factory : IDesignTimeDbContextFactory<EntityFramework>
{
    public EntityFramework CreateDbContext(string[]? args = null)
    {
        DbContextOptionsBuilder builder = new DbContextOptionsBuilder<EntityFramework>();
        builder.UseNpgsql("Host=localhost;Port=5432;Username=postgres;Password=root;Database=chat");
        
        return new(builder.Options);
    }
}