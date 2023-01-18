using System.Text.Json.Serialization;
using ChatShared.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatServer.Handlers;

/*
public class Message : MessageShared, IEntityTypeConfiguration<MessageShared>
{
    public void Configure(EntityTypeBuilder<MessageShared> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.AuthorId).IsRequired();
        builder.HasOne(e => e.Author)
            .WithMany()
            .HasForeignKey(e => e.AuthorId);

        builder.Property(e => e.ServerId).IsRequired();
        builder.HasOne(e => e.Server)
            .WithMany()
            .HasForeignKey(e => e.ServerId);

        builder.Property(e => e.ChannelId).IsRequired();
        builder.HasOne(e => e.Channel)
            .WithMany()
            .HasForeignKey(e => e.ChannelId);

        builder.Property(e => e.Content).IsRequired();

        builder.Property(e => e.Sent).IsRequired();
    }
}
*/

public class Message : MessageShared
{
    
}