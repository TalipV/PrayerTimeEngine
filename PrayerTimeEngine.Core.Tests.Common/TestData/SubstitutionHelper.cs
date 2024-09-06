using NSubstitute;
using PrayerTimeEngine.Core.Data.WebSocket.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Mosques.Mawaqit.Services;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimeProviders.Fazilet.Interfaces;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimeProviders.Mosques.Mawaqit.Interfaces;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimeProviders.Muwaqqit.Interfaces;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimeProviders.Semerkand.Interfaces;
using Refit;
using System.Net;
using System.Net.WebSockets;
using System.Text;

namespace PrayerTimeEngine.Core.Tests.Common.TestData
{
    public class SubstitutionHelper
    {
        public static ISemerkandApiService GetMockedSemerkandApiService()
        {
            static HttpResponseMessage handleRequestFunc(HttpRequestMessage request)
            {
                Stream responseStream;

                if (request.RequestUri.AbsoluteUri == "https://semerkandtakvimi.semerkandmobile.com/countries?language=tr")
                    responseStream = File.OpenRead(Path.Combine(TestDataHelper.SEMERKAND_TEST_DATA_FILE_PATH, "Semerkand_TestCountriesData.txt"));
                else if (request.RequestUri.AbsoluteUri == "https://semerkandtakvimi.semerkandmobile.com/cities?countryID=3")
                    responseStream = File.OpenRead(Path.Combine(TestDataHelper.SEMERKAND_TEST_DATA_FILE_PATH, "Semerkand_TestCityData_Austria.txt"));
                else if (request.RequestUri.AbsoluteUri == "https://semerkandtakvimi.semerkandmobile.com/salaattimes?cityId=197&year=2023")
                    responseStream = File.OpenRead(Path.Combine(TestDataHelper.SEMERKAND_TEST_DATA_FILE_PATH, "Semerkand_TestPrayerTimeData_20230729_Innsbruck.txt"));
                else
                    throw new Exception($"No response registered for URL: {request.RequestUri.AbsoluteUri}");

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
                        @"https://www.muwaqqit.com/api2.json?d=2023-07-30&ln=11.41337&lt=47.2803835&tz=Europe%2FVienna&fa=-12&ia=3.5&isn=-8&ea=-12" => File.OpenRead(Path.Combine(TestDataHelper.MUWAQQIT_TEST_DATA_FILE_PATH, "Muwaqqit_TestPrayerTimeData_20230730_Innsbruck_Config1.txt")),
                        @"https://www.muwaqqit.com/api2.json?d=2023-07-30&ln=11.41337&lt=47.2803835&tz=Europe%2FVienna&fa=-7.5&ia=4.5&isn=-12&ea=-15.5" => File.OpenRead(Path.Combine(TestDataHelper.MUWAQQIT_TEST_DATA_FILE_PATH, "Muwaqqit_TestPrayerTimeData_20230730_Innsbruck_Config2.txt")),
                        @"https://www.muwaqqit.com/api2.json?d=2023-07-30&ln=11.41337&lt=47.2803835&tz=Europe%2FVienna&fa=-4.5&ia=-12&isn=-12&ea=-12" => File.OpenRead(Path.Combine(TestDataHelper.MUWAQQIT_TEST_DATA_FILE_PATH, "Muwaqqit_TestPrayerTimeData_20230730_Innsbruck_Config3.txt")),
                        @"https://www.muwaqqit.com/api2.json?d=2023-07-30&ln=11.41337&lt=47.2803835&tz=Europe%2FVienna&fa=-15&ia=-12&isn=-12&ea=-12" => File.OpenRead(Path.Combine(TestDataHelper.MUWAQQIT_TEST_DATA_FILE_PATH, "Muwaqqit_TestPrayerTimeData_20230730_Innsbruck_Config4.txt")),
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
                BaseAddress = new Uri(@"https://www.muwaqqit.com/")
            };

            return RestService.For<IMuwaqqitApiService>(httpClient);
        }

        public static IFaziletApiService GetMockedFaziletApiService()
        {
            string baseURL = @"https://fazilettakvimi.com/api/cms";

            HttpResponseMessage handleRequestFunc(HttpRequestMessage request)
            {
                Stream responseStream;

                if (request.RequestUri.AbsoluteUri == $@"{baseURL}/daily?districtId=232&lang=1")
                    responseStream = File.OpenRead(Path.Combine(TestDataHelper.FAZILET_TEST_DATA_FILE_PATH, "Fazilet_TestCountriesData.txt"));
                else if (request.RequestUri.AbsoluteUri == $@"{baseURL}/cities-by-country?districtId=2")
                    responseStream = File.OpenRead(Path.Combine(TestDataHelper.FAZILET_TEST_DATA_FILE_PATH, "Fazilet_TestCityData_Austria.txt"));
                else if (request.RequestUri.AbsoluteUri == $@"{baseURL}/daily?districtId=92&lang=2")
                    responseStream = File.OpenRead(Path.Combine(TestDataHelper.FAZILET_TEST_DATA_FILE_PATH, "Fazilet_TestPrayerTimeData_20230729_Innsbruck.txt"));
                else
                    throw new Exception($"No response registered for URL: {request.RequestUri.AbsoluteUri}");

                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StreamContent(responseStream)
                };
            }

            var mockHttpMessageHandler = new MockHttpMessageHandler(handleRequestFunc);
            var httpClient = new HttpClient(mockHttpMessageHandler) { BaseAddress = new Uri(baseURL) };

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

        public static IWebSocketClient GetMockedMyMosqWebSocketClient()
        {
            var mockWebSocketClient = Substitute.For<IWebSocketClient>();

            var filePath = Path.Combine(TestDataHelper.MYMOSQ_TEST_DATA_FILE_PATH, "MyMosq_WebSocketMessage_20240830_1239.txt");
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

            mockWebSocketClient.State.Returns(WebSocketState.Open);

            return mockWebSocketClient;
        }

    }
}
