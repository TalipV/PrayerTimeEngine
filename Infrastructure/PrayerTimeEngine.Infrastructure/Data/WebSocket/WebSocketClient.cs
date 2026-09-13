using PrayerTimeEngine.Core.Data.WebSocket.Interfaces;
using System.Net.WebSockets;

namespace PrayerTimeEngine.Core.Data.WebSocket;

public class WebSocketClient(
        ClientWebSocket client
    ) : IWebSocketClient
{
    public Task ConnectAsync(Uri uri, CancellationToken cancellationToken)
    {
        return client.ConnectAsync(uri, cancellationToken);
    }

    public Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
    {
        return client.SendAsync(buffer, messageType, endOfMessage, cancellationToken);
    }

    public Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
    {
        return client.ReceiveAsync(buffer, cancellationToken);
    }

    public WebSocketState State => client.State;

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        client.Dispose();
    }
}
