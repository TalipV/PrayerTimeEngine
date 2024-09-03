using Microsoft.Extensions.DependencyInjection;
using PrayerTimeEngine.Core.Data.WebSocket.Interfaces;

namespace PrayerTimeEngine.Core.Data.WebSocket
{
    public class WebSocketClientFactory(
            IServiceProvider serviceProvider
        ) : IWebSocketClientFactory
    {
        public IWebSocketClient CreateWebSocketClient()
        {
            return serviceProvider.GetService<IWebSocketClient>();
        }
    }
}
