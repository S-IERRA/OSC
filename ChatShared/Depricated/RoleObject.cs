using System.ComponentModel.DataAnnotations.Schema;

namespace ChatShared.Types;

[NotMapped]
public class RoleShared
{
    public Guid Id { get; set; }
    
    [NotMapped] public Guid userId;
    [NotMapped] public Guid serverId;
    
    public required ServerShared Server { get; set; }
    public MemberShared   User   { get; set; }

    public required string Name   { get; set; }
    public required int    Color  { get; set; } //Hex
    
    public required Permissions Permissions { get; set; }
}