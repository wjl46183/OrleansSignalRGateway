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

    public Task<string> SayHello(string greeting)
    {
        _lastGreeting = greeting;
        _logger.LogInformation($"Player {this.GetPrimaryKeyString()} received greeting: {greeting}");
        return Task.FromResult($"Hello from Grain! You said: {greeting}");
    }

    public Task UpdatePosition(float x, float y, float z)
    {
        _position = (x, y, z);
        _logger.LogInformation($"Player {this.GetPrimaryKeyString()} moved to ({x}, {y}, {z})");
        return Task.CompletedTask;
    }

    public Task<string> GetStatus()
    {
        return Task.FromResult($"Last Greeting: {_lastGreeting}, Position: ({_position.x}, {_position.y}, {_position.z})");
    }
}
