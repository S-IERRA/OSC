using System.ComponentModel.DataAnnotations.Schema;
using Google.Protobuf.WellKnownTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatServer.Handlers;

public class User : IEntityTypeConfiguration<User>
{
    [NotMapped]
    private static readonly Timestamp Now = Timestamp.FromDateTime(DateTime.UtcNow);
    
    public int Id { get; set; }
    
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required string Email    { get; set; }
    public string? Session { get; set; }
    
    public string? Icon { get; set; }
    public string? Bio  { get; set; }
    
    public Status Status { get; set; }
    
    public DateTime LastOnline { get; set; } = DateTime.UtcNow;
    public DateTime Created    { get; set; } = DateTime.UtcNow;
    
    public virtual List<Server> Servers { get; set; }

    public static implicit operator int(User user) 
        => user.Id;

    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(e => e.Id).ValueGeneratedOnAdd();
        
        builder.Property(e => e.Username).IsRequired();
        builder.Property(e => e.Password).IsRequired();
        builder.Property(e => e.Email).IsRequired();

        builder.Property(e => e.Status).HasDefaultValue(Status.Offline);
    }
}

public enum Status
{
    Online,
    Offline,
}