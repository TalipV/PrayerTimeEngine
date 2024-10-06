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

public partial class MyMosqApiService(
        IWebSocketClientFactory webSocketClientFactory
    ) : IMyMosqApiService
{
    [GeneratedRegex(@"[^\s""]+\.firebasedatabase\.app")]
    private static partial Regex getWebSocketBaseURLExtractionRegex();

    private const string WEBSOCKET_DEFAULT_BASE_URL = "s-euw1b-nss-209.europe-west1.firebasedatabase.app";

    private const string WEBSOCKET_URL_TEMPLATE = "wss://{0}/.ws?v=5&p=1:734647730920:web:7632560710973b246b3f5d&ns=takvim-ch-default-rtdb";

    private const string EXTERNAL_ID_PLACEHOLDER_KEY = "EXTERNAL_ID_PLACEHOLDER";
    private static readonly string INITIAL_MESSAGE_TEMPLATE = $$"""
        {
            "t": "d",
            "d": {
                "r": 2,
                "a": "g",
                "b": {
                    "p": "/prayerTimes/{{EXTERNAL_ID_PLACEHOLDER_KEY}}"
                }
            }
        }
        """;

    public async Task<List<MyMosqPrayerTimesDTO>> GetPrayerTimesAsync(LocalDate date, string externalID, CancellationToken cancellationToken)
    {
        string finaleMessage = string.Join(
            string.Empty, 
            await readResponseMessages(externalID, cancellationToken).Where(x => x.Contains("Asr")).ToListAsync(cancellationToken));
        
        finaleMessage = JsonDocument.Parse(finaleMessage).RootElement.GetProperty("d").GetProperty("b").GetProperty("d").ToString();

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

    public async Task<bool> ValidateData(
        string externalID, 
        CancellationToken cancellationToken)
    {
        List<string> responseMessages = await readResponseMessages(externalID, cancellationToken).ToListAsync(cancellationToken);

        // invalid pages send normal responses but don't contain any time data
        if (responseMessages.Count == 2 && responseMessages[1].EndsWith("\"d\":null}}}"))
        {
            return false;
        }

        return true;
    }

    private async IAsyncEnumerable<string> readResponseMessages(
        string externalID, 
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        string initialMessage = INITIAL_MESSAGE_TEMPLATE.Replace(EXTERNAL_ID_PLACEHOLDER_KEY, externalID);
        string baseUrl = await getCorrectWebSocketURLAsync(initialMessage, cancellationToken);

        using IWebSocketClient webSocketClient = webSocketClientFactory.CreateWebSocketClient();
        await webSocketClient.ConnectAsync(new Uri(string.Format(WEBSOCKET_URL_TEMPLATE, baseUrl)), cancellationToken);

        var bytesToSend = Encoding.UTF8.GetBytes(initialMessage);
        var arraySegment = new ArraySegment<byte>(bytesToSend);
        await webSocketClient.SendAsync(arraySegment, WebSocketMessageType.Text, true, cancellationToken);

        byte[] buffer = new byte[1024];
        while (webSocketClient.State == WebSocketState.Open)
        {
            WebSocketReceiveResult result;
            do
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    CancellationToken token = new CancellationTokenSource(1000).Token;
                    result = await webSocketClient.ReceiveAsync(new ArraySegment<byte>(buffer), token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                yield return Encoding.UTF8.GetString(buffer, 0, result.Count);
            }
            while (!result.EndOfMessage);
        }
    }

    private async Task<string> getCorrectWebSocketURLAsync(string initialMessage, CancellationToken cancellationToken)
    {
        string defaultUrl = string.Format(WEBSOCKET_URL_TEMPLATE, WEBSOCKET_DEFAULT_BASE_URL);

        using IWebSocketClient webSocketClient = webSocketClientFactory.CreateWebSocketClient();
        await webSocketClient.ConnectAsync(new Uri(defaultUrl), cancellationToken);

        var bytesToSend = Encoding.UTF8.GetBytes(initialMessage);
        var arraySegment = new ArraySegment<byte>(bytesToSend);
        await webSocketClient.SendAsync(arraySegment, WebSocketMessageType.Text, true, cancellationToken);

        byte[] buffer = new byte[1024];
        WebSocketReceiveResult result;

        cancellationToken.ThrowIfCancellationRequested();
        result = await webSocketClient.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();

        string textValue = Encoding.UTF8.GetString(buffer, 0, result.Count);
        var regex = getWebSocketBaseURLExtractionRegex();
        Match match = regex.Match(textValue);

        if (!match.Success)
        {
            throw new Exception("Base URL could not be determined!");
        }

        return match.Value;
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
