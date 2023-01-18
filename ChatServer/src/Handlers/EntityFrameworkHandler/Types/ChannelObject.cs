using ChatServer.Objects;
using ChatShared.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatServer.Handlers;

/*
public class Channel : ChannelShared, IEntityTypeConfiguration<ChannelShared>
{
    public void Configure(EntityTypeBuilder<ChannelShared> builder)
    {
        builder.Property(e => e.Id).ValueGeneratedOnAdd();
        
        builder.Property(e => e.Name).IsRequired();
        builder.Property(e => e.ServerId).IsRequired();
        builder.Property(e => e.ViewPermission).IsRequired();
        
        builder.HasOne(e => e.Server)
            .WithMany(e => e.Channels)
            .HasForeignKey(e => e.ServerId) // And that
            .IsRequired();
    }
}
*/

public class Channel : ChannelShared
{
    
}