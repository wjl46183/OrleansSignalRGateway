using Microsoft.AspNetCore.SignalR;
using OrleansSignalR.Interfaces;

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

    public async Task<string> SendMessage(string playerId, string message)
    {
        _logger.LogInformation($"Hub received message from {playerId}: {message}");
        var grain = _client.GetGrain<IPlayerGrain>(playerId);
        return await grain.SayHello(message);
    }

    public async Task UpdatePosition(string playerId, float x, float y, float z)
    {
        var grain = _client.GetGrain<IPlayerGrain>(playerId);
        await grain.UpdatePosition(x, y, z);
    }

    public async Task<string> GetPlayerStatus(string playerId)
    {
        var grain = _client.GetGrain<IPlayerGrain>(playerId);
        return await grain.GetStatus();
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
