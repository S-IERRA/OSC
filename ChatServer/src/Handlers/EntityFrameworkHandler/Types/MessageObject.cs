using System.Text.Json.Serialization;
using ChatShared.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatServer.Handlers;

public class Message : MessageShared, IEntityTypeConfiguration<MessageShared>
{
    public void Configure(EntityTypeBuilder<MessageShared> builder)
    {
        builder.Property(e => e.Id).ValueGeneratedOnAdd();
        
        builder.Property(e => e.Author).IsRequired();
        builder.Property(e => e.Server).IsRequired();
        builder.Property(e => e.Channel).IsRequired();
        builder.Property(e => e.Content).IsRequired();

        builder.HasOne(e => e.Author)
            .WithMany()
            .HasForeignKey(e => e.Id)
            .IsRequired();
        
        builder.HasOne(e => e.Channel)
            .WithMany(e => e.Messages)
            .HasForeignKey(e => e.Id)
            .IsRequired();
    }
}