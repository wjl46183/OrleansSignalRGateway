using Orleans;

namespace OrleansSignalR.Interfaces;

public interface IPlayerGrain : IGrainWithStringKey
{
    Task<string> SayHello(GameMessage message);
    Task UpdatePosition(PositionUpdate update);
    Task<PlayerStatusResponse> GetStatus();
}
