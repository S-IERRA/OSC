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

public class EntityFramework2 : DbContext
{
    public EntityFramework2(DbContextOptions options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Server> Servers { get; set; }
    public DbSet<Member> Members { get; set; }
    public DbSet<Channel> Channels { get; set; }
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