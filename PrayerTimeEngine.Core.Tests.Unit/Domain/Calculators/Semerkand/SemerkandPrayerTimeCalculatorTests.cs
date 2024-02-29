using Microsoft.Extensions.Logging;
using NodaTime;
using NSubstitute;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Models;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Services;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Models.Common;
using FluentAssertions;
using NSubstitute.ReceivedExtensions;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Tests.Common;

namespace PrayerTimeEngine.Core.Tests.Unit.Domain.Calculators.Semerkand
{
    public class SemerkandPrayerTimeCalculatorTests : BaseTest
    {
        private readonly ISemerkandDBAccess _semerkandDBAccessMock;
        private readonly ISemerkandApiService _semerkandApiServiceMock;
        private readonly IPlaceService _placeServiceMock;
        private readonly SemerkandPrayerTimeCalculator _semerkandPrayerTimeCalculator;

        public SemerkandPrayerTimeCalculatorTests()
        {
            _semerkandDBAccessMock = Substitute.For<ISemerkandDBAccess>();
            _semerkandApiServiceMock = Substitute.For<ISemerkandApiService>();
            _placeServiceMock = Substitute.For<IPlaceService>();

            _semerkandPrayerTimeCalculator = 
                new SemerkandPrayerTimeCalculator(
                    _semerkandDBAccessMock, 
                    _semerkandApiServiceMock, 
                    _placeServiceMock, 
                    Substitute.For<ILogger<SemerkandPrayerTimeCalculator>>());
        }

        #region GetPrayerTimesAsync

        [Fact]
        public async Task GetPrayerTimesAsync_AllValuesFromDbCache_Success()
        {
            // ARRANGE
            var date = new LocalDate(2024, 1, 1);
            ZonedDateTime dateInUtc = date.AtStartOfDayInZone(DateTimeZone.Utc);
            BaseLocationData locationData = new SemerkandLocationData { CityName = "Berlin", CountryName = "Deutschland", TimezoneName = "Europe/Vienna" };
            List<GenericSettingConfiguration> configurations = 
                [
                    new GenericSettingConfiguration { TimeType = ETimeType.FajrEnd, Source = ECalculationSource.Semerkand }
                ];

            _semerkandDBAccessMock.GetCountryIDByName(Arg.Is("Deutschland")).Returns(1);
            _semerkandDBAccessMock.GetCityIDByName(Arg.Is(1), Arg.Is("Berlin")).Returns(1);

            var times = new SemerkandPrayerTimes
            {
                CityID = 1,
                DayOfYear = 5,
                Date = date,
                Fajr = dateInUtc.PlusHours(5),
                Shuruq = dateInUtc.PlusHours(7),
                Dhuhr = dateInUtc.PlusHours(12),
                Asr = dateInUtc.PlusHours(15),
                Maghrib = dateInUtc.PlusHours(18),
                Isha = dateInUtc.PlusHours(20),
            };
            
            _semerkandDBAccessMock.GetTimesByDateAndCityID(
                Arg.Is<LocalDate>(x => x == date || x == date.PlusDays(1)), 
                Arg.Any<int>())
                .Returns(times);
            
            // ACT
            var calculationResult = 
                await _semerkandPrayerTimeCalculator.GetPrayerTimesAsync(date, locationData, configurations);

            // ASSERT
            calculationResult.Should().NotBeNull().And.HaveCount(1);
            calculationResult.First().Should().HaveCount(1);
            calculationResult.First().Key.Should().BeEquivalentTo(times);

            _placeServiceMock.ReceivedCalls().Should().BeEmpty();
            _semerkandApiServiceMock.ReceivedCalls().Should().BeEmpty();
            _semerkandDBAccessMock.ReceivedCalls().Should().HaveCount(4);
            await _semerkandDBAccessMock.Received(1).GetCountryIDByName(Arg.Is("Deutschland"));
            await _semerkandDBAccessMock.Received(1).GetCityIDByName(Arg.Is(1), Arg.Is("Berlin"));
            await _semerkandDBAccessMock.Received(1).GetTimesByDateAndCityID(Arg.Is(date), Arg.Is(1));
            await _semerkandDBAccessMock.Received(1).GetTimesByDateAndCityID(Arg.Is(date.PlusDays(1)), Arg.Is(1));
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

            var turkishBasicPlaceInfo = new BasicPlaceInfo("1", 1M, 1M, "de", "Avusturya", "Innsbruck", "", "6020", "Yol");;
            
            _placeServiceMock
                .GetPlaceBasedOnPlace(Arg.Is(completePlaceInfo), Arg.Is("tr"))
                .Returns(turkishBasicPlaceInfo);
            _semerkandDBAccessMock.GetCountryIDByName(Arg.Is("Avusturya")).Returns(1);
            _semerkandDBAccessMock.GetCityIDByName(Arg.Is(1), Arg.Is("Innsbruck")).Returns(1);
            
            // ACT
            var locationData = await _semerkandPrayerTimeCalculator.GetLocationInfo(completePlaceInfo) as SemerkandLocationData;

            // ASSERT
            locationData.Should().NotBeNull();
            locationData.CountryName.Should().Be("Avusturya");
            locationData.CityName.Should().Be("Innsbruck");
            locationData.Source.Should().Be(ECalculationSource.Semerkand);
            locationData.TimezoneName.Should().Be("Europe/Vienna");
        }

        #endregion GetLocationInfo
    }
}
