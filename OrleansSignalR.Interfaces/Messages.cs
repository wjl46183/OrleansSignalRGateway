using MemoryPack;

namespace OrleansSignalR.Interfaces;

[MemoryPackable]
public partial class GameMessage
{
    public string PlayerId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

[MemoryPackable]
public partial class PositionUpdate
{
    public string PlayerId { get; set; } = string.Empty;
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
}

[MemoryPackable]
public partial class PlayerStatusResponse
{
    public string Status { get; set; } = string.Empty;
}
