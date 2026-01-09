# Unity 客户端连接指南 (针对微信小游戏)

在 Unity 中连接到基于 Orleans 和 SignalR 的网关，需要考虑微信小游戏的特殊网络环境。

## 1. 方案选择

### 方案 A: 使用 SignalR C# 客户端 (推荐用于开发和非微信平台)
使用 `Microsoft.AspNetCore.SignalR.Client` NuGet 包。但在发布到微信小游戏时，由于微信小游戏的 WebSocket API 不同，可能需要适配。

### 方案 B: 使用微信小游戏专用 SignalR 适配器 (推荐用于微信小游戏)
微信小游戏环境推荐使用 JavaScript 版本的 SignalR 客户端，并通过 Unity 的 `JSB` (JavaScript Bridge) 进行调用。

## 2. 微信小游戏适配要点

微信小游戏不支持标准的浏览器 WebSocket，需要使用 `wx.connectSocket`。
对于 SignalR，可以使用开源的适配器，例如 [signalr-wxapp](https://github.com/m-Ryan/signalr-wxapp) 或者在 Unity 微信小游戏 SDK 中提供的网络适配层。

## 3. Unity 代码示例 (C#)

假设您使用的是支持微信环境的 SignalR 客户端插件：

```csharp
using Microsoft.AspNetCore.SignalR.Client;
using UnityEngine;
using System.Threading.Tasks;

public class GameClient : MonoBehaviour
{
    private HubConnection _connection;

    async void Start()
    {
        // 网关地址
        string hubUrl = "http://your-gateway-ip:port/gamehub";

        _connection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()
            .Build();

        _connection.On<string>("ReceiveMessage", (message) =>
        {
            Debug.Log($"收到服务器消息: {message}");
        });

        try
        {
            await _connection.StartAsync();
            Debug.Log("已连接到网关");

            // 调用网关方法，网关会调度到 Orleans Grain
            string result = await _connection.InvokeAsync<string>("SendMessage", "Player_123", "Hello Manus!");
            Debug.Log($"Grain 返回: {result}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"连接失败: {ex.Message}");
        }
    }

    public async void SendPosition(float x, float y, float z)
    {
        if (_connection.State == HubConnectionState.Connected)
        {
            await _connection.SendAsync("UpdatePosition", "Player_123", x, y, z);
        }
    }
}
```

## 4. 微信小游戏配置

1.  **域名白名单**: 在微信公众平台后台，将您的网关域名添加到 `socket 合法域名` 中。
2.  **协议**: 生产环境必须使用 `wss://`。
3.  **适配层**: 如果使用 Unity 官方的微信小游戏转换工具，确保在转换设置中开启了网络适配。

## 5. 网关调度逻辑说明

在我们的实现中，`GameHub` 充当了中转站：
1.  客户端通过 SignalR 调用 `GameHub.SendMessage(playerId, message)`。
2.  `GameHub` 内部通过 `IClusterClient` 获取 `IPlayerGrain` 的引用：`_client.GetGrain<IPlayerGrain>(playerId)`。
3.  `GameHub` 调用 Grain 的方法并等待结果。
4.  `GameHub` 将结果返回给客户端。
