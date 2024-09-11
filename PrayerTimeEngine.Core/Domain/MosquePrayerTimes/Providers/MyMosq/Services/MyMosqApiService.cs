using NodaTime;
using PrayerTimeEngine.Core.Data.WebSocket.Interfaces;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.MyMosq.Interfaces;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.MyMosq.Models.DTOs;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.MyMosq.Services;

public class MyMosqApiService(
        IWebSocketClientFactory webSocketClientFactory
    ) : IMyMosqApiService
{
    private const string URL =
        "wss://s-euw1b-nss-200.europe-west1.firebasedatabase.app/.ws?v=5&p=1:734647730920:web:7632560710973b246b3f5d&ns=takvim-ch-default-rtdb";

    public async Task<List<MyMosqPrayerTimesDTO>> GetPrayerTimesAsync(LocalDate date, string externalID, CancellationToken cancellationToken)
    {
        string finaleMessage = "";
        await foreach (string currentMessage in readResponseMessages(externalID, cancellationToken))
        {
            if (currentMessage.Contains("Asr"))
            {
                finaleMessage +=
                    currentMessage
                    .Replace("""{"t":"d","d":{"r":2,"b":{"s":"ok","d":""", "")
                    .Replace("}}}}}", "}}");
            }
        }

        finaleMessage =
            Regex.Replace(finaleMessage,
                pattern: $$$"""
                            "{{{date.Year}}}[0-9][0-9][0-9][0-9]":{
                            """,
                replacement: "{")
            .Replace(
                oldValue: "{{\"Asr\"",
                newValue: "{\"prayerTimes\":[{\"Asr\"")
            .Replace("}}", "}]}");

        var prayerTimes = JsonSerializer.Deserialize<MyMosqResponseDTO>(json: finaleMessage).PrayerTimes;
        fixPrayerTimes(prayerTimes);
        return prayerTimes;
    }
    public async Task<bool> ValidateData(string externalID, CancellationToken cancellationToken)
    {
        List<string> responseMessages = await readResponseMessages(externalID, cancellationToken).ToListAsync(cancellationToken);

        // invalid pages send normal responses but don't contain any time data
        if (responseMessages.Count == 2 && responseMessages[1].EndsWith("\"d\":null}}}"))
        {
            return false;
        }

        return true;
    }

    private async IAsyncEnumerable<string> readResponseMessages(string externalID, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using IWebSocketClient webSocketClient = webSocketClientFactory.CreateWebSocketClient();
        await webSocketClient.ConnectAsync(new Uri(URL), cancellationToken);

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
        var arraySegment = new ArraySegment<byte>(bytesToSend);
        await webSocketClient.SendAsync(arraySegment, WebSocketMessageType.Text, true, cancellationToken);

        byte[] buffer = new byte[1024];
        while (webSocketClient.State == WebSocketState.Open)
        {
            WebSocketReceiveResult result;
            do
            {
                cancellationToken.ThrowIfCancellationRequested();

                CancellationToken token = new CancellationTokenSource(1000).Token;
                try
                {
                    result = await webSocketClient.ReceiveAsync(new ArraySegment<byte>(buffer), token);
                }
                catch { break; }

                yield return Encoding.UTF8.GetString(buffer, 0, result.Count);
            }
            while (!result.EndOfMessage);
        }
    }

    /// <summary>
    /// For some reason, the jumu'ah values provided by this API are always empty 
    /// and the jumuah values are found in the dhuhr values of friday. 
    /// This method makes fills all the jumu'ah values for each day with the Dhuhr value 
    /// of the next friday.. and other things. 
    /// </summary>
    private static void fixPrayerTimes(List<MyMosqPrayerTimesDTO> prayerTimes)
    {
        // replace "0:00" jumu'ah with NULL
        foreach (var day in prayerTimes)
        {
            if (day.Jumuah == new LocalTime(0, 0))
                day.Jumuah = null;

            if (day.Jumuah2 == new LocalTime(0, 0))
                day.Jumuah2 = null;
        }

        var fridays = prayerTimes
            .Where(x => x.Date.DayOfWeek == IsoDayOfWeek.Friday)
            .OrderBy(x => x.Date)
            .ToList();

        foreach (var day in prayerTimes)
        {
            MyMosqPrayerTimesDTO nextFriday = fridays.FirstOrDefault(f => f.Date > day.Date);

            if (nextFriday != null)
            {
                day.Jumuah ??= nextFriday.Dhuhr;
            }
        }
    }
}
