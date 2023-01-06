namespace ChatShared.Types;

public class ChannelShared
{
    public int Id { get; set; }

    public required string Name { get; set; }
    public required Permissions ViewPermission { get; set; }
    
    public required ServerShared Server { get; set; }
    
    public virtual List<MessageShared> Messages { get; set; }
}