using System.Text.Json.Serialization;
using ChatServer.Objects;
using ChatShared.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatServer.Handlers;

/*
//Convert to combined key
public class Invite : InviteShared, IEntityTypeConfiguration<InviteShared>
{
    public void Configure(EntityTypeBuilder<InviteShared> builder)
    {
        builder.HasIndex(e => e.InviteCode);
        builder.HasKey(e => new {e.ServerId, e.InviteCode});

        builder.Property(e => e.InviteCode).HasMaxLength(25);
    }
}

public class Server : ServerShared, IEntityTypeConfiguration<ServerShared>
{
    public void Configure(EntityTypeBuilder<ServerShared> builder)
    {
        builder.Property(e => e.Id).ValueGeneratedOnAdd();
        builder.Property(e => e.Name).IsRequired();
        builder.Property(e => e.OwnerId).IsRequired();
        
        builder.HasOne(e => e.Owner)
            .WithMany()
            .HasForeignKey(e => e.OwnerId)
            .IsRequired();

        builder.HasMany(e => e.Channels)
            .WithOne(e=>e.Server)
            .HasForeignKey(e => e.ServerId)
            .IsRequired();
    }
}

//Remove id convert to combined key
public class Member : MemberShared, IEntityTypeConfiguration<MemberShared>
{
    public void Configure(EntityTypeBuilder<MemberShared> builder)
    {
        builder.HasKey(e => new {e.userId, e.serverId});
        builder.Property(e => e.serverId).IsRequired();
        
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
*/

public class Invite : InviteShared
{
}

public class Server : ServerShared
{
}

public class Member : MemberShared
{
}