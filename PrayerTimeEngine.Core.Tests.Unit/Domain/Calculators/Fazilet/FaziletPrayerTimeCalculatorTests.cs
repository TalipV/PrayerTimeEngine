using FluentAssertions;
using Microsoft.Extensions.Logging;
using NodaTime;
using NSubstitute;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Models;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Services;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Models.Common;
using PrayerTimeEngine.Core.Tests.Common;

namespace PrayerTimeEngine.Core.Tests.Unit.Domain.Calculators.Fazilet
{
    public class FaziletPrayerTimeCalculatorTests : BaseTest
    {
        private readonly IFaziletDBAccess _faziletDBAccessMock;
        private readonly IFaziletApiService _faziletApiServiceMock;
        private readonly IPlaceService _placeServiceMock;
        private readonly FaziletPrayerTimeCalculator _faziletPrayerTimeCalculator;

        public FaziletPrayerTimeCalculatorTests()
        {
            _faziletDBAccessMock = Substitute.For<IFaziletDBAccess>();
            _faziletApiServiceMock = Substitute.For<IFaziletApiService>();
            _placeServiceMock = Substitute.For<IPlaceService>();

            _faziletPrayerTimeCalculator =
                new FaziletPrayerTimeCalculator(
                    _faziletDBAccessMock,
                    _faziletApiServiceMock,
                    _placeServiceMock,
                    Substitute.For<ILogger<FaziletPrayerTimeCalculator>>());
        }

        #region GetPrayerTimesAsync

        [Fact]
        public async Task GetPrayerTimesAsync_AllValuesFromDbCache_Success()
        {
            // ARRANGE
            var date = new LocalDate(2024, 1, 1);
            ZonedDateTime dateInUtc = date.AtStartOfDayInZone(DateTimeZone.Utc);
            BaseLocationData locationData = new FaziletLocationData { CityName = "Berlin", CountryName = "Deutschland" };
            List<GenericSettingConfiguration> configurations =
                [
                    new GenericSettingConfiguration { TimeType = ETimeType.FajrEnd, Source = ECalculationSource.Fazilet }
                ];

            _faziletDBAccessMock.GetCountryIDByName(Arg.Is("Deutschland"), Arg.Any<CancellationToken>()).Returns(1);
            _faziletDBAccessMock.GetCityIDByName(Arg.Is(1), Arg.Is("Berlin"), Arg.Any<CancellationToken>()).Returns(1);

            var times = new FaziletPrayerTimes
            {
                CityID = 1,
                Date = date,
                Imsak = dateInUtc.PlusHours(4),
                Fajr = dateInUtc.PlusHours(5),
                Shuruq = dateInUtc.PlusHours(7),
                Dhuhr = dateInUtc.PlusHours(12),
                Asr = dateInUtc.PlusHours(15),
                Maghrib = dateInUtc.PlusHours(18),
                Isha = dateInUtc.PlusHours(20),
            };

            _faziletDBAccessMock.GetTimesByDateAndCityID(
                Arg.Is<LocalDate>(x => x == date || x == date.PlusDays(1)),
                Arg.Any<int>(), 
                Arg.Any<CancellationToken>())
                .Returns(times);

            // ACT
            var calculationResult =
                await _faziletPrayerTimeCalculator.GetPrayerTimesAsync(date, locationData, configurations, default);

            // ASSERT
            calculationResult.Should().NotBeNull().And.HaveCount(1);
            calculationResult.First().Should().HaveCount(1);
            calculationResult.First().Key.Should().BeEquivalentTo(times);

            _placeServiceMock.ReceivedCalls().Should().BeEmpty();
            _faziletApiServiceMock.ReceivedCalls().Should().BeEmpty();
            _faziletDBAccessMock.ReceivedCalls().Should().HaveCount(4);
            await _faziletDBAccessMock.Received(1).GetCountryIDByName(Arg.Is("Deutschland"), Arg.Any<CancellationToken>());
            await _faziletDBAccessMock.Received(1).GetCityIDByName(Arg.Is(1), Arg.Is("Berlin"), Arg.Any<CancellationToken>());
            await _faziletDBAccessMock.Received(1).GetTimesByDateAndCityID(Arg.Is(date), Arg.Is(1), Arg.Any<CancellationToken>());
            await _faziletDBAccessMock.Received(1).GetTimesByDateAndCityID(Arg.Is(date.PlusDays(1)), Arg.Is(1), Arg.Any<CancellationToken>());
        }

        #endregion GetPrayerTimesAsync

        #region GetLocationInfo

        [Fact]
        public async Task GetLocationInfo_X_X()
        {
            // ARRANGE
            var basicPlaceInfo = new BasicPlaceInfo("1", 1M, 1M, "de", "Österreich", "Innsbruck", "", "6020", "Straße");
            var completePlaceInfo = new CompletePlaceInfo(basicPlaceInfo)
            {
                TimezoneInfo = new TimezoneInfo { Name = "Europe/Vienna" },
            };

            var turkishBasicPlaceInfo = new BasicPlaceInfo("1", 1M, 1M, "de", "Avusturya", "Innsbruck", "", "6020", "Yol"); ;

            _placeServiceMock
                .GetPlaceBasedOnPlace(Arg.Is(completePlaceInfo), Arg.Is("tr"), Arg.Any<CancellationToken>())
                .Returns(turkishBasicPlaceInfo);
            _faziletDBAccessMock.GetCountryIDByName(Arg.Is("Avusturya"), Arg.Any<CancellationToken>()).Returns(1);
            _faziletDBAccessMock.GetCityIDByName(Arg.Is(1), Arg.Is("Innsbruck"), Arg.Any<CancellationToken>()).Returns(1);

            // ACT
            var locationData = await _faziletPrayerTimeCalculator.GetLocationInfo(completePlaceInfo, default) as FaziletLocationData;

            // ASSERT
            locationData.Should().NotBeNull();
            locationData.CountryName.Should().Be("Avusturya");
            locationData.CityName.Should().Be("Innsbruck");
            locationData.Source.Should().Be(ECalculationSource.Fazilet);
        }

        #endregion GetLocationInfo
    }
}
