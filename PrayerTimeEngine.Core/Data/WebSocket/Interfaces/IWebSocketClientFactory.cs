namespace PrayerTimeEngine.Core.Data.WebSocket.Interfaces
{
    public interface IWebSocketClientFactory
    {
        IWebSocketClient CreateWebSocketClient();
    }
}
