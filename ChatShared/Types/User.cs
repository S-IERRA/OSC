using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ChatShared.Types;

public class UserShared
{
    public Guid Id { get; set; }
    
    public required string Username { get; set; }
    [JsonIgnore] public string Password { get; set; }
    public required string Email    { get; set; }
    public string? Session { get; set; }
    
    public string? Icon { get; set; }
    public string? Bio  { get; set; }
    
    public Status Status { get; set; }
    
    public DateTime LastOnline { get; set; } = DateTime.UtcNow;
    public DateTime Created    { get; set; } = DateTime.UtcNow;

    public virtual ICollection<ServerShared> Servers { get; set; } = new HashSet<ServerShared>();
}
public enum Status
{
    Online,
    Offline,
}