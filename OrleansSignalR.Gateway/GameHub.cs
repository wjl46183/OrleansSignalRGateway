using Microsoft.AspNetCore.SignalR;
using OrleansSignalR.Interfaces;
using MemoryPack;

namespace OrleansSignalR.Gateway;

public class GameHub : Hub
{
    private readonly IClusterClient _client;
    private readonly ILogger<GameHub> _logger;

    public GameHub(IClusterClient client, ILogger<GameHub> logger)
    {
        _client = client;
        _logger = logger;
    }

    // 客户端发送字节数组，服务端反序列化为 MemoryPack 对象
    public async Task<byte[]> SendMessageBinary(byte[] data)
    {
        var message = MemoryPackSerializer.Deserialize<GameMessage>(data);
        if (message == null) throw new HubException("Invalid message data");

        _logger.LogInformation($"Hub received binary message from {message.PlayerId}: {message.Content}");
        
        var grain = _client.GetGrain<IPlayerGrain>(message.PlayerId);
        var result = await grain.SayHello(message);
        
        // 返回结果也序列化为字节数组（如果需要）
        return MemoryPackSerializer.Serialize(result);
    }

    public async Task UpdatePositionBinary(byte[] data)
    {
        var update = MemoryPackSerializer.Deserialize<PositionUpdate>(data);
        if (update == null) throw new HubException("Invalid position data");

        var grain = _client.GetGrain<IPlayerGrain>(update.PlayerId);
        await grain.UpdatePosition(update);
    }

    public async Task<byte[]> GetPlayerStatusBinary(string playerId)
    {
        var grain = _client.GetGrain<IPlayerGrain>(playerId);
        var status = await grain.GetStatus();
        return MemoryPackSerializer.Serialize(status);
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation($"Client connected: {Context.ConnectionId}");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation($"Client disconnected: {Context.ConnectionId}");
        await base.OnDisconnectedAsync(exception);
    }
}
