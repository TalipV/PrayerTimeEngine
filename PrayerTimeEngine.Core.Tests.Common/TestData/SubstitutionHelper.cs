using NodaTime;
using NSubstitute;
using PrayerTimeEngine.Core.Common;
using PrayerTimeEngine.Core.Data.WebSocket.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Mosques.Mawaqit.Services;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Interfaces;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Muwaqqit.Interfaces;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Interfaces;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.Mawaqit.Interfaces;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.MyMosq.Services;
using Refit;
using System.Globalization;
using System.Net;
using System.Net.WebSockets;
using System.Text;

namespace PrayerTimeEngine.Core.Tests.Common.TestData;

public class SubstitutionHelper
{
    private const string SEMERKAND_BASE_URL = @"https://semerkandtakvimi.semerkandmobile.com";
    private const string FAZILET_BASE_URL = @"https://fazilettakvimi.com/api/cms";
    private const string MUWAQQIT_BASE_URL = @"https://www.muwaqqit.com";

    public static ISemerkandApiService GetMockedSemerkandApiService()
    {
        static HttpResponseMessage handleRequestFunc(HttpRequestMessage request)
        {
            Stream responseStream = request.RequestUri.AbsoluteUri switch
            {
                $"{SEMERKAND_BASE_URL}/countries?language=tr" => File.OpenRead(Path.Combine(TestDataHelper.SEMERKAND_TEST_DATA_FILE_PATH, "Semerkand_TestCountriesData.txt")),
                $"{SEMERKAND_BASE_URL}/cities?countryID=3" => File.OpenRead(Path.Combine(TestDataHelper.SEMERKAND_TEST_DATA_FILE_PATH, "Semerkand_TestCityData_Austria.txt")),
                $"{SEMERKAND_BASE_URL}/cities?countryID=2" => File.OpenRead(Path.Combine(TestDataHelper.SEMERKAND_TEST_DATA_FILE_PATH, "Semerkand_TestCityData_Germany.txt")),
                $"{SEMERKAND_BASE_URL}/salaattimes?cityId=197&year=2023" => File.OpenRead(Path.Combine(TestDataHelper.SEMERKAND_TEST_DATA_FILE_PATH, "Semerkand_TestPrayerTimeData_20230729_Innsbruck.txt")),
                $"{SEMERKAND_BASE_URL}/salaattimes?cityId=786&year=2025" => File.OpenRead(Path.Combine(TestDataHelper.SEMERKAND_TEST_DATA_FILE_PATH, "Semerkand_TestPrayerTimeData_20250330_Leverkusen.txt")),
                _ => throw new Exception($"No response registered for URL: {request.RequestUri.AbsoluteUri}"),
            };

            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StreamContent(responseStream)
            };
        }

        var mockHttpMessageHandler = new MockHttpMessageHandler(handleRequestFunc);
        var httpClient = new HttpClient(mockHttpMessageHandler) { BaseAddress = new Uri("https://semerkandtakvimi.semerkandmobile.com/") };

