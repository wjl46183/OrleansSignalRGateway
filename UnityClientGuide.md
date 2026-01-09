# Unity 客户端连接指南 (针对微信小游戏 - MemoryPack 二进制版)

在 Unity 中连接到基于 Orleans 和 SignalR 的网关，并使用 MemoryPack 进行高性能二进制序列化。

## 1. 为什么使用二进制传输？

由于目前 SignalR 官方尚未内置 MemoryPack 协议支持，我们采用 **"SignalR + Byte Array + MemoryPack"** 的方案。这种方案：
- **兼容性最强**: 绕过了复杂的 HubProtocol 实现。
- **性能极高**: 核心数据依然通过 MemoryPack 序列化。
- **易于调试**: 可以在同一 Hub 中混合使用 JSON 和二进制方法。

## 2. Unity 代码示例 (C#)

```csharp
using Microsoft.AspNetCore.SignalR.Client;
using UnityEngine;
using MemoryPack;
using System.Threading.Tasks;

public class GameClient : MonoBehaviour
{
    private HubConnection _connection;

    async void Start()
    {
        string hubUrl = "http://your-gateway-ip:5000/gamehub";

        _connection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()
            .Build();

        await _connection.StartAsync();
        Debug.Log("已连接到网关");

        // 1. 准备数据对象
        var msg = new GameMessage { PlayerId = "Player_123", Content = "Hello from Unity!" };

        // 2. 使用 MemoryPack 序列化为字节数组
        byte[] bin = MemoryPackSerializer.Serialize(msg);

        // 3. 通过 SignalR 发送二进制数据，并接收二进制返回
        byte[] resultBin = await _connection.InvokeAsync<byte[]>("SendMessageBinary", bin);

        // 4. 反序列化结果
        string result = MemoryPackSerializer.Deserialize<string>(resultBin);
        Debug.Log($"服务器返回: {result}");
    }

    public async void SendPosition(float x, float y, float z)
    {
        if (_connection.State == HubConnectionState.Connected)
        {
            var update = new PositionUpdate { PlayerId = "Player_123", X = x, Y = y, Z = z };
            byte[] bin = MemoryPackSerializer.Serialize(update);
            
            // 使用 SendAsync 发送不等待返回的二进制数据
            await _connection.SendAsync("UpdatePositionBinary", bin);
        }
    }
}
```

## 3. 微信小游戏适配

在微信小游戏环境中，确保您的 WebSocket 适配器支持二进制（ArrayBuffer）传输。

1.  **MemoryPack JS**: 如果您在微信小游戏侧使用 JavaScript，可以使用 MemoryPack 的 TypeScript 生成功能。
2.  **二进制处理**: 微信小游戏的 `wx.sendSocketMessage` 接受 `ArrayBuffer`，这与 SignalR 的二进制帧是兼容的。

## 4. 网关端实现说明

网关 Hub 方法接收 `byte[]`，内部调用 `MemoryPackSerializer.Deserialize<T>(data)` 还原对象，处理完成后再通过 `MemoryPackSerializer.Serialize(result)` 返回。

```csharp
public async Task<byte[]> SendMessageBinary(byte[] data)
{
    var message = MemoryPackSerializer.Deserialize<GameMessage>(data);
    var grain = _client.GetGrain<IPlayerGrain>(message.PlayerId);
    var result = await grain.SayHello(message);
    return MemoryPackSerializer.Serialize(result);
}
```
