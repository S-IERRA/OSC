using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatServer.Handlers;

public class Message : IEntityTypeConfiguration<Message>
{
    public int Id { get; set; }
    
    public required User    Author  { get; set; }
    public required Server  Server  { get; set; }
    public required Channel Channel { get; set; }
    public required string  Content { get; set; }
    
    public DateTime Sent { get; set; } = DateTime.UtcNow;
    
    public void Configure(EntityTypeBuilder<Message> builder)
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