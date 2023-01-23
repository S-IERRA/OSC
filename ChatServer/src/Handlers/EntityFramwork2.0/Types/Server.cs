using ChatShared.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatServer.Handlers;

//Todo: Swap depricated files for the new ones

public class Server : ServerShared, IEntityTypeConfiguration<ServerShared>
{
    public void Configure(EntityTypeBuilder<ServerShared> builder)
    {
        builder.ToTable("Servers");
        
        builder.Property(e => e.Name).IsRequired();
        
        builder.HasOne(e => e.Owner)
            .WithMany()
            .HasForeignKey(e => e.OwnerId)
            .IsRequired();
    }
}

public class Member : ServerMemberShared, IEntityTypeConfiguration<ServerMemberShared>
{
    public void Configure(EntityTypeBuilder<ServerMemberShared> builder)
    {
        builder.ToTable("Members");

        builder.HasKey(x => new { x.UserId, x.ServerId });
        
        builder.HasOne(x => x.Server)
            .WithMany(x => x.Members)
            .HasForeignKey(x => x.ServerId);
    }
}

public class Role : RoleShared, IEntityTypeConfiguration<RoleShared>
{
    public void Configure(EntityTypeBuilder<RoleShared> builder)
    {
        builder.ToTable("Roles");
        
        builder.HasOne(e => e.Server)
            .WithMany(e=>e.Roles)
            .HasForeignKey(e => e.ServerId);
    }
}

public class Invite : InviteShared, IEntityTypeConfiguration<InviteShared>
{
    public void Configure(EntityTypeBuilder<InviteShared> builder)
    {
        builder.ToTable("Invites");

        builder.HasIndex(e => e.InviteCode);
        builder.HasKey(e => new {e.ServerId, e.InviteCode});

        builder.Property(e => e.InviteCode).HasMaxLength(25);
    }
}