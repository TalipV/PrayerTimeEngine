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
            Fajr = localDateTime(2023, 7, 30, 2, 27, 04, dateTimeZone),
            NextFajr = localDateTime(2023, 7, 31, 2, 28, 04, dateTimeZone),
            Shuruq = localDateTime(2023, 7, 30, 3, 49, 53, dateTimeZone),
            Duha = localDateTime(2023, 7, 30, 4, 49, 53, dateTimeZone),
            Dhuhr = localDateTime(2023, 7, 30, 11, 21, 22, dateTimeZone),
            Asr = localDateTime(2023, 7, 30, 15, 25, 53, dateTimeZone),
            AsrMithlayn = localDateTime(2023, 7, 30, 16, 25, 53, dateTimeZone),
            AsrKaraha = localDateTime(2023, 7, 30, 17, 25, 53, dateTimeZone),
            Maghrib = localDateTime(2023, 7, 30, 18, 50, 59, dateTimeZone),
            Ishtibaq = localDateTime(2023, 7, 30, 19, 50, 59, dateTimeZone),
            Isha = localDateTime(2023, 7, 30, 20, 13, 17, dateTimeZone)
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
            Fajr = localDateTime(2023, 7, 30, 2, 27, 04, dateTimeZone),
            NextFajr = localDateTime(2023, 7, 31, 2, 28, 04, dateTimeZone),
            Shuruq = localDateTime(2023, 7, 30, 3, 49, 53, dateTimeZone),
            Duha = localDateTime(2023, 7, 30, 4, 49, 53, dateTimeZone),
            Dhuhr = localDateTime(2023, 7, 30, 11, 21, 22, dateTimeZone),
            Asr = localDateTime(2023, 7, 30, 15, 25, 53, dateTimeZone),
            AsrMithlayn = localDateTime(2023, 7, 30, 16, 25, 53, dateTimeZone),
            AsrKaraha = localDateTime(2023, 7, 30, 17, 25, 53, dateTimeZone),
            Maghrib = localDateTime(2023, 7, 30, 18, 50, 59, dateTimeZone),
            Ishtibaq = localDateTime(2023, 7, 30, 19, 50, 59, dateTimeZone),
            Isha = localDateTime(2023, 7, 30, 20, 13, 17, dateTimeZone)
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
            Fajr = oldDate.LocalDateTime, NextFajr = oldDate.LocalDateTime, Shuruq = oldDate.LocalDateTime, Duha = oldDate.LocalDateTime, Dhuhr = oldDate.LocalDateTime, Asr = oldDate.LocalDateTime,
            AsrMithlayn = oldDate.LocalDateTime, AsrKaraha = oldDate.LocalDateTime, Maghrib = oldDate.LocalDateTime, Ishtibaq = oldDate.LocalDateTime, Isha = oldDate.LocalDateTime,
        };
        var newTime = new MuwaqqitDailyPrayerTimes
        {
            Date = newDate.Date,
            TimeZone = DateTimeZone.Utc,
            Latitude = 1, Longitude = 1, FajrDegree = 1, IshaDegree = 1, IshtibaqDegree = 1, AsrKarahaDegree = 1,
            Fajr = newDate.LocalDateTime, NextFajr = newDate.LocalDateTime, Shuruq = newDate.LocalDateTime, Duha = newDate.LocalDateTime, Dhuhr = newDate.LocalDateTime, Asr = newDate.LocalDateTime,
            AsrMithlayn = newDate.LocalDateTime, AsrKaraha = newDate.LocalDateTime, Maghrib = newDate.LocalDateTime, Ishtibaq = newDate.LocalDateTime, Isha = newDate.LocalDateTime,
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
    
    private static LocalDateTime localDateTime(int year, int month, int day, int hour, int minute, int second, DateTimeZone zone)
    {
        return Instant.FromUtc(year, month, day, hour, minute, second).InZone(zone).LocalDateTime;
    }
}
