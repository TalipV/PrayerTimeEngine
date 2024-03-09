using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NodaTime;
using NSubstitute;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Models;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Services;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Interfaces;
using PrayerTimeEngine.Core.Tests.Common;
using System.Net;

namespace PrayerTimeEngine.Core.Tests.Integration.Domain.Calculators.Fazilet
{
    public class FaziletPrayerTimeCalculatorTests : BaseTest
    {
        private static FaziletApiService getMockedFaziletApiService()
        {
            string dummyBaseURL = @"http://dummy.url.com";

            HttpResponseMessage handleRequestFunc(HttpRequestMessage request)
            {
                Stream responseStream;

                if (request.RequestUri.AbsoluteUri == $@"{dummyBaseURL}/{FaziletApiService.GET_COUNTRIES_URL}")
                    responseStream = File.OpenRead(Path.Combine(FAZILET_TEST_DATA_FILE_PATH, "Fazilet_TestCountriesData.txt"));
                else if (request.RequestUri.AbsoluteUri == $@"{dummyBaseURL}/{FaziletApiService.GET_CITIES_BY_COUNTRY_URL}2")
                    responseStream = File.OpenRead(Path.Combine(FAZILET_TEST_DATA_FILE_PATH, "Fazilet_TestCityData_Austria.txt"));
                else if (request.RequestUri.AbsoluteUri == $@"{dummyBaseURL}/{string.Format(FaziletApiService.GET_TIMES_BY_CITY_URL, "92")}")
                    responseStream = File.OpenRead(Path.Combine(FAZILET_TEST_DATA_FILE_PATH, "Fazilet_TestPrayerTimeData_20230729_Innsbruck.txt"));
                else
                    throw new Exception($"No response registered for URL: {request.RequestUri.AbsoluteUri}");

                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StreamContent(responseStream)
                };
            }

            var mockHttpMessageHandler = new MockHttpMessageHandler(handleRequestFunc);
            var httpClient = new HttpClient(mockHttpMessageHandler) { BaseAddress = new Uri(dummyBaseURL) };

            return new FaziletApiService(httpClient);
        }

        [Fact]
        public async Task GetPrayerTimesAsync_NormalInput_PrayerTimesForThatDay()
        {
            // ARRANGE
            ServiceProvider serviceProvider = createServiceProvider(
                configureServiceCollection: serviceCollection =>
                {
                    serviceCollection.AddSingleton(GetHandledDbContext());
                    serviceCollection.AddSingleton(Substitute.For<IPlaceService>());
                    serviceCollection.AddSingleton<IFaziletDBAccess, FaziletDBAccess>();
                    serviceCollection.AddSingleton<IFaziletApiService>(getMockedFaziletApiService());
                    serviceCollection.AddSingleton(Substitute.For<ILogger<FaziletPrayerTimeCalculator>>());
                    serviceCollection.AddSingleton<FaziletPrayerTimeCalculator>();
                });
            FaziletPrayerTimeCalculator faziletPrayerTimeCalculator = serviceProvider.GetService<FaziletPrayerTimeCalculator>();

            // ACT
            ICalculationPrayerTimes result =
                (await faziletPrayerTimeCalculator.GetPrayerTimesAsync(
                    new LocalDate(2023, 7, 29),
                    new FaziletLocationData { CountryName = "Avusturya", CityName = "Innsbruck" },
                    [new GenericSettingConfiguration { TimeType = ETimeType.DhuhrStart, Source = ECalculationSource.Fazilet }], 
                    default))
                .Single().Key;

            FaziletPrayerTimes faziletPrayerTimes = result as FaziletPrayerTimes;

            // ASSERT
            faziletPrayerTimes.Should().NotBeNull();

            faziletPrayerTimes.Date.Should().Be(new LocalDate(2023, 7, 29));
            faziletPrayerTimes.Imsak.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 29, 03, 04, 0));
            faziletPrayerTimes.Fajr.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 29, 03, 24, 0));
            faziletPrayerTimes.NextFajr.Value.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 30, 03, 27, 0));

            faziletPrayerTimes.Shuruq.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 29, 05, 43, 0));
            faziletPrayerTimes.Dhuhr.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 29, 13, 28, 0));
            faziletPrayerTimes.Asr.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 29, 17, 31, 0));
            faziletPrayerTimes.Maghrib.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 29, 21, 02, 0));
            faziletPrayerTimes.Isha.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 29, 23, 11, 0));
        }
    }
}