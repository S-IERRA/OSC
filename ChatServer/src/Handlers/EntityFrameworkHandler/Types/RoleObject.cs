using ChatServer.Objects;
using ChatShared.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatServer.Handlers;

/*
public class Role : RoleShared, IEntityTypeConfiguration<RoleShared>
{
    public void Configure(EntityTypeBuilder<RoleShared> builder)
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
*/

public class Role : RoleShared
{
}