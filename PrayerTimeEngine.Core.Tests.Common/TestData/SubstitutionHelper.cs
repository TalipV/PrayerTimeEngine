using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Services;
using Refit;
using System.Net;

namespace PrayerTimeEngine.Core.Tests.Common.TestData
{
    public class SubstitutionHelper
    {
        public static SemerkandApiService GetMockedSemerkandApiService()
        {
            static HttpResponseMessage handleRequestFunc(HttpRequestMessage request)
            {
                Stream responseStream;

                if (request.RequestUri.AbsoluteUri == SemerkandApiService.GET_COUNTRIES_URL)
                    responseStream = File.OpenRead(Path.Combine(TestDataHelper.SEMERKAND_TEST_DATA_FILE_PATH, "Semerkand_TestCountriesData.txt"));
                else if (request.RequestUri.AbsoluteUri == SemerkandApiService.GET_CITIES_BY_COUNTRY_URL)
                    responseStream = File.OpenRead(Path.Combine(TestDataHelper.SEMERKAND_TEST_DATA_FILE_PATH, "Semerkand_TestCityData_Austria.txt"));
                else if (request.RequestUri.AbsoluteUri == string.Format(SemerkandApiService.GET_TIMES_BY_CITY, "197", "2023"))
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
            var httpClient = new HttpClient(mockHttpMessageHandler);

            return new SemerkandApiService(httpClient);
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

                if (request.RequestUri.AbsoluteUri == $@"{baseURL}/daily?lang=1&districtId=232")
                    responseStream = File.OpenRead(Path.Combine(TestDataHelper.FAZILET_TEST_DATA_FILE_PATH, "Fazilet_TestCountriesData.txt"));
                else if (request.RequestUri.AbsoluteUri == $@"{baseURL}/cities-by-country?districtId=2")
                    responseStream = File.OpenRead(Path.Combine(TestDataHelper.FAZILET_TEST_DATA_FILE_PATH, "Fazilet_TestCityData_Austria.txt"));
                else if (request.RequestUri.AbsoluteUri == $@"{baseURL}/daily?lang=2&districtId=92")
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
