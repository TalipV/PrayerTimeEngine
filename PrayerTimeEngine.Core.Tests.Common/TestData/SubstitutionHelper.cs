using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Interfaces;
using Refit;
using System.Net;

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
    }
}
