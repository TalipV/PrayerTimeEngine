using NodaTime;
using NSubstitute;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Models;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Models.DTOs;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Services;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Tests.Common;

namespace PrayerTimeEngine.Core.Tests.Unit.Domain.Calculators.Muwaqqit
{
    public class MuwaqqitPrayerTimeCalculatorTests : BaseTest
    {
        private readonly IMuwaqqitDBAccess _muwaqqitDBAccessMock;
        private readonly IMuwaqqitApiService _muwaqqitApiServiceMock;
        private readonly MuwaqqitPrayerTimeCalculator _muwaqqitPrayerTimeCalculator;

        public MuwaqqitPrayerTimeCalculatorTests()
        {
            _muwaqqitDBAccessMock = Substitute.For<IMuwaqqitDBAccess>();
            _muwaqqitApiServiceMock = Substitute.For<IMuwaqqitApiService>();

            _muwaqqitPrayerTimeCalculator =
                new MuwaqqitPrayerTimeCalculator(
                    _muwaqqitDBAccessMock,
                    _muwaqqitApiServiceMock,
                    new TimeTypeAttributeService());
        }

        [Fact]
        public async Task GetPrayerTimesAsync_ReturnsCorrectTimes_GivenValidData()
        {
            // ARRANGE
            var date = new LocalDate(2024, 2, 25);
            var locationData = new MuwaqqitLocationData
            {
                Longitude = 39.8262M,
                Latitude = 21.4225M,
                TimezoneName = "Asia/Riyadh"
            };
            List<GenericSettingConfiguration> configurations =
            [
                new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.FajrStart, Degree = 18 },
                new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.IshaEnd, Degree = 18 }
            ];

            var expectedResponse = new MuwaqqitPrayerTimesResponseDTO
            {
                Date = date,
                Timezone = DateTimeZoneProviders.Tzdb[locationData.TimezoneName],
                Latitude = locationData.Latitude,
                Longitude = locationData.Longitude,
                FajrDegree = 18,
                IshaDegree = 18,
                IshtibaqDegree = 1,
                AsrKarahaDegree = 1,
                Fajr = new ZonedDateTime(Instant.FromUtc(2023, 7, 30, 2, 27, 04), DateTimeZoneProviders.Tzdb[locationData.TimezoneName]).ToOffsetDateTime(),
                NextFajr = new ZonedDateTime(Instant.FromUtc(2023, 7, 31, 2, 28, 04), DateTimeZoneProviders.Tzdb[locationData.TimezoneName]).ToOffsetDateTime(),
                Shuruq = new ZonedDateTime(Instant.FromUtc(2023, 7, 30, 3, 49, 53), DateTimeZoneProviders.Tzdb[locationData.TimezoneName]).ToOffsetDateTime(),
                Ishraq = new ZonedDateTime(Instant.FromUtc(2023, 7, 30, 4, 49, 53), DateTimeZoneProviders.Tzdb[locationData.TimezoneName]).ToOffsetDateTime(),
                Dhuhr = new ZonedDateTime(Instant.FromUtc(2023, 7, 30, 11, 21, 22), DateTimeZoneProviders.Tzdb[locationData.TimezoneName]).ToOffsetDateTime(),
                Asr = new ZonedDateTime(Instant.FromUtc(2023, 7, 30, 15, 25, 53), DateTimeZoneProviders.Tzdb[locationData.TimezoneName]).ToOffsetDateTime(),
                AsrMithlayn = new ZonedDateTime(Instant.FromUtc(2023, 7, 30, 16, 25, 53), DateTimeZoneProviders.Tzdb[locationData.TimezoneName]).ToOffsetDateTime(),
                AsrKaraha = new ZonedDateTime(Instant.FromUtc(2023, 7, 30, 17, 25, 53), DateTimeZoneProviders.Tzdb[locationData.TimezoneName]).ToOffsetDateTime(),
                Maghrib = new ZonedDateTime(Instant.FromUtc(2023, 7, 30, 18, 50, 59), DateTimeZoneProviders.Tzdb[locationData.TimezoneName]).ToOffsetDateTime(),
                Ishtibaq = new ZonedDateTime(Instant.FromUtc(2023, 7, 30, 19, 50, 59), DateTimeZoneProviders.Tzdb[locationData.TimezoneName]).ToOffsetDateTime(),
                Isha = new ZonedDateTime(Instant.FromUtc(2023, 7, 30, 20, 13, 17), DateTimeZoneProviders.Tzdb[locationData.TimezoneName]).ToOffsetDateTime()
            };

            _muwaqqitApiServiceMock.GetTimesAsync(
                    date: Arg.Any<string>(),
                    longitude: Arg.Any<decimal>(),
                    latitude: Arg.Any<decimal>(),
                    timezone: Arg.Any<string>(),
                    fajrDegree: Arg.Any<double>(),
                    ishaDegree: Arg.Any<double>(),
                    ishtibaqDegree: Arg.Any<double>(),
                    asrKarahaDegree: Arg.Any<double>(),
                    cancellationToken: Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(expectedResponse));

            // ACT
            List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)> result = 
                await _muwaqqitPrayerTimeCalculator.GetPrayerTimesAsync(date, locationData, configurations, default);

            // ASSERT
            result.FirstOrDefault(x => x.TimeType == ETimeType.FajrStart).Should().NotBeNull();
            result.FirstOrDefault(x => x.TimeType == ETimeType.IshaEnd).Should().NotBeNull();
        }

    }
}
