using Microsoft.EntityFrameworkCore;
using NodaTime;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimeProviders.Mosques.Mawaqit.Models.Entities;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimeProviders.Mosques.Mawaqit.Services;
using PrayerTimeEngine.Core.Tests.Common;

namespace PrayerTimeEngine.Core.Tests.Unit.Domain.DynamicPrayerTimeProviders.Mawaqit
{
    public class MawaqitDBAccessTests : BaseTest
    {
        private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
        private readonly MawaqitDBAccess _mawaqitDBAccess;

        public MawaqitDBAccessTests()
        {
            _dbContextFactory = GetHandledDbContextFactory();
            _mawaqitDBAccess = new MawaqitDBAccess(_dbContextFactory);
        }

        [Fact]
        public async Task GetPrayerTimesAsync_ExistingTime_ReturnsCorrectTime()
        {
            // ARRANGE
            var date = new LocalDate(2024, 8, 29);
            string externalID = "hamza-koln";

            var mawaqitTime = new MawaqitPrayerTimes
            {
                ID = 0,
                Date = new LocalDate(2024, 8, 29),
                ExternalID = externalID,
                Fajr = new LocalTime(05, 05, 00),
                FajrCongregation = new LocalTime(05, 35, 00),
                Shuruq = new LocalTime(06, 35, 00),
                Dhuhr = new LocalTime(13, 35, 00),
                DhuhrCongregation = new LocalTime(13, 45, 00),
                Asr = new LocalTime(17, 22, 00),
                AsrCongregation = new LocalTime(17, 32, 00),
                Maghrib = new LocalTime(20, 30, 00),
                MaghribCongregation = new LocalTime(20, 35, 00),
                Isha = new LocalTime(22, 06, 00),
                IshaCongregation = new LocalTime(22, 16, 00),
                Jumuah = new LocalTime(14, 30, 00),
                Jumuah2 = new LocalTime(15, 30, 00),
            };

            await TestArrangeDbContext.MawaqitPrayerTimes.AddAsync(mawaqitTime);
            await TestArrangeDbContext.SaveChangesAsync();

            // ACT
            var retrievedTime = await _mawaqitDBAccess.GetPrayerTimesAsync(mawaqitTime.Date, externalID, default);

            // ASSERT
            retrievedTime.Should().BeEquivalentTo(mawaqitTime);
        }

        [Fact]
        public async Task InsertPrayerTimesAsync_NewTime_TimeInDb()
        {
            // ARRANGE
            var date = new LocalDate(2024, 8, 29);
            string externalID = "hamza-koln";

            var newMawaqitTime = new MawaqitPrayerTimes
            {
                Date = new LocalDate(2024, 8, 29),
                ExternalID = externalID,
                Fajr = new LocalTime(05, 05, 00),
                FajrCongregation = new LocalTime(05, 35, 00),
                Shuruq = new LocalTime(06, 35, 00),
                Dhuhr = new LocalTime(13, 35, 00),
                DhuhrCongregation = new LocalTime(13, 45, 00),
                Asr = new LocalTime(17, 22, 00),
                AsrCongregation = new LocalTime(17, 32, 00),
                Maghrib = new LocalTime(20, 30, 00),
                MaghribCongregation = new LocalTime(20, 35, 00),
                Isha = new LocalTime(22, 06, 00),
                IshaCongregation = new LocalTime(22, 16, 00),
                Jumuah = new LocalTime(14, 30, 00),
                Jumuah2 = new LocalTime(15, 30, 00),
            };

            // ACT
            await _mawaqitDBAccess.InsertPrayerTimesAsync([newMawaqitTime], default);

            // ASSERT
            var insertedTime = await TestAssertDbContext.MawaqitPrayerTimes.FindAsync(newMawaqitTime.ID);
            insertedTime.Should().BeEquivalentTo(newMawaqitTime);
        }
    }
}
