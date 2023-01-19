using ChatShared.DTos;
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

public class Channel : ChannelShared, IEntityTypeConfiguration<ChannelShared>
{
    public void Configure(EntityTypeBuilder<ChannelShared> builder)
    {
        builder.ToTable("Channels");
        
        builder.HasKey(x => new { x.Id, x.ServerId });

        builder.HasMany(x => x.Messages)
            .WithOne(x => x.Channel)
            .HasForeignKey(x => new { x.ChannelId, x.ServerId });
    }
}

public class Message : MessageShared, IEntityTypeConfiguration<MessageShared>
{
    public void Configure(EntityTypeBuilder<MessageShared> builder)
    {
        builder.ToTable("Messages");
        
        builder.HasKey(e => e.Id);

        builder.Property(e => e.AuthorId).IsRequired();
        builder.HasOne(e => e.Author)
            .WithMany()
            .HasForeignKey(e => new{e.AuthorId, e.ServerId});

        //Todo: Critical, doesnt allow dms
        builder.Property(e => e.ServerId).IsRequired();
        builder.HasOne(e => e.Server)
            .WithMany()
            .HasForeignKey(e => e.ServerId);

        builder.Property(e => e.ChannelId).IsRequired();
        builder.HasOne(e => e.Channel)
            .WithMany(e=>e.Messages)
            .HasForeignKey(e => new { e.ChannelId, e.ServerId });

        builder.Property(e => e.Content)
            .IsRequired();
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