using Orleans;

namespace OrleansSignalR.Interfaces;

public interface IPlayerGrain : IGrainWithStringKey
{
    Task<string> SayHello(string greeting);
    Task UpdatePosition(float x, float y, float z);
    Task<string> GetStatus();
}
