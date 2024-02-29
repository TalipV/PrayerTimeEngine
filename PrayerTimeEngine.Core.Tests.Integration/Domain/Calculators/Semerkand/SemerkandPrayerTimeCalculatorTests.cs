using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NodaTime;
using NSubstitute;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Models;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Services;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Interfaces;
using PrayerTimeEngine.Core.Tests.Common;
using System.Net;

namespace PrayerTimeEngine.Core.Tests.Integration.Domain.Calculators.Semerkand
{
    public class SemerkandPrayerTimeCalculatorTests : BaseTest
    {
        private static SemerkandApiService getMockedSemerkandApiService()
        {
            Func<HttpRequestMessage, HttpResponseMessage> handleRequestFunc =
                (request) =>
                {
                    string responseText;

                    if (request.RequestUri.AbsoluteUri == $@"{SemerkandApiService.GET_COUNTRIES_URL}")
                        responseText = File.ReadAllText(Path.Combine(TEST_DATA_FILE_PATH, "SemerkandTestData", "Semerkand_TestCountriesData.txt"));
                    else if (request.RequestUri.AbsoluteUri == $@"{SemerkandApiService.GET_CITIES_BY_COUNTRY_URL}")
                        responseText = File.ReadAllText(Path.Combine(TEST_DATA_FILE_PATH, "SemerkandTestData", "Semerkand_TestCityData_Austria.txt"));
                    else if (request.RequestUri.AbsoluteUri == $@"{string.Format(SemerkandApiService.GET_TIMES_BY_CITY, "197", "2023")}")
                        responseText = File.ReadAllText(Path.Combine(TEST_DATA_FILE_PATH, "SemerkandTestData", "Semerkand_TestPrayerTimeData_20230729_Innsbruck.txt"));
                    else
                        throw new Exception($"No response registered for URL: {request.RequestUri.AbsoluteUri}");

                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(responseText)
                    };
                };

            var mockHttpMessageHandler = new MockHttpMessageHandler(handleRequestFunc);
            var httpClient = new HttpClient(mockHttpMessageHandler);

            return new SemerkandApiService(httpClient);
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

                    serviceCollection.AddSingleton<ISemerkandDBAccess, SemerkandDBAccess>();
                    serviceCollection.AddSingleton<ISemerkandApiService>(getMockedSemerkandApiService());
                    serviceCollection.AddSingleton(Substitute.For<ILogger<SemerkandPrayerTimeCalculator>>());
                    serviceCollection.AddSingleton<SemerkandPrayerTimeCalculator>();
                });

            SemerkandPrayerTimeCalculator semerkandPrayerTimeCalculator = serviceProvider.GetService<SemerkandPrayerTimeCalculator>();

            // ACT
            ICalculationPrayerTimes result =
                (await semerkandPrayerTimeCalculator.GetPrayerTimesAsync(
                    new LocalDate(2023, 7, 29),
                    new SemerkandLocationData
                    {
                        CountryName = "Avusturya",
                        CityName = "Innsbruck",
                        TimezoneName = "Europe/Vienna"
                    },
                    [new GenericSettingConfiguration { TimeType = ETimeType.DhuhrStart, Source = ECalculationSource.Semerkand }]
                )).Single().Key;

            SemerkandPrayerTimes semerkandPrayerTimes = result as SemerkandPrayerTimes;

            // ASSERT
            semerkandPrayerTimes.Should().NotBeNull();

            semerkandPrayerTimes.Date.Should().Be(new LocalDate(2023, 7, 29));
            semerkandPrayerTimes.Fajr.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 29, 03, 15, 0));
            semerkandPrayerTimes.NextFajr.Value.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 30, 03, 17, 0));

            semerkandPrayerTimes.Shuruq.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 29, 05, 41, 0));
            semerkandPrayerTimes.Dhuhr.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 29, 13, 26, 0));
            semerkandPrayerTimes.Asr.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 29, 17, 30, 0));
            semerkandPrayerTimes.Maghrib.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 29, 21, 00, 0));
            semerkandPrayerTimes.Isha.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 29, 23, 02, 0));
        }
    }
}