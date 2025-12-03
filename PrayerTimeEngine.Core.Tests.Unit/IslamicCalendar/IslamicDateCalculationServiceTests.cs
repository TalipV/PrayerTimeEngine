using NodaTime;
using NSubstitute;
using PrayerTimeEngine.Core.Common;
using PrayerTimeEngine.Core.Domain.ConfigurationManagement;
using PrayerTimeEngine.Core.Domain.IslamicCalendar.Services;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;
using PrayerTimeEngine.Core.Tests.Common.TestData;
using System;
using System.Collections.Generic;
using System.Text;

namespace PrayerTimeEngine.Core.Tests.Unit.IslamicCalendar
{
    public class IslamicDateCalculationServiceTests
    {
        private readonly ISystemInfoService _systemInfoServiceMock;
        private readonly IslamicDateCalculationService _islamicDateCalculationService;

        public IslamicDateCalculationServiceTests()
        {
            _systemInfoServiceMock = Substitute.For<ISystemInfoService>();
            _islamicDateCalculationService = new IslamicDateCalculationService(_systemInfoServiceMock);
        }

        #region GetWeeksUntilRamadan

        public static TheoryData<LocalDate, int> GetWeeksUntilRamadan_Data =>
            new()
            {
                // 58 days → 8.285 weeks → 8
                { new LocalDate(2025, 1, 1), 8 },

                // 13 days → 1.857 weeks → 2
                { new LocalDate(2025, 2, 15), 2 },

                // First of Ramadan → 0 days → 0 weeks
                { new LocalDate(2025, 3, 1), 0 },

                // 353 days → 50.4 weeks → 50
                { new LocalDate(2025, 3, 2), 50 },
            };

        [Theory]
        [Trait("Method", "GetWeeksUntilRamadan")]
        [MemberData(nameof(GetWeeksUntilRamadan_Data))]
        public void GetWeeksUntilRamadan_DataDrivenTests(LocalDate today, int expectedWeeks)
        {
            // ARRANGE
            var zoned = today.AtStartOfDayInZone(DateTimeZone.Utc);
            _systemInfoServiceMock.GetCurrentZonedDateTime().Returns(zoned);

            // ACT
            int result = _islamicDateCalculationService.GetWeeksUntilRamadan();

            // ASSERT
            result.Should().Be(expectedWeeks);
        }

        #endregion GetWeeksUntilRamadan

        #region GetWeeksUntilHajj

        public static TheoryData<LocalDate, int> GetWeeksUntilHajj_Data =>
            new()
            {
                // 148 days → ~21.14 weeks → 21
                { new LocalDate(2024, 12, 31), 21 },

                // 153 days → ~21.86 weeks → 22
                { new LocalDate(2024, 12, 26), 22 },
                
                // First of Dhul Hijjah → 0 days → 0 weeks
                { new LocalDate(2025, 5, 28), 0 },
                
                // 354 days → ~50.57 weeks → 51
                { new LocalDate(2025, 5, 29), 51 },
            };

        [Theory]
        [Trait("Method", "GetWeeksUntilHajj")]
        [MemberData(nameof(GetWeeksUntilHajj_Data))]
        public void GetWeeksUntilHajj_DataDrivenTests(LocalDate today, int expectedWeeks)
        {
            // ARRANGE
            var zoned = today.AtStartOfDayInZone(DateTimeZone.Utc);
            _systemInfoServiceMock.GetCurrentZonedDateTime().Returns(zoned);

            // ACT
            int result = _islamicDateCalculationService.GetWeeksUntilHajj();

            // ASSERT
            result.Should().Be(expectedWeeks);
        }

        #endregion GetWeeksUntilHajj
    }
}
