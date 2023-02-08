using ChatShared.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using System.ComponentModel.DataAnnotations.Schema;

namespace ChatServer.Handlers;

public class Server : ServerShared, IEntityTypeConfiguration<ServerShared>
{
	public ICollection<ServerSubscriber> Subscribers { get; set; } = new HashSet<ServerSubscriber>();

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

public class ServerSubscriber : IEntityTypeConfiguration<ServerSubscriber>
{
    public Guid Id { get; set; }

    public Server Server { get; set; }
	public Guid ServerId { get; set; }

	public required byte[] AddressBytes { get; set; }
	public required int Port { get; set; }

    public void Configure(EntityTypeBuilder<ServerSubscriber> builder)
    {
        builder.HasKey(e => e.Id);

        builder.HasOne(e => e.Server)
            .WithMany(e => e.Subscribers)
            .HasForeignKey(e => e.ServerId);
    }
}