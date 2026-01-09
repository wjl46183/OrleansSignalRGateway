using OrleansSignalR.Interfaces;
using Microsoft.Extensions.Logging;

namespace OrleansSignalR.Grains;

public class PlayerGrain : Grain, IPlayerGrain
{
    private readonly ILogger _logger;
    private string _lastGreeting = "None";
    private (float x, float y, float z) _position;

    public PlayerGrain(ILogger<PlayerGrain> logger)
    {
        _logger = logger;
    }

    public Task<string> SayHello(GameMessage message)
    {
        _lastGreeting = message.Content;
        _logger.LogInformation($"Player {this.GetPrimaryKeyString()} received greeting: {message.Content}");
        return Task.FromResult($"Hello from Grain! You said: {message.Content}");
    }

    public Task UpdatePosition(PositionUpdate update)
    {
        _position = (update.X, update.Y, update.Z);
        _logger.LogInformation($"Player {this.GetPrimaryKeyString()} moved to ({update.X}, {update.Y}, {update.Z})");
        return Task.CompletedTask;
    }

    public Task<PlayerStatusResponse> GetStatus()
    {
        return Task.FromResult(new PlayerStatusResponse 
        { 
            Status = $"Last Greeting: {_lastGreeting}, Position: ({_position.x}, {_position.y}, {_position.z})" 
        });
    }
}