        return RestService.For<ISemerkandApiService>(httpClient);
    }

    public static IMuwaqqitApiService GetMockedMuwaqqitApiService()
    {
        static HttpResponseMessage handleRequestFunc(HttpRequestMessage request)
        {
            Stream responseStream =
                request.RequestUri.AbsoluteUri switch
                {
                    $@"{MUWAQQIT_BASE_URL}/api2.json?d=2023-07-30&ln=11.41337&lt=47.2803835&tz=Europe%2FVienna&fa=-12&ia=3.5&isn=-8&ea=-12" => File.OpenRead(Path.Combine(TestDataHelper.MUWAQQIT_TEST_DATA_FILE_PATH, "Muwaqqit_TestPrayerTimeData_20230730_Innsbruck_Config1.txt")),
                    $@"{MUWAQQIT_BASE_URL}/api2.json?d=2023-07-30&ln=11.41337&lt=47.2803835&tz=Europe%2FVienna&fa=-7.5&ia=4.5&isn=-12&ea=-15.5" => File.OpenRead(Path.Combine(TestDataHelper.MUWAQQIT_TEST_DATA_FILE_PATH, "Muwaqqit_TestPrayerTimeData_20230730_Innsbruck_Config2.txt")),
                    $@"{MUWAQQIT_BASE_URL}/api2.json?d=2023-07-30&ln=11.41337&lt=47.2803835&tz=Europe%2FVienna&fa=-4.5&ia=-12&isn=-12&ea=-12" => File.OpenRead(Path.Combine(TestDataHelper.MUWAQQIT_TEST_DATA_FILE_PATH, "Muwaqqit_TestPrayerTimeData_20230730_Innsbruck_Config3.txt")),
                    $@"{MUWAQQIT_BASE_URL}/api2.json?d=2023-07-30&ln=11.41337&lt=47.2803835&tz=Europe%2FVienna&fa=-15&ia=-12&isn=-12&ea=-12" => File.OpenRead(Path.Combine(TestDataHelper.MUWAQQIT_TEST_DATA_FILE_PATH, "Muwaqqit_TestPrayerTimeData_20230730_Innsbruck_Config4.txt")),
                    _ => throw new Exception($"No response registered for URL: {request.RequestUri.AbsoluteUri}")
                };

            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StreamContent(responseStream)
            };
        }

        var mockHttpMessageHandler = new MockHttpMessageHandler(handleRequestFunc);
        var httpClient = new HttpClient(mockHttpMessageHandler)
        {
            BaseAddress = new Uri(MUWAQQIT_BASE_URL)
        };

        return RestService.For<IMuwaqqitApiService>(httpClient);
    }

    public static IFaziletApiService GetMockedFaziletApiService()
    {
        static HttpResponseMessage handleRequestFunc(HttpRequestMessage request)
        {
            Stream responseStream = request.RequestUri.AbsoluteUri switch
            {
                $@"{FAZILET_BASE_URL}/daily?districtId=232&lang=1" => File.OpenRead(Path.Combine(TestDataHelper.FAZILET_TEST_DATA_FILE_PATH, "Fazilet_TestCountriesData.txt")),
                $@"{FAZILET_BASE_URL}/cities-by-country?districtId=2" => File.OpenRead(Path.Combine(TestDataHelper.FAZILET_TEST_DATA_FILE_PATH, "Fazilet_TestCityData_Austria.txt")),
                $@"{FAZILET_BASE_URL}/cities-by-country?districtId=14" => File.OpenRead(Path.Combine(TestDataHelper.FAZILET_TEST_DATA_FILE_PATH, "Fazilet_TestCityData_Germany.txt")),
                $@"{FAZILET_BASE_URL}/daily?districtId=92&lang=2" => File.OpenRead(Path.Combine(TestDataHelper.FAZILET_TEST_DATA_FILE_PATH, "Fazilet_TestPrayerTimeData_20230729_Innsbruck.txt")),
                $@"{FAZILET_BASE_URL}/daily?districtId=2762&lang=2" => File.OpenRead(Path.Combine(TestDataHelper.FAZILET_TEST_DATA_FILE_PATH, "Fazilet_TestPrayerTimeData_20250330_Leverkusen.txt")),
                _ => throw new Exception($"No response registered for URL: {request.RequestUri.AbsoluteUri}"),
            };

            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StreamContent(responseStream)
            };
        }

        var mockHttpMessageHandler = new MockHttpMessageHandler(handleRequestFunc);
        var httpClient = new HttpClient(mockHttpMessageHandler) { BaseAddress = new Uri(FAZILET_BASE_URL) };

        return RestService.For<IFaziletApiService>(httpClient);
    }

    public static IMawaqitApiService GetMockedMawaqitApiService()
    {
        static HttpResponseMessage handleRequestFunc(HttpRequestMessage request)
        {
            Stream responseStream =
                request.RequestUri.AbsoluteUri switch
                {
                    "https://mawaqit.net/de/hamza-koln" => File.OpenRead(Path.Combine(TestDataHelper.MAWAQIT_TEST_DATA_FILE_PATH, "Mawaqit_ResponsePageContent_20240829_hamza-koln.txt")),
                    _ => throw new Exception($"No response registered for URL: {request.RequestUri.AbsoluteUri}")
                };

            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StreamContent(responseStream)
            };
        }

        var mockHttpMessageHandler = new MockHttpMessageHandler(handleRequestFunc);
        var httpClient = new HttpClient(mockHttpMessageHandler)
        {
            BaseAddress = new Uri("https://mawaqit.net/de/")
        };

        return new MawaqitApiService(httpClient);
    }

    private const string WEB_SOCKET_MESSAGE_SEPARATOR_TEXT = "#CUSTOM-SEPARATOR#";

    private static IWebSocketClient getMockedMyMosqWebSocketClient()
    {
        var mockWebSocketClient = Substitute.For<IWebSocketClient>();

        mockWebSocketClient.SendAsync(Arg.Any<ArraySegment<byte>>(), Arg.Any<WebSocketMessageType>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                ArraySegment<byte> arraySegment = callInfo.Arg<ArraySegment<byte>>();
                string inputMessage = Encoding.UTF8.GetString(arraySegment.Array, arraySegment.Offset, arraySegment.Count);

                var filePath = Path.Combine(
                    TestDataHelper.MYMOSQ_TEST_DATA_FILE_PATH,
                    inputMessage.Contains("""
                                        "p": "/prayerTimes/1239"
                                        """)
                        ? "MyMosq_WebSocketMessage_20240830_1239.txt"
                        : "MyMosq_WebSocketMessage_20240830_InvalidMosqueID.txt");
                var fileContent = File.ReadAllText(filePath);

                var messages = new Queue<string>(fileContent.Split(WEB_SOCKET_MESSAGE_SEPARATOR_TEXT));

                mockWebSocketClient.ReceiveAsync(Arg.Any<ArraySegment<byte>>(), Arg.Any<CancellationToken>())
                    .Returns(callInfo =>
                    {
                        ArraySegment<byte> arraySegment = callInfo.Arg<ArraySegment<byte>>();
                        var token = callInfo.Arg<CancellationToken>();

                        if (messages.Count == 0)
                        {
                            // unfortunately, the server suddenly stops reacting and leaves us hanging
                            // => I (currently) use this as the end of the transmission (yes.. I know)
                            Thread.Sleep(2000);

                            if (token.IsCancellationRequested)
                            {
                                mockWebSocketClient.State.Returns(WebSocketState.Aborted);
                                token.ThrowIfCancellationRequested();
                            }

                            return new WebSocketReceiveResult(count: 0, messageType: WebSocketMessageType.Text, endOfMessage: true);
                        }

                        var message = messages.Dequeue();
                        var byteArray = Encoding.UTF8.GetBytes(message);
                        Array.Copy(byteArray, arraySegment.Array, byteArray.Length);

                        return new WebSocketReceiveResult(count: byteArray.Length, messageType: WebSocketMessageType.Text, endOfMessage: true);
                    });

                return Task.CompletedTask;
            });

        mockWebSocketClient.State.Returns(WebSocketState.Open);

        return mockWebSocketClient;
    }

    public static MyMosqApiService GetMockedMyMosqApiService()
    {
        var _mockWebSocketClient = getMockedMyMosqWebSocketClient();
        var _mockWebSocketClientFactory = Substitute.For<IWebSocketClientFactory>();
        _mockWebSocketClientFactory.CreateWebSocketClient().Returns(_mockWebSocketClient);
        return new MyMosqApiService(_mockWebSocketClientFactory);
    }

    public static ISystemInfoService GetMockedSystemInfoService(ZonedDateTime zonedDateTime)
    {
        var mock = Substitute.For<ISystemInfoService>();
        mock.GetCurrentInstant().Returns(zonedDateTime.ToInstant());
        mock.GetCurrentZonedDateTime().Returns(zonedDateTime);
        mock.GetSystemTimeZone().Returns(zonedDateTime.Zone);
        mock.GetSystemCulture().Returns(CultureInfo.InvariantCulture);
        return mock;
    }
}
