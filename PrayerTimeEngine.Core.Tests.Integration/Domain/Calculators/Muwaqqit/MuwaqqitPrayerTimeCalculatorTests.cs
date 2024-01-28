using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NodaTime;
using NSubstitute;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Models;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Services;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Interfaces;
using PrayerTimeEngine.Core.Tests.Common;
using System.Net;

namespace PrayerTimeEngine.Core.Tests.Integration.Domain.Calculators.Muwaqqit
{
    public class MuwaqqitPrayerTimeCalculatorTests : BaseTest
    {
        private static MuwaqqitApiService getMockedMuwaqqitApiService()
        {
            Func<HttpRequestMessage, HttpResponseMessage> handleRequestFunc =
                (request) =>
                {
                    string responseText =
                        request.RequestUri.AbsoluteUri switch
                        {
                            @"https://www.muwaqqit.com/api2.json?d=2023-07-30&ln=11.41337&lt=47.2803835&tz=Europe%2fVienna&fa=-12&ia=3.5&isn=-8&ea=-12" => File.ReadAllText(Path.Combine(TEST_DATA_FILE_PATH, "MuwaqqitTestData", "Muwaqqit_TestPrayerTimeData_20230730_Innsbruck_Part1.txt")),
                            @"https://www.muwaqqit.com/api2.json?d=2023-07-30&ln=11.41337&lt=47.2803835&tz=Europe%2fVienna&fa=-7.5&ia=4.5&isn=-12&ea=-15.5" => File.ReadAllText(Path.Combine(TEST_DATA_FILE_PATH, "MuwaqqitTestData", "Muwaqqit_TestPrayerTimeData_20230730_Innsbruck_Part2.txt")),
                            @"https://www.muwaqqit.com/api2.json?d=2023-07-30&ln=11.41337&lt=47.2803835&tz=Europe%2fVienna&fa=-4.5&ia=-12&isn=-12&ea=-12" => File.ReadAllText(Path.Combine(TEST_DATA_FILE_PATH, "MuwaqqitTestData", "Muwaqqit_TestPrayerTimeData_20230730_Innsbruck_Part3.txt")),
                            @"https://www.muwaqqit.com/api2.json?d=2023-07-30&ln=11.41337&lt=47.2803835&tz=Europe%2fVienna&fa=-15&ia=-12&isn=-12&ea=-12" => File.ReadAllText(Path.Combine(TEST_DATA_FILE_PATH, "MuwaqqitTestData", "Muwaqqit_TestPrayerTimeData_20230730_Innsbruck_Part4.txt")),
                            _ => throw new Exception($"No response registered for URL: {request.RequestUri.AbsoluteUri}")
                        };

                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(responseText)
                    };
                };

            var mockHttpMessageHandler = new MockHttpMessageHandler(handleRequestFunc);
            var httpClient = new HttpClient(mockHttpMessageHandler);

            return new MuwaqqitApiService(httpClient);
        }

