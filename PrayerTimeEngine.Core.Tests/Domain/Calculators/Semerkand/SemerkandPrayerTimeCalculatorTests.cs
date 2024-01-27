using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using NodaTime;
using NSubstitute;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Models;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Services;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace PrayerTimeEngine.Core.Tests.Unit.Domain.Calculators.Semerkand
{
    public class SemerkandPrayerTimeCalculatorTests
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
        public async Task GetPrayerTimesAsync_X_X()
        {
            // ARRANGE
            LocalDate date = new LocalDate(2024, 1, 1);
            BaseLocationData locationData = new SemerkandLocationData { CityName = "Innsbruck", CountryName = "Österreich", TimezoneName = "Europe/Vienna" };
            List<GenericSettingConfiguration> configurations = [];

            // ACT
            var calculationResult = await _semerkandPrayerTimeCalculator.GetPrayerTimesAsync(date, locationData, configurations);

            // ASSERT

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

            // ACT
            var locationData = await _semerkandPrayerTimeCalculator.GetLocationInfo(completePlaceInfo);

            // ASSERT

        }

        #endregion GetLocationInfo
    }
}
