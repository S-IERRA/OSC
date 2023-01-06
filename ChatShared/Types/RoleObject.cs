using System.ComponentModel.DataAnnotations.Schema;

namespace ChatShared.Types;

public class RoleShared
{
    public int Id { get; set; }
    
    [NotMapped] public int userId;
    [NotMapped] public int serverId;
    
    public required ServerShared Server { get; set; }
    public MemberShared   User   { get; set; }

    public required string Name   { get; set; }
    public required int    Color  { get; set; } //Hex
    
    public required Permissions Permissions { get; set; }
}