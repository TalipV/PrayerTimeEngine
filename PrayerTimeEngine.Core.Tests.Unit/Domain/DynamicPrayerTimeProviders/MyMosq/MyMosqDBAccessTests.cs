using Microsoft.EntityFrameworkCore;
using NodaTime;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.MyMosq.Models.Entities;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.MyMosq.Services;
using PrayerTimeEngine.Core.Tests.Common;

namespace PrayerTimeEngine.Core.Tests.Unit.Domain.DynamicPrayerTimeProviders.MyMosq
{
    public class MyMosqDBAccessTests : BaseTest
    {
        private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
        private readonly MyMosqDBAccess _myMosqDBAccess;

        public MyMosqDBAccessTests()
        {
            _dbContextFactory = GetHandledDbContextFactory();
            _myMosqDBAccess = new MyMosqDBAccess(_dbContextFactory);
        }

        [Fact]
        public async Task GetPrayerTimesAsync_ExistingTime_ReturnsCorrectTime()
        {
            // ARRANGE
            var date = new LocalDate(2024, 8, 30);
            string externalID = "1239";

            var myMosqTime = new MyMosqPrayerTimes
            {
                ID = 0,
                Date = date,
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

            await TestArrangeDbContext.MyMosqPrayerTimes.AddAsync(myMosqTime);
            await TestArrangeDbContext.SaveChangesAsync();

            // ACT
            var retrievedTime = await _myMosqDBAccess.GetPrayerTimesAsync(myMosqTime.Date, externalID, default);

            // ASSERT
            retrievedTime.Should().BeEquivalentTo(myMosqTime);
        }

        [Fact]
        public async Task InsertPrayerTimesAsync_NewTime_TimeInDb()
        {
            // ARRANGE
            var date = new LocalDate(2024, 8, 30);
            string externalID = "1239";

            var newMyMosqTime = new MyMosqPrayerTimes
            {
                Date = date,
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
            await _myMosqDBAccess.InsertPrayerTimesAsync([newMyMosqTime], default);

            // ASSERT
            var insertedTime = await TestAssertDbContext.MyMosqPrayerTimes.FindAsync(newMyMosqTime.ID);
            insertedTime.Should().BeEquivalentTo(newMyMosqTime);
        }
    }
}
