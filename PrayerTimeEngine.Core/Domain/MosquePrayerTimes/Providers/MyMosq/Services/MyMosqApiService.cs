using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using NodaTime;
using PrayerTimeEngine.Core.Data.WebSocket.Interfaces;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.MyMosq.Interfaces;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.MyMosq.Models.DTOs;

namespace PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.MyMosq.Services
{
    public class MyMosqApiService(
            IWebSocketClientFactory webSocketClientFactory
        ) : IMyMosqApiService
    {
        private const string URL =
            "wss://s-euw1b-nss-200.europe-west1.firebasedatabase.app/.ws?v=5&p=1:734647730920:web:7632560710973b246b3f5d&ns=takvim-ch-default-rtdb";

        public async Task<List<MyMosqPrayerTimesDTO>> GetPrayerTimesAsync(LocalDate date, string externalID, CancellationToken cancellationToken)
        {
            using IWebSocketClient _webSocketClient = webSocketClientFactory.CreateWebSocketClient();
            await _webSocketClient.ConnectAsync(new Uri(URL), cancellationToken);

            string message = $$"""
            {
            	"t": "d",
            	"d": {
            		"r": 2,
            		"a": "g",
            		"b": {
            			"p": "/prayerTimes/{{externalID}}"
            		}
            	}
            }
            """;

            var bytesToSend = Encoding.UTF8.GetBytes(message);
            await _webSocketClient.SendAsync(new ArraySegment<byte>(bytesToSend), WebSocketMessageType.Text, true, cancellationToken);

            byte[] buffer = new byte[1024];
            string finaleMessage = "";

            while (_webSocketClient.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result;
                do
                {
                    CancellationToken token = new CancellationTokenSource(1000).Token;
                    try
                    {
                        result = await _webSocketClient.ReceiveAsync(new ArraySegment<byte>(buffer), token);
                    }
                    catch { break; }

                    string currentMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    if (currentMessage.Contains("Asr"))
                    {
                        finaleMessage +=
                            currentMessage
                            .Replace("""{"t":"d","d":{"r":2,"b":{"s":"ok","d":""", "")
                            .Replace("}}}}}", "}}")
                            ;
                    }
                }
                while (!result.EndOfMessage);
            }

            finaleMessage =
                Regex.Replace(finaleMessage,
                    pattern: $$$"""
                                "{{{date.Year}}}[0-9][0-9][0-9][0-9]":{
                                """,
                    replacement: "{")
                .Replace(
                    oldValue:
                        """
                        {{"Asr"
                        """,
                    newValue:
                        """
                        {"prayerTimes":[{"Asr"
                        """)
                .Replace("}}", "}]}");

            return JsonSerializer.Deserialize<MyMosqResponseDTO>(json: finaleMessage).PrayerTimes;
        }
    }
}
