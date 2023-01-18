using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using ChatShared.Types;
using Google.Protobuf.WellKnownTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatServer.Handlers;

/*
public class User : UserShared, IEntityTypeConfiguration<UserShared>
{
    public void Configure(EntityTypeBuilder<UserShared> builder)
    {
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.HasIndex(e => e.Session);
        
        builder.Property(e => e.Username).IsRequired();
        builder.Property(e => e.Password).IsRequired();
        builder.Property(e => e.Email).IsRequired();

        builder.Property(e => e.Status).HasDefaultValue(Status.Offline);
    }
}*/

public class User : UserShared
{
    
}