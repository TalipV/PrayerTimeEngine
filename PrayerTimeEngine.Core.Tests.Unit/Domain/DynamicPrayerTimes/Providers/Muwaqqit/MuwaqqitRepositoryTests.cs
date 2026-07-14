using Microsoft.EntityFrameworkCore;
using NodaTime;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Muwaqqit.Models.Entities;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Muwaqqit.Services;
using PrayerTimeEngine.Core.Tests.Common;
using PrayerTimeEngine.Core.Tests.Common.TestData;

namespace PrayerTimeEngine.Core.Tests.Unit.Domain.DynamicPrayerTimes.Providers.Muwaqqit;

public class MuwaqqitRepositoryTests : BaseTest
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    private readonly MuwaqqitRepository _muwaqqitRepository;

    public MuwaqqitRepositoryTests()
    {
        _dbContextFactory = GetHandledDbContextFactory();
        _muwaqqitRepository = new MuwaqqitRepository(_dbContextFactory);
    }

    [Fact]
    public async Task GetPrayerTimesAsync_ExistingTime_ReturnsCorrectTime()
    {
        // ARRANGE
        var date = new LocalDate(2023, 7, 30);
        var dateTimeZone = TestDataHelper.EUROPE_VIENNA_TIME_ZONE;
        var muwaqqitTime = new MuwaqqitDailyPrayerTimes
        {
            Date = date,
            TimeZone = dateTimeZone,
            Latitude = 47.2803835M,
            Longitude = 11.41337M,
            FajrDegree = 1,
            IshaDegree = 1,
            IshtibaqDegree = 1,
            AsrKarahaDegree = 1,
            Fajr = instant(2023, 7, 30, 2, 27, 04),
            NextFajr = instant(2023, 7, 31, 2, 28, 04),
            Shuruq = instant(2023, 7, 30, 3, 49, 53),
            Duha = instant(2023, 7, 30, 4, 49, 53),
            Dhuhr = instant(2023, 7, 30, 11, 21, 22),
            Asr = instant(2023, 7, 30, 15, 25, 53),
            AsrMithlayn = instant(2023, 7, 30, 16, 25, 53),
            AsrKaraha = instant(2023, 7, 30, 17, 25, 53),
            Maghrib = instant(2023, 7, 30, 18, 50, 59),
            Ishtibaq = instant(2023, 7, 30, 19, 50, 59),
            Isha = instant(2023, 7, 30, 20, 13, 17)
        };

        await TestArrangeDbContext.MuwaqqitPrayerTimes.AddAsync(muwaqqitTime);
        await TestArrangeDbContext.SaveChangesAsync();

        // ACT
        var retrievedTime = await _muwaqqitRepository.GetPrayerTimesAsync(
            muwaqqitTime.Date,
            muwaqqitTime.Longitude,
            muwaqqitTime.Latitude,
            muwaqqitTime.FajrDegree,
            muwaqqitTime.IshaDegree,
            muwaqqitTime.IshtibaqDegree,
            muwaqqitTime.AsrKarahaDegree,
            default);

        // ASSERT
        retrievedTime.Should().BeEquivalentTo(muwaqqitTime, options => options.IgnoringCyclicReferences());
    }

    [Fact]
    public async Task InsertPrayerTimesAsync_NewTime_TimeInDb()
    {
        // ARRANGE
        var date = new LocalDate(2023, 7, 31);
        var dateTimeZone = TestDataHelper.EUROPE_VIENNA_TIME_ZONE;
        var newMuwaqqitTime = new MuwaqqitDailyPrayerTimes
        {
            Date = date,
            TimeZone = dateTimeZone,
            Latitude = 47.2803835M,
            Longitude = 11.41337M,
            FajrDegree = 1,
            IshaDegree = 1,
            IshtibaqDegree = 1,
            AsrKarahaDegree = 1,
            Fajr = instant(2023, 7, 30, 2, 27, 04),
            NextFajr = instant(2023, 7, 31, 2, 28, 04),
            Shuruq = instant(2023, 7, 30, 3, 49, 53),
            Duha = instant(2023, 7, 30, 4, 49, 53),
            Dhuhr = instant(2023, 7, 30, 11, 21, 22),
            Asr = instant(2023, 7, 30, 15, 25, 53),
            AsrMithlayn = instant(2023, 7, 30, 16, 25, 53),
            AsrKaraha = instant(2023, 7, 30, 17, 25, 53),
            Maghrib = instant(2023, 7, 30, 18, 50, 59),
            Ishtibaq = instant(2023, 7, 30, 19, 50, 59),
            Isha = instant(2023, 7, 30, 20, 13, 17)
        };

        // ACT
        await _muwaqqitRepository.InsertPrayerTimesAsync([newMuwaqqitTime], default);

        // ASSERT
        var insertedTime = await TestAssertDbContext.MuwaqqitPrayerTimes.FindAsync(newMuwaqqitTime.ID);
        insertedTime.Should().BeEquivalentTo(newMuwaqqitTime, options => options.IgnoringCyclicReferences());
    }

    [Fact]
    public async Task DeleteCacheDataAsync_RemoveOlderEntries_KeepNewerOnes()
    {
        // ARRANGE
        var baseDate = new LocalDate(2023, 1, 1).AtStartOfDayInZone(DateTimeZone.Utc);
        ZonedDateTime oldDate = baseDate.Minus(Duration.FromDays(5));
        ZonedDateTime newDate = baseDate.Plus(Duration.FromDays(1));

        var oldTime = new MuwaqqitDailyPrayerTimes
        {
            Date = oldDate.Date,
            TimeZone = DateTimeZone.Utc,
            Latitude = 1, Longitude = 1, FajrDegree = 1, IshaDegree = 1, IshtibaqDegree = 1, AsrKarahaDegree = 1,
            Fajr = oldDate.ToInstant(), NextFajr = oldDate.ToInstant(), Shuruq = oldDate.ToInstant(), Duha = oldDate.ToInstant(), Dhuhr = oldDate.ToInstant(), Asr = oldDate.ToInstant(),
            AsrMithlayn = oldDate.ToInstant(), AsrKaraha = oldDate.ToInstant(), Maghrib = oldDate.ToInstant(), Ishtibaq = oldDate.ToInstant(), Isha = oldDate.ToInstant(),
        };
        var newTime = new MuwaqqitDailyPrayerTimes
        {
            Date = newDate.Date,
            TimeZone = DateTimeZone.Utc,
            Latitude = 1, Longitude = 1, FajrDegree = 1, IshaDegree = 1, IshtibaqDegree = 1, AsrKarahaDegree = 1,
            Fajr = newDate.ToInstant(), NextFajr = newDate.ToInstant(), Shuruq = newDate.ToInstant(), Duha = newDate.ToInstant(), Dhuhr = newDate.ToInstant(), Asr = newDate.ToInstant(),
            AsrMithlayn = newDate.ToInstant(), AsrKaraha = newDate.ToInstant(), Maghrib = newDate.ToInstant(), Ishtibaq = newDate.ToInstant(), Isha = newDate.ToInstant(),
        };

        await TestArrangeDbContext.MuwaqqitPrayerTimes.AddRangeAsync(oldTime, newTime);
        await TestArrangeDbContext.SaveChangesAsync();

        (await TestArrangeDbContext.MuwaqqitPrayerTimes.FindAsync(oldTime.ID)).Should().NotBeNull();
        (await TestArrangeDbContext.MuwaqqitPrayerTimes.FindAsync(newTime.ID)).Should().NotBeNull();

        // ACT
        await _muwaqqitRepository.DeleteCacheDataAsync(baseDate.Date, default);

        // ASSERT
        (await TestAssertDbContext.MuwaqqitPrayerTimes.FindAsync(oldTime.ID)).Should().BeNull();
        (await TestAssertDbContext.MuwaqqitPrayerTimes.FindAsync(newTime.ID)).Should().NotBeNull();
    }

    private static Instant instant(int year, int month, int day, int hour, int minute, int second)
    {
        return Instant.FromUtc(year, month, day, hour, minute, second);
    }
}
