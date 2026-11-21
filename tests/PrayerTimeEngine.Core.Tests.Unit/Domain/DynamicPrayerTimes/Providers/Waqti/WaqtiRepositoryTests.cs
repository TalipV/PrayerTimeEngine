using Microsoft.EntityFrameworkCore;
using NodaTime;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Waqti.Models.Entities;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Waqti.Services;
using PrayerTimeEngine.Core.Tests.Common;
using PrayerTimeEngine.Core.Tests.Common.TestData;

namespace PrayerTimeEngine.Core.Tests.Unit.Domain.DynamicPrayerTimes.Providers.Waqti;

public class WaqtiRepositoryTests : BaseTest
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    private readonly WaqtiRepository _waqtiRepository;

    public WaqtiRepositoryTests()
    {
        _dbContextFactory = GetHandledDbContextFactory();
        _waqtiRepository = new WaqtiRepository(_dbContextFactory);
    }

    private static WaqtiDailyPrayerTimes CreatePrayerTimes(LocalDate date, string method = "Mwl2007", DateTimeZone? timeZone = null, double? fajrAngle = null, double? ishaaAngle = null)
    {
        return new WaqtiDailyPrayerTimes
        {
            Date = date,
            TimeZone = timeZone ?? TestDataHelper.EUROPE_VIENNA_TIME_ZONE,
            Latitude = 47.2692124M,
            Longitude = 11.4041024M,
            Method = method,
            FajrAngle = fajrAngle,
            IshaaAngle = ishaaAngle,
            Fajr = Instant(date, 1, 28),
            Shuruq = Instant(date, 3, 47),
            Duha = Instant(date, 4, 24),
            Dhuhr = Instant(date, 11, 25),
            Asr = Instant(date, 15, 29),
            AsrMithlayn = Instant(date, 16, 37),
            Maghrib = Instant(date, 18, 53),
            Isha = Instant(date, 21, 1),
            NextFajr = Instant(date.PlusDays(1), 1, 30),
        };
    }

    [Fact]
    public async Task GetPrayerTimesAsync_ExistingTime_ReturnsCorrectTime()
    {
        // ARRANGE
        WaqtiDailyPrayerTimes waqtiTime = CreatePrayerTimes(new LocalDate(2023, 7, 30));

        await TestArrangeDbContext.WaqtiPrayerTimes.AddAsync(waqtiTime);
        await TestArrangeDbContext.SaveChangesAsync();

        // ACT
        WaqtiDailyPrayerTimes? retrievedTime = await _waqtiRepository.GetPrayerTimesAsync(
            waqtiTime.Date,
            waqtiTime.Longitude,
            waqtiTime.Latitude,
            waqtiTime.Method,
            waqtiTime.FajrAngle,
            waqtiTime.IshaaAngle,
            default);

        // ASSERT
        retrievedTime.Should().BeEquivalentTo(waqtiTime, options => options.IgnoringCyclicReferences());
    }

    [Fact]
    public async Task GetPrayerTimesAsync_OtherMethodStored_ReturnsNull()
    {
        // ARRANGE
        WaqtiDailyPrayerTimes waqtiTime = CreatePrayerTimes(new LocalDate(2023, 7, 30), method: "Mwl2007");

        await TestArrangeDbContext.WaqtiPrayerTimes.AddAsync(waqtiTime);
        await TestArrangeDbContext.SaveChangesAsync();

        // ACT
        WaqtiDailyPrayerTimes? retrievedTime = await _waqtiRepository.GetPrayerTimesAsync(
            waqtiTime.Date,
            waqtiTime.Longitude,
            waqtiTime.Latitude,
            "Diyanet",
            fajrAngle: null,
            ishaaAngle: null,
            default);

        // ASSERT
        retrievedTime.Should().BeNull();
    }

    [Fact]
    public async Task GetPrayerTimesAsync_OtherLocationStored_ReturnsNull()
    {
        // ARRANGE
        WaqtiDailyPrayerTimes waqtiTime = CreatePrayerTimes(new LocalDate(2023, 7, 30));

        await TestArrangeDbContext.WaqtiPrayerTimes.AddAsync(waqtiTime);
        await TestArrangeDbContext.SaveChangesAsync();

        // ACT
        WaqtiDailyPrayerTimes? retrievedTime = await _waqtiRepository.GetPrayerTimesAsync(
            waqtiTime.Date,
            longitude: 6.9578M,
            latitude: 50.9413M,
            waqtiTime.Method,
            waqtiTime.FajrAngle,
            waqtiTime.IshaaAngle,
            default);

        // ASSERT
        retrievedTime.Should().BeNull();
    }

    [Fact]
    public async Task GetPrayerTimesAsync_WithAngleOverrides_ReturnsOnlyEntryWithTheSameAngles()
    {
        // ARRANGE
        var date = new LocalDate(2023, 7, 30);
        WaqtiDailyPrayerTimes regular = CreatePrayerTimes(date);
        WaqtiDailyPrayerTimes derived = CreatePrayerTimes(date, fajrAngle: 7.5, ishaaAngle: 10);
        WaqtiDailyPrayerTimes onlyFajrDerived = CreatePrayerTimes(date, fajrAngle: 7.5);

        await TestArrangeDbContext.WaqtiPrayerTimes.AddRangeAsync(regular, derived, onlyFajrDerived);
        await TestArrangeDbContext.SaveChangesAsync();

        // ACT
        WaqtiDailyPrayerTimes? retrievedRegular = await _waqtiRepository.GetPrayerTimesAsync(date, regular.Longitude, regular.Latitude, "Mwl2007", null, null, default);
        WaqtiDailyPrayerTimes? retrievedDerived = await _waqtiRepository.GetPrayerTimesAsync(date, regular.Longitude, regular.Latitude, "Mwl2007", 7.5, 10, default);
        WaqtiDailyPrayerTimes? retrievedOnlyFajrDerived = await _waqtiRepository.GetPrayerTimesAsync(date, regular.Longitude, regular.Latitude, "Mwl2007", 7.5, null, default);
        WaqtiDailyPrayerTimes? retrievedUnknownAngle = await _waqtiRepository.GetPrayerTimesAsync(date, regular.Longitude, regular.Latitude, "Mwl2007", 4.5, null, default);

        // ASSERT
        retrievedRegular!.ID.Should().Be(regular.ID);
        retrievedDerived!.ID.Should().Be(derived.ID);
        retrievedOnlyFajrDerived!.ID.Should().Be(onlyFajrDerived.ID);
        retrievedUnknownAngle.Should().BeNull();
    }

    [Fact]
    public async Task InsertPrayerTimesAsync_NewTime_TimeInDb()
    {
        // ARRANGE
        WaqtiDailyPrayerTimes newWaqtiTime = CreatePrayerTimes(new LocalDate(2023, 7, 31));

        // ACT
        await _waqtiRepository.InsertPrayerTimesAsync([newWaqtiTime], default);

        // ASSERT
        WaqtiDailyPrayerTimes? insertedTime = await TestAssertDbContext.WaqtiPrayerTimes.FindAsync(newWaqtiTime.ID);
        insertedTime.Should().BeEquivalentTo(newWaqtiTime, options => options.IgnoringCyclicReferences());
    }

    [Fact]
    public async Task InsertPrayerTimesAsync_WholeMonth_EveryDayInDb()
    {
        // ARRANGE
        List<WaqtiDailyPrayerTimes> month =
            [.. Enumerable.Range(1, 31).Select(day => CreatePrayerTimes(new LocalDate(2023, 7, day)))];

        // ACT
        await _waqtiRepository.InsertPrayerTimesAsync(month, default);

        // ASSERT
        (await TestAssertDbContext.WaqtiPrayerTimes.CountAsync()).Should().Be(31);
    }

    [Fact]
    public async Task InsertPrayerTimesAsync_MonthPartlyStored_OnlyMissingDaysInserted()
    {
        // ARRANGE
        WaqtiDailyPrayerTimes alreadyStored = CreatePrayerTimes(new LocalDate(2023, 7, 15));
        await TestArrangeDbContext.WaqtiPrayerTimes.AddAsync(alreadyStored);
        await TestArrangeDbContext.SaveChangesAsync();

        List<WaqtiDailyPrayerTimes> month =
            [.. Enumerable.Range(1, 31).Select(day => CreatePrayerTimes(new LocalDate(2023, 7, day)))];

        // ACT
        await _waqtiRepository.InsertPrayerTimesAsync(month, default);

        // ASSERT
        List<WaqtiDailyPrayerTimes> storedTimes = await TestAssertDbContext.WaqtiPrayerTimes.ToListAsync();
        storedTimes.Should().HaveCount(31);
        storedTimes.Select(x => x.Date).Should().OnlyHaveUniqueItems();
        storedTimes.Single(x => x.Date == alreadyStored.Date).ID.Should().Be(alreadyStored.ID);
    }

    [Fact]
    public async Task InsertPrayerTimesAsync_SameDayOtherMethodStored_DayInsertedForNewMethod()
    {
        // ARRANGE
        var date = new LocalDate(2023, 7, 30);
        await TestArrangeDbContext.WaqtiPrayerTimes.AddAsync(CreatePrayerTimes(date, method: "Mwl2007"));
        await TestArrangeDbContext.SaveChangesAsync();

        // ACT
        await _waqtiRepository.InsertPrayerTimesAsync([CreatePrayerTimes(date, method: "Diyanet")], default);

        // ASSERT
        List<WaqtiDailyPrayerTimes> storedTimes = await TestAssertDbContext.WaqtiPrayerTimes.ToListAsync();
        storedTimes.Select(x => x.Method).Should().BeEquivalentTo(["Mwl2007", "Diyanet"]);
    }

    [Fact]
    public async Task InsertPrayerTimesAsync_SameDayOtherAnglesStored_DayInsertedForNewAngles()
    {
        // ARRANGE
        var date = new LocalDate(2023, 7, 30);
        await TestArrangeDbContext.WaqtiPrayerTimes.AddAsync(CreatePrayerTimes(date));
        await TestArrangeDbContext.SaveChangesAsync();

        // ACT
        await _waqtiRepository.InsertPrayerTimesAsync(
            [
                CreatePrayerTimes(date),
                CreatePrayerTimes(date, fajrAngle: 4.5, ishaaAngle: -4.5),
            ],
            default);

        // ASSERT
        List<WaqtiDailyPrayerTimes> storedTimes = await TestAssertDbContext.WaqtiPrayerTimes.ToListAsync();
        storedTimes.Should().HaveCount(2);
        storedTimes.Should().ContainSingle(x => x.FajrAngle == 4.5 && x.IshaaAngle == -4.5);
    }

    [Fact]
    public async Task InsertPrayerTimesAsync_EmptyList_NothingInserted()
    {
        // ACT
        await _waqtiRepository.InsertPrayerTimesAsync([], default);

        // ASSERT
        (await TestAssertDbContext.WaqtiPrayerTimes.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task DeleteCacheDataAsync_EntriesStoredBeforeInstant_DeletedRegardlessOfTheirDay()
    {
        // ARRANGE
        ZonedDateTime baseDate = new LocalDate(2023, 1, 1).AtStartOfDayInZone(DateTimeZone.Utc);
        ZonedDateTime oldDate = baseDate.Minus(Duration.FromDays(5));
        ZonedDateTime newDate = baseDate.Plus(Duration.FromDays(1));

        WaqtiDailyPrayerTimes oldTime = CreatePrayerTimes(oldDate.Date, timeZone: DateTimeZone.Utc);
        WaqtiDailyPrayerTimes newTime = CreatePrayerTimes(newDate.Date, timeZone: DateTimeZone.Utc);

        await TestArrangeDbContext.WaqtiPrayerTimes.AddRangeAsync(oldTime, newTime);
        await TestArrangeDbContext.SaveChangesAsync();

        // the day of the times doesn't matter, only when they were stored: the times of the
        // earlier day were stored recently, the ones of the later day long ago
        var insertedBefore = NodaTime.Instant.FromUtc(2024, 1, 1, 0, 0);
        oldTime.InsertInstant = insertedBefore.Plus(Duration.FromHours(1));
        newTime.InsertInstant = insertedBefore.Minus(Duration.FromDays(1));
        await TestArrangeDbContext.SaveChangesAsync();

        (await TestArrangeDbContext.WaqtiPrayerTimes.FindAsync(oldTime.ID)).Should().NotBeNull();
        (await TestArrangeDbContext.WaqtiPrayerTimes.FindAsync(newTime.ID)).Should().NotBeNull();

        // ACT
        await _waqtiRepository.DeleteCacheDataAsync(insertedBefore, default);

        // ASSERT
        (await TestAssertDbContext.WaqtiPrayerTimes.FindAsync(oldTime.ID)).Should().NotBeNull();
        (await TestAssertDbContext.WaqtiPrayerTimes.FindAsync(newTime.ID)).Should().BeNull();
    }

    private static Instant Instant(LocalDate date, int hour, int minute)
    {
        return NodaTime.Instant.FromUtc(date.Year, date.Month, date.Day, hour, minute);
    }
}
