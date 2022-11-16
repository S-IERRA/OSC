﻿using ChatServer.Objects;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatServer.Handlers;

public class Server : IEntityTypeConfiguration<Server>
{
    public int Id { get; set; }
    private int _ownerId;
    
    public required User   Owner { get; set; }
    public required string Name  { get; set; }
    
    public string?   Description  { get; set; }
    public string?   Icon         { get; set; }
    public string?   Banner       { get; set; }
    public string[]  InviteCodes { get; set; } = Array.Empty<string>();
    
    public DateTime Created { get; set; } = DateTime.UtcNow;

    public virtual List<Member>  Members  { get; set; }
    public virtual List<Role>    Roles    { get; set; }
    public virtual List<Channel> Channels { get; set; }

    public void Configure(EntityTypeBuilder<Server> builder)
    {
        builder.Property(e => e.Id).ValueGeneratedOnAdd();
        builder.Property(e => e.Name).IsRequired();
        builder.Property(e => e.Owner).IsRequired();
        builder.Property(e => e.InviteCodes).HasDefaultValue(Array.Empty<string>());
        
        builder.HasOne(e => e.Owner)
            .WithMany()
            .HasForeignKey(e => e._ownerId)
            .IsRequired();
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

[Flags]
public enum Permissions
{
    Member,
    Administrator,
    CanKick,
    CanBan,
    CanMute,
}