using ChatShared.DTos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

#pragma warning disable CS0649
#pragma warning disable CS8618

namespace ChatServer.Rewrite;

//Todo: MAJOR runtime error with database model creation
public class EntityFramework3 : DbContext
{
    public EntityFramework3(DbContextOptions options) : base(options) { }
    
    public DbSet<User> Users { get; set; }
    public DbSet<Server> Servers { get; set; }
    public DbSet<Member> Members { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    }
}

public class Factory2 : IDesignTimeDbContextFactory<EntityFramework3>
{
    public EntityFramework3 CreateDbContext(string[]? args = null)
    {
        DbContextOptionsBuilder builder = new DbContextOptionsBuilder<EntityFramework3>();
        builder.UseNpgsql("Host=localhost;Port=5432;Username=postgres;Password=root;Database=chat");
        
        return new(builder.Options);
    }
}