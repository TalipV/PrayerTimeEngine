using Microsoft.EntityFrameworkCore;
using NodaTime;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.Mawaqit.Models.Entities;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.Mawaqit.Services;
using PrayerTimeEngine.Core.Tests.Common;

namespace PrayerTimeEngine.Core.Tests.Unit.Domain.MosquePrayerTimes.Providers.Mawaqit;

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

        var mawaqitTime = new MawaqitMosqueDailyPrayerTimes
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

        var newMawaqitTime = new MawaqitMosqueDailyPrayerTimes
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
        await _mawaqitDBAccess.InsertPrayerTimesAsync([newMawaqitTime], default);

        // ASSERT
        var insertedTime = await TestAssertDbContext.MawaqitPrayerTimes.FindAsync(newMawaqitTime.ID);
        insertedTime.Should().BeEquivalentTo(newMawaqitTime);
    }

    [Fact]
    public async Task DeleteCacheDataAsync_RemoveOlderEntries_KeepNewerOnes()
    {
        // ARRANGE
        var baseDate = new LocalDate(2023, 1, 1).AtStartOfDayInZone(DateTimeZone.Utc);
        ZonedDateTime oldDate = baseDate.Minus(Duration.FromDays(5));
        ZonedDateTime newDate = baseDate.Plus(Duration.FromDays(1));

        var oldTime = new MawaqitMosqueDailyPrayerTimes
        {
            Date = oldDate.Date,
            ExternalID = "1", Fajr = oldDate.LocalDateTime.TimeOfDay, FajrCongregation = oldDate.LocalDateTime.TimeOfDay, 
            Shuruq = oldDate.LocalDateTime.TimeOfDay, Dhuhr = oldDate.LocalDateTime.TimeOfDay, 
            DhuhrCongregation = oldDate.LocalDateTime.TimeOfDay, Asr = oldDate.LocalDateTime.TimeOfDay, 
            AsrCongregation = oldDate.LocalDateTime.TimeOfDay, Maghrib = oldDate.LocalDateTime.TimeOfDay, 
            MaghribCongregation = oldDate.LocalDateTime.TimeOfDay, Isha = oldDate.LocalDateTime.TimeOfDay, 
            IshaCongregation = oldDate.LocalDateTime.TimeOfDay, Jumuah = oldDate.LocalDateTime.TimeOfDay, 
            Jumuah2 = oldDate.LocalDateTime.TimeOfDay,
        };
        var newTime = new MawaqitMosqueDailyPrayerTimes
        {
            Date = newDate.Date,
            ExternalID = "1",
            Fajr = newDate.LocalDateTime.TimeOfDay, FajrCongregation = newDate.LocalDateTime.TimeOfDay,
            Shuruq = newDate.LocalDateTime.TimeOfDay, Dhuhr = newDate.LocalDateTime.TimeOfDay,
            DhuhrCongregation = newDate.LocalDateTime.TimeOfDay, Asr = newDate.LocalDateTime.TimeOfDay,
            AsrCongregation = newDate.LocalDateTime.TimeOfDay, Maghrib = newDate.LocalDateTime.TimeOfDay,
            MaghribCongregation = newDate.LocalDateTime.TimeOfDay,Isha = newDate.LocalDateTime.TimeOfDay,
            IshaCongregation = newDate.LocalDateTime.TimeOfDay, Jumuah = newDate.LocalDateTime.TimeOfDay, 
            Jumuah2 = newDate.LocalDateTime.TimeOfDay,
        };

        await TestArrangeDbContext.MawaqitPrayerTimes.AddRangeAsync(oldTime, newTime);
        await TestArrangeDbContext.SaveChangesAsync();

        (await TestArrangeDbContext.MawaqitPrayerTimes.FindAsync(oldTime.ID)).Should().NotBeNull();
        (await TestArrangeDbContext.MawaqitPrayerTimes.FindAsync(newTime.ID)).Should().NotBeNull();

        // ACT
        await _mawaqitDBAccess.DeleteCacheDataAsync(baseDate, default);

        // ASSERT
        (await TestAssertDbContext.MawaqitPrayerTimes.FindAsync(oldTime.ID)).Should().BeNull();
        (await TestAssertDbContext.MawaqitPrayerTimes.FindAsync(newTime.ID)).Should().NotBeNull();
    }
}
