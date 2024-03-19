using Microsoft.Extensions.Logging;
using NodaTime;
using NSubstitute;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Models;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Services;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Interfaces;
using FluentAssertions;
using NSubstitute.ReceivedExtensions;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Tests.Common;
using NSubstitute.ReturnsExtensions;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Models.Entities;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Models.DTOs;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Models;

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

            _semerkandDBAccessMock.GetCountryIDByName(Arg.Is("Deutschland"), Arg.Any<CancellationToken>()).Returns(1);
            _semerkandDBAccessMock.GetCityIDByName(Arg.Is(1), Arg.Is("Berlin"), Arg.Any<CancellationToken>()).Returns(1);

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
                    Arg.Any<int>(), 
                    Arg.Any<CancellationToken>())
                .Returns(times);

            // ACT
            List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)> calculationResult = 
                await _semerkandPrayerTimeCalculator.GetPrayerTimesAsync(date, locationData, configurations, default);

            // ASSERT
            calculationResult.Should().NotBeNull().And.HaveCount(1);
            calculationResult.First().Should().BeEquivalentTo((ETimeType.FajrEnd, times.Shuruq));

            _placeServiceMock.ReceivedCalls().Should().BeEmpty();
            _semerkandApiServiceMock.ReceivedCalls().Should().BeEmpty();
            _semerkandDBAccessMock.ReceivedCalls().Should().HaveCount(4);
            await _semerkandDBAccessMock.Received(1).GetCountryIDByName(Arg.Is("Deutschland"), Arg.Any<CancellationToken>());
            await _semerkandDBAccessMock.Received(1).GetCityIDByName(Arg.Is(1), Arg.Is("Berlin"), Arg.Any<CancellationToken>());
            await _semerkandDBAccessMock.Received(1).GetTimesByDateAndCityID(Arg.Is(date), Arg.Is(1), Arg.Any<CancellationToken>());
            await _semerkandDBAccessMock.Received(1).GetTimesByDateAndCityID(Arg.Is(date.PlusDays(1)), Arg.Is(1), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task GetPrayerTimesAsync_AllValuesFromApi_Success()
        {
            // ARRANGE
            var date = new LocalDate(2024, 1, 1);
            DateTimeZone dateTimeZone = DateTimeZoneProviders.Tzdb["Europe/Vienna"];
            ZonedDateTime zonedDateTime = date.AtStartOfDayInZone(dateTimeZone);

            var locationData = new SemerkandLocationData { CityName = "Berlin", CountryName = "Deutschland", TimezoneName = dateTimeZone.Id };
            List<GenericSettingConfiguration> configurations =
                [
                    new GenericSettingConfiguration { TimeType = ETimeType.FajrEnd, Source = ECalculationSource.Semerkand }
                ];

            _semerkandDBAccessMock.GetCountryIDByName(Arg.Any<string>(), Arg.Any<CancellationToken>()).ReturnsNull();
            _semerkandDBAccessMock.GetCityIDByName(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).ReturnsNull();
            _semerkandDBAccessMock.HasCountryData(Arg.Any<CancellationToken>()).Returns(false);

            _semerkandApiServiceMock.GetCountries(Arg.Any<CancellationToken>()).Returns([new SemerkandCountryResponseDTO { Name = "Deutschland", ID = 1 }]);
            _semerkandApiServiceMock.GetCitiesByCountryID(Arg.Is(1), Arg.Any<CancellationToken>()).Returns([new SemerkandCityResponseDTO { Name = "Berlin", ID = 1 }]);

            ZonedDateTime shuruqZonedDateTime = zonedDateTime.PlusHours(7);
            var semerkandPrayerTimesDTO = new SemerkandPrayerTimesResponseDTO
            {
                DayOfYear = 1,
                Fajr = zonedDateTime.PlusHours(5).TimeOfDay,
                Shuruq = shuruqZonedDateTime.TimeOfDay,
                Dhuhr = zonedDateTime.PlusHours(12).TimeOfDay,
                Asr = zonedDateTime.PlusHours(15).TimeOfDay,
                Maghrib = zonedDateTime.PlusHours(18).TimeOfDay,
                Isha = zonedDateTime.PlusHours(20).TimeOfDay,
            };

            _semerkandApiServiceMock.GetTimesByCityID(
                    Arg.Is<LocalDate>(x => x == date || x == date.PlusDays(1)),
                    Arg.Is(1),
                    Arg.Any<CancellationToken>())
                .Returns([semerkandPrayerTimesDTO]);
            
            // ACT
            var calculationResult = await _semerkandPrayerTimeCalculator.GetPrayerTimesAsync(date, locationData, configurations, default);

            // ASSERT
            calculationResult.Should().NotBeNull().And.HaveCount(1);
            calculationResult.Should().HaveCount(1);
            calculationResult.First().Should().BeEquivalentTo((ETimeType.FajrEnd, shuruqZonedDateTime));

            _placeServiceMock.ReceivedCalls().Should().BeEmpty();
            _semerkandApiServiceMock.ReceivedCalls().Should().HaveCount(4);
            _semerkandDBAccessMock.ReceivedCalls().Should().HaveCount(9);
            await _semerkandApiServiceMock.Received(1).GetCountries(Arg.Any<CancellationToken>());
            await _semerkandApiServiceMock.Received(1).GetCitiesByCountryID(Arg.Is(1), Arg.Any<CancellationToken>());
            await _semerkandApiServiceMock.Received(1).GetTimesByCityID(Arg.Is(date), Arg.Is(1), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task GetPrayerTimesAsync_OnlyTimesFromApi_Success()
        {
            // ARRANGE
            var date = new LocalDate(2024, 1, 1);
            DateTimeZone dateTimeZone = DateTimeZoneProviders.Tzdb["Europe/Vienna"];
            ZonedDateTime zonedDateTime = date.AtStartOfDayInZone(dateTimeZone);

            var locationData = new SemerkandLocationData { CityName = "Berlin", CountryName = "Deutschland", TimezoneName = dateTimeZone.Id };
            List<GenericSettingConfiguration> configurations =
                [
                    new GenericSettingConfiguration { TimeType = ETimeType.FajrEnd, Source = ECalculationSource.Semerkand }
                ];

            _semerkandDBAccessMock.GetCountryIDByName(Arg.Is<string>("Deutschland"), Arg.Any<CancellationToken>()).Returns(1);
            _semerkandDBAccessMock.GetCityIDByName(Arg.Is(1), Arg.Is("Berlin"), Arg.Any<CancellationToken>()).Returns(1);
            _semerkandDBAccessMock.HasCountryData(Arg.Any<CancellationToken>()).Returns(true);

            var shuruqZonedDateTime1 = zonedDateTime.PlusHours(7);
            var semerkandPrayerTimesDTO1 = new SemerkandPrayerTimesResponseDTO
            {
                DayOfYear = 1,
                Fajr = zonedDateTime.PlusHours(5).TimeOfDay,
                Shuruq = shuruqZonedDateTime1.TimeOfDay,
                Dhuhr = zonedDateTime.PlusHours(12).TimeOfDay,
                Asr = zonedDateTime.PlusHours(15).TimeOfDay,
                Maghrib = zonedDateTime.PlusHours(18).TimeOfDay,
                Isha = zonedDateTime.PlusHours(20).TimeOfDay,
            };

            _semerkandApiServiceMock.GetTimesByCityID(
                    date: Arg.Is(date),
                    cityID: Arg.Is(1),
                    cancellationToken: Arg.Any<CancellationToken>())
                .Returns([semerkandPrayerTimesDTO1]);

            var semerkandPrayerTimesDTO2 = new SemerkandPrayerTimesResponseDTO
            {
                DayOfYear = 2,
                Fajr = zonedDateTime.PlusHours(5).TimeOfDay,
                Shuruq = zonedDateTime.PlusHours(7).TimeOfDay,
                Dhuhr = zonedDateTime.PlusHours(12).TimeOfDay,
                Asr = zonedDateTime.PlusHours(15).TimeOfDay,
                Maghrib = zonedDateTime.PlusHours(18).TimeOfDay,
                Isha = zonedDateTime.PlusHours(20).TimeOfDay,
            };

            _semerkandApiServiceMock.GetTimesByCityID(
                    date: Arg.Is(date.PlusDays(1)),
                    cityID: Arg.Is(1),
                    cancellationToken: Arg.Any<CancellationToken>())
                .Returns([semerkandPrayerTimesDTO2]);

            // ACT
            var calculationResult = await _semerkandPrayerTimeCalculator.GetPrayerTimesAsync(date, locationData, configurations, default);

            // ASSERT
            calculationResult.Should().NotBeNull().And.HaveCount(1);
            calculationResult.Should().HaveCount(1);
            calculationResult.First().Should().BeEquivalentTo((ETimeType.FajrEnd, shuruqZonedDateTime1));

            _placeServiceMock.ReceivedCalls().Should().BeEmpty();

            _semerkandDBAccessMock.ReceivedCalls().Should().HaveCount(6);
            await _semerkandDBAccessMock.Received(1).GetCountryIDByName(Arg.Is("Deutschland"), Arg.Any<CancellationToken>());
            await _semerkandDBAccessMock.Received(1).GetCityIDByName(Arg.Is(1), Arg.Is("Berlin"), Arg.Any<CancellationToken>());
            
            await _semerkandDBAccessMock.Received(1).GetTimesByDateAndCityID(Arg.Is(date), Arg.Is(1), Arg.Any<CancellationToken>());
            await _semerkandDBAccessMock.Received(1).InsertSemerkandPrayerTimes(Arg.Is(date), Arg.Is(1), Arg.Is<SemerkandPrayerTimes>(time => time.Date == date), Arg.Any<CancellationToken>());
            
            await _semerkandDBAccessMock.Received(1).GetTimesByDateAndCityID(Arg.Is(date.PlusDays(1)), Arg.Is(1), Arg.Any<CancellationToken>());
            await _semerkandDBAccessMock.Received(1).InsertSemerkandPrayerTimes(Arg.Is(date.PlusDays(1)), Arg.Is(1), Arg.Is<SemerkandPrayerTimes>(time => time.Date == date.PlusDays(1)), Arg.Any<CancellationToken>());

            _semerkandApiServiceMock.ReceivedCalls().Should().HaveCount(2);
            await _semerkandApiServiceMock.Received(0).GetCountries(Arg.Any<CancellationToken>());
            await _semerkandApiServiceMock.Received(0).GetCitiesByCountryID(Arg.Is(1), Arg.Any<CancellationToken>());
            await _semerkandApiServiceMock.Received(1).GetTimesByCityID(Arg.Is(date), Arg.Is(1), Arg.Any<CancellationToken>());
            await _semerkandApiServiceMock.Received(1).GetTimesByCityID(Arg.Is(date.PlusDays(1)), Arg.Is(1), Arg.Any<CancellationToken>());
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
                .GetPlaceBasedOnPlace(Arg.Is(completePlaceInfo), Arg.Is("tr"), Arg.Any<CancellationToken>())
                .Returns(turkishBasicPlaceInfo);
            _semerkandDBAccessMock.GetCountryIDByName(Arg.Is("Avusturya"), Arg.Any<CancellationToken>()).Returns(1);
            _semerkandDBAccessMock.GetCityIDByName(Arg.Is(1), Arg.Is("Innsbruck"), Arg.Any<CancellationToken>()).Returns(1);

            // ACT
            var locationData = await _semerkandPrayerTimeCalculator.GetLocationInfo(completePlaceInfo, default) as SemerkandLocationData;

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