        [Fact]
        public async Task MuwaqqitPrayerTimeCalculator_GetPrayerTimesAsyncWithNormalInput_PrayerTimesForThatDay()
        {
            // ARRANGE
            ServiceProvider serviceProvider = createServiceProvider(
                configureServiceCollection: serviceCollection =>
                {
                    SetUpTestDbContext(serviceCollection);
                    serviceCollection.AddSingleton(Substitute.For<IPlaceService>());
                    serviceCollection.AddSingleton<TimeTypeAttributeService>();

                    serviceCollection.AddSingleton(Substitute.For<ILogger<MuwaqqitDBAccess>>());
                    serviceCollection.AddSingleton<IMuwaqqitDBAccess, MuwaqqitDBAccess>();
                    serviceCollection.AddSingleton<IMuwaqqitApiService>(getMockedMuwaqqitApiService());
                    serviceCollection.AddSingleton(Substitute.For<ILogger<MuwaqqitPrayerTimeCalculator>>());
                    serviceCollection.AddSingleton<MuwaqqitPrayerTimeCalculator>();
                });

            LocalDate testDate = new LocalDate(2023, 7, 30);
            List<GenericSettingConfiguration> configs =
                new()
                {
                    new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.FajrStart, Degree = -12.0 },
                    new GenericSettingConfiguration { TimeType = ETimeType.FajrEnd, Source = ECalculationSource.Muwaqqit },
                    new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.FajrGhalas, Degree = -7.5 },
                    new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.FajrKaraha, Degree = -4.5 },
                    new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.DuhaStart, Degree = 3.5 },
                    new GenericSettingConfiguration { TimeType = ETimeType.DhuhrStart, Source = ECalculationSource.Muwaqqit },
                    new GenericSettingConfiguration { TimeType = ETimeType.DhuhrEnd, Source = ECalculationSource.Muwaqqit },
                    new GenericSettingConfiguration { TimeType = ETimeType.AsrStart, Source = ECalculationSource.Muwaqqit },
                    new GenericSettingConfiguration { TimeType = ETimeType.AsrEnd, Source = ECalculationSource.Muwaqqit },
                    new GenericSettingConfiguration { TimeType = ETimeType.AsrMithlayn, Source = ECalculationSource.Muwaqqit },
                    new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.AsrKaraha, Degree = 4.5 },
                    new GenericSettingConfiguration { TimeType = ETimeType.MaghribStart, Source = ECalculationSource.Muwaqqit },
                    new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.MaghribEnd, Degree = -12.0 },
                    new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.MaghribIshtibaq, Degree = -8 },
                    new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.IshaStart, Degree = -15.5 },
                    new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.IshaEnd, Degree = -15.0 },
                };

            MuwaqqitPrayerTimeCalculator muwaqqitPrayerTimeCalculator = serviceProvider.GetService<MuwaqqitPrayerTimeCalculator>();

            // ACT
            ILookup<ICalculationPrayerTimes, ETimeType> result =
                await muwaqqitPrayerTimeCalculator.GetPrayerTimesAsync(
                    testDate,
                    new MuwaqqitLocationData
                    {
                        Latitude = 47.2803835M,
                        Longitude = 11.41337M,
                        TimezoneName = "Europe/Vienna"
                    },
                    configs
                );

            IDictionary<ETimeType, MuwaqqitPrayerTimes> timeTypeByCalculationPrayerTimes =
                result
                .SelectMany(pair => pair.Select(value => new { Key = value, Value = pair.Key }))
                .ToDictionary(x => x.Key, x => x.Value as MuwaqqitPrayerTimes);

            // ASSERT
            timeTypeByCalculationPrayerTimes.Should().HaveCount(16);
            result.Select(x => x.Key.Date).Should().AllBeEquivalentTo(new LocalDate(2023, 7, 30));

            getMappedValue(ETimeType.FajrStart, timeTypeByCalculationPrayerTimes).Should().Be(new LocalDateTime(2023, 7, 30, 04, 27, 04));
            getMappedValue(ETimeType.FajrEnd, timeTypeByCalculationPrayerTimes).Should().Be(new LocalDateTime(2023, 7, 30, 05, 49, 53));
            getMappedValue(ETimeType.FajrGhalas, timeTypeByCalculationPrayerTimes).Should().Be(new LocalDateTime(2023, 7, 30, 05, 02, 27));
            getMappedValue(ETimeType.FajrKaraha, timeTypeByCalculationPrayerTimes).Should().Be(new LocalDateTime(2023, 7, 30, 05, 20, 25));

            getMappedValue(ETimeType.DuhaStart, timeTypeByCalculationPrayerTimes).Should().Be(new LocalDateTime(2023, 7, 30, 06, 17, 04));

            getMappedValue(ETimeType.DhuhrStart, timeTypeByCalculationPrayerTimes).Should().Be(new LocalDateTime(2023, 7, 30, 13, 21, 22));
            getMappedValue(ETimeType.DhuhrEnd, timeTypeByCalculationPrayerTimes).Should().Be(new LocalDateTime(2023, 7, 30, 17, 25, 53));

            getMappedValue(ETimeType.AsrStart, timeTypeByCalculationPrayerTimes).Should().Be(new LocalDateTime(2023, 7, 30, 17, 25, 53));
            getMappedValue(ETimeType.AsrEnd, timeTypeByCalculationPrayerTimes).Should().Be(new LocalDateTime(2023, 7, 30, 20, 50, 59));
            getMappedValue(ETimeType.AsrMithlayn, timeTypeByCalculationPrayerTimes).Should().Be(new LocalDateTime(2023, 7, 30, 18, 33, 27));
            getMappedValue(ETimeType.AsrKaraha, timeTypeByCalculationPrayerTimes).Should().Be(new LocalDateTime(2023, 7, 30, 20, 17, 15));

            getMappedValue(ETimeType.MaghribStart, timeTypeByCalculationPrayerTimes).Should().Be(new LocalDateTime(2023, 7, 30, 20, 50, 59));
            getMappedValue(ETimeType.MaghribEnd, timeTypeByCalculationPrayerTimes).Should().Be(new LocalDateTime(2023, 7, 30, 22, 13, 17));
            getMappedValue(ETimeType.MaghribIshtibaq, timeTypeByCalculationPrayerTimes).Should().Be(new LocalDateTime(2023, 7, 30, 21, 41, 46));

            getMappedValue(ETimeType.IshaStart, timeTypeByCalculationPrayerTimes).Should().Be(new LocalDateTime(2023, 7, 30, 22, 44, 14));
            getMappedValue(ETimeType.IshaEnd, timeTypeByCalculationPrayerTimes).Should().Be(new LocalDateTime(2023, 7, 31, 04, 02, 30));
        }

        private LocalDateTime getMappedValue(ETimeType timeType, IDictionary<ETimeType, MuwaqqitPrayerTimes> timeTypeByCalculationPrayerTimes)
        {
            return
                timeTypeByCalculationPrayerTimes[timeType]
                .GetZonedDateTimeForTimeType(timeType)
                .LocalDateTime;
        }
    }
}