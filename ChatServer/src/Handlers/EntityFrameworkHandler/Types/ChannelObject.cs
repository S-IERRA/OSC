using ChatServer.Objects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatServer.Handlers;

public class Channel : IEntityTypeConfiguration<Channel>
{
    public int Id { get; set; }

    public required string Name { get; set; }
    public required Permissions ViewPermission { get; set; }
    
    public required Server Server { get; set; }
    
    public virtual List<Message> Messages { get; set; }

    public void Configure(EntityTypeBuilder<Channel> builder)
    {
        builder.Property(e => e.Id).ValueGeneratedOnAdd();
        
        builder.Property(e => e.Name).IsRequired();
        builder.Property(e => e.Server).IsRequired();
        builder.Property(e => e.ViewPermission).IsRequired();
        
        builder.HasOne(e => e.Server)
            .WithMany(e => e.Channels)
            .HasForeignKey(e => e.Id)
            .IsRequired();
    }
}