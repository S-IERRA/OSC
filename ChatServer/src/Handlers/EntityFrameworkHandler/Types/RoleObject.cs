using ChatServer.Objects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatServer.Handlers;

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