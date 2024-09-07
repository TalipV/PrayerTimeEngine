using System.Net.WebSockets;

namespace PrayerTimeEngine.Core.Data.WebSocket.Interfaces;

public interface IWebSocketClient : IDisposable
{
    Task ConnectAsync(Uri uri, CancellationToken cancellationToken);
    Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken);
    Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken);
    WebSocketState State { get; }
}
