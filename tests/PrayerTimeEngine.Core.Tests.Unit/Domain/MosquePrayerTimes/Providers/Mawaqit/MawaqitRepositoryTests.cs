using Microsoft.EntityFrameworkCore;
using NodaTime;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.Mawaqit.Models.Entities;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.Mawaqit.Services;
using PrayerTimeEngine.Core.Tests.Common;

namespace PrayerTimeEngine.Core.Tests.Unit.Domain.MosquePrayerTimes.Providers.Mawaqit;

public class MawaqitRepositoryTests : BaseTest
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    private readonly MawaqitRepository _mawaqitRepository;

    public MawaqitRepositoryTests()
    {
        _dbContextFactory = GetHandledDbContextFactory();
        _mawaqitRepository = new MawaqitRepository(_dbContextFactory);
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
        MawaqitMosqueDailyPrayerTimes retrievedTime = await _mawaqitRepository.GetPrayerTimesAsync(mawaqitTime.Date, externalID, default);

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
        await _mawaqitRepository.InsertPrayerTimesAsync([newMawaqitTime], default);

        // ASSERT
        MawaqitMosqueDailyPrayerTimes? insertedTime = await TestAssertDbContext.MawaqitPrayerTimes.FindAsync(newMawaqitTime.ID);
        insertedTime.Should().BeEquivalentTo(newMawaqitTime);
    }

    [Fact]
    public async Task InsertPrayerTimesAsync_DaysAlreadyStored_OnlyOverlappingDaysOfSameMosqueReplaced()
    {
        // ARRANGE
        var date = new LocalDate(2024, 8, 1);

        static MawaqitMosqueDailyPrayerTimes Create(string externalID, LocalDate day, int fajrHour) =>
            new()
            {
                Date = day,
                ExternalID = externalID,
                Fajr = new LocalTime(fajrHour, 0),
                FajrCongregation = new LocalTime(fajrHour, 30),
                Shuruq = new LocalTime(7, 0),
                Dhuhr = new LocalTime(13, 0),
                DhuhrCongregation = new LocalTime(13, 30),
                Asr = new LocalTime(17, 0),
                AsrCongregation = new LocalTime(17, 30),
                Maghrib = new LocalTime(20, 0),
                MaghribCongregation = new LocalTime(20, 30),
                Isha = new LocalTime(22, 0),
                IshaCongregation = new LocalTime(22, 30),
                Jumuah = new LocalTime(14, 0),
                Jumuah2 = null,
            };

        MawaqitMosqueDailyPrayerTimes before = Create("a", date, 4);
        MawaqitMosqueDailyPrayerTimes overlapping = Create("a", date.PlusDays(1), 4);
        MawaqitMosqueDailyPrayerTimes after = Create("a", date.PlusDays(3), 4);
        MawaqitMosqueDailyPrayerTimes otherMosque = Create("b", date.PlusDays(1), 4);

        await TestArrangeDbContext.MawaqitPrayerTimes.AddRangeAsync(before, overlapping, after, otherMosque);
        await TestArrangeDbContext.SaveChangesAsync();

        MawaqitMosqueDailyPrayerTimes newOverlapping = Create("a", date.PlusDays(1), 5);
        MawaqitMosqueDailyPrayerTimes newNext = Create("a", date.PlusDays(2), 5);

        // ACT
        await _mawaqitRepository.InsertPrayerTimesAsync([newOverlapping, newNext], default);

        // ASSERT
        List<MawaqitMosqueDailyPrayerTimes> stored = await TestAssertDbContext.MawaqitPrayerTimes.ToListAsync();
        stored.Select(x => x.ID).Should().BeEquivalentTo([before.ID, after.ID, otherMosque.ID, newOverlapping.ID, newNext.ID]);
        stored.Single(x => x.ExternalID == "a" && x.Date == date.PlusDays(1)).Fajr.Should().Be(new LocalTime(5, 0));
    }

    [Fact]
    public async Task DeleteCacheDataAsync_EntriesStoredBeforeInstant_DeletedRegardlessOfTheirDay()
    {
        // ARRANGE
        ZonedDateTime baseDate = new LocalDate(2023, 1, 1).AtStartOfDayInZone(DateTimeZone.Utc);
        ZonedDateTime oldDate = baseDate.Minus(Duration.FromDays(5));
        ZonedDateTime newDate = baseDate.Plus(Duration.FromDays(1));

        var oldTime = new MawaqitMosqueDailyPrayerTimes
        {
            Date = oldDate.Date,
            ExternalID = "1",
            Fajr = oldDate.LocalDateTime.TimeOfDay,
            FajrCongregation = oldDate.LocalDateTime.TimeOfDay,
            Shuruq = oldDate.LocalDateTime.TimeOfDay,
            Dhuhr = oldDate.LocalDateTime.TimeOfDay,
            DhuhrCongregation = oldDate.LocalDateTime.TimeOfDay,
            Asr = oldDate.LocalDateTime.TimeOfDay,
            AsrCongregation = oldDate.LocalDateTime.TimeOfDay,
            Maghrib = oldDate.LocalDateTime.TimeOfDay,
            MaghribCongregation = oldDate.LocalDateTime.TimeOfDay,
            Isha = oldDate.LocalDateTime.TimeOfDay,
            IshaCongregation = oldDate.LocalDateTime.TimeOfDay,
            Jumuah = oldDate.LocalDateTime.TimeOfDay,
            Jumuah2 = oldDate.LocalDateTime.TimeOfDay,
        };
        var newTime = new MawaqitMosqueDailyPrayerTimes
        {
            Date = newDate.Date,
            ExternalID = "1",
            Fajr = newDate.LocalDateTime.TimeOfDay,
            FajrCongregation = newDate.LocalDateTime.TimeOfDay,
            Shuruq = newDate.LocalDateTime.TimeOfDay,
            Dhuhr = newDate.LocalDateTime.TimeOfDay,
            DhuhrCongregation = newDate.LocalDateTime.TimeOfDay,
            Asr = newDate.LocalDateTime.TimeOfDay,
            AsrCongregation = newDate.LocalDateTime.TimeOfDay,
            Maghrib = newDate.LocalDateTime.TimeOfDay,
            MaghribCongregation = newDate.LocalDateTime.TimeOfDay,
            Isha = newDate.LocalDateTime.TimeOfDay,
            IshaCongregation = newDate.LocalDateTime.TimeOfDay,
            Jumuah = newDate.LocalDateTime.TimeOfDay,
            Jumuah2 = newDate.LocalDateTime.TimeOfDay,
        };

        await TestArrangeDbContext.MawaqitPrayerTimes.AddRangeAsync(oldTime, newTime);
        await TestArrangeDbContext.SaveChangesAsync();

        // the day of the times doesn't matter, only when they were stored: the times of the
        // earlier day were stored recently, the ones of the later day long ago
        var insertedBefore = Instant.FromUtc(2024, 1, 1, 0, 0);
        oldTime.InsertInstant = insertedBefore.Plus(Duration.FromHours(1));
        newTime.InsertInstant = insertedBefore.Minus(Duration.FromDays(1));
        await TestArrangeDbContext.SaveChangesAsync();

        (await TestArrangeDbContext.MawaqitPrayerTimes.FindAsync(oldTime.ID)).Should().NotBeNull();
        (await TestArrangeDbContext.MawaqitPrayerTimes.FindAsync(newTime.ID)).Should().NotBeNull();

        // ACT
        await _mawaqitRepository.DeleteCacheDataAsync(insertedBefore, default);

        // ASSERT
        (await TestAssertDbContext.MawaqitPrayerTimes.FindAsync(oldTime.ID)).Should().NotBeNull();
        (await TestAssertDbContext.MawaqitPrayerTimes.FindAsync(newTime.ID)).Should().BeNull();
    }
}
