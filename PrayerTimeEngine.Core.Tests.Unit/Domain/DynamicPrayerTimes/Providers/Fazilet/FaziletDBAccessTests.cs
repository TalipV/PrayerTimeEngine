using Microsoft.EntityFrameworkCore;
using NodaTime;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Models.Entities;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Services;
using PrayerTimeEngine.Core.Tests.Common;

namespace PrayerTimeEngine.Core.Tests.Unit.Domain.DynamicPrayerTimes.Providers.Fazilet;

public class FaziletDBAccessTests : BaseTest
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    private readonly FaziletDBAccess _faziletDBAccess;

    public FaziletDBAccessTests()
    {
        _dbContextFactory = GetHandledDbContextFactory();
        _faziletDBAccess = new FaziletDBAccess(_dbContextFactory);
    }

    [Fact]
    public async Task GetCountries_NoCountryPresent_SingleCountryReturned()
    {
        // ARRANGE

        // ACT
        var countries = await _faziletDBAccess.GetCountries(default);

        // ASSERT
        countries.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCountries_TwoCountriesPresent_SingleCountryReturned()
    {
        // ARRANGE
        var country1 = new FaziletCountry { ID = 1, Name = "Deutschland" };
        var country2 = new FaziletCountry { ID = 2, Name = "Türkei" };
        await TestArrangeDbContext.FaziletCountries.AddAsync(country1);
        await TestArrangeDbContext.FaziletCountries.AddAsync(country2);
        await TestArrangeDbContext.SaveChangesAsync();

        // ACT
        var countries = await _faziletDBAccess.GetCountries(default);

        // ASSERT
        countries.Should().HaveCount(2);
        countries.OrderBy(x => x.Name).First().Should().BeEquivalentTo(country1);
        countries.OrderBy(x => x.Name).Last().Should().BeEquivalentTo(country2);
    }

    [Fact]
    public async Task GetCitiesByCountryID_RequestForCountryA_OnlyReturnForCountryA()
    {
        // ARRANGE
        var germany = new FaziletCountry { ID = 1, Name = "Deutschland" };
        var austria = new FaziletCountry { ID = 2, Name = "Österreich" };
        var gerCity1 = new FaziletCity { ID = 1, CountryID = germany.ID, Name = "Berlin", Country = germany };
        var gerCity2 = new FaziletCity { ID = 2, CountryID = germany.ID, Name = "Hamburg", Country = germany };
        var autCity1 = new FaziletCity { ID = 3, CountryID = austria.ID, Name = "Wien", Country = austria };
        germany.Cities = [gerCity1, gerCity2];
        austria.Cities = [autCity1];
        await TestArrangeDbContext.FaziletCountries.AddAsync(germany);
        await TestArrangeDbContext.FaziletCountries.AddAsync(austria);
        await TestArrangeDbContext.SaveChangesAsync();

        // ACT
        var germanCities = await _faziletDBAccess.GetCitiesByCountryID(germany.ID, default);

        // ASSERT
        germanCities.Should().HaveCount(2);
        germanCities.OrderBy(x => x.Name).First().Should()
            .BeEquivalentTo(
                gerCity1,
                config: options => options.IgnoringCyclicReferences());
        germanCities.OrderBy(x => x.Name).Last().Should()
            .BeEquivalentTo(
                gerCity2,
                config: options => options.IgnoringCyclicReferences());
    }

    [Fact]
    public async Task GetCitiesByCountryID_CountryWithoutCities_EmptyResult()
    {
        // ARRANGE
        var germany = new FaziletCountry { ID = 1, Name = "Deutschland" };
        await TestArrangeDbContext.FaziletCountries.AddAsync(germany);
        await TestArrangeDbContext.SaveChangesAsync();

        // ACT
        var germanCities = await _faziletDBAccess.GetCitiesByCountryID(germany.ID, default);

        // ASSERT
        germanCities.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCitiesByCountryID_UnknownCountry_EmptyResult()
    {
        // ARRANGE
        var germany = new FaziletCountry { ID = 1, Name = "Deutschland" };
        var gerCity1 = new FaziletCity { ID = 1, CountryID = germany.ID, Name = "Berlin", Country = germany };
        var gerCity2 = new FaziletCity { ID = 2, CountryID = germany.ID, Name = "Hamburg", Country = germany };
        germany.Cities = [gerCity1, gerCity2];
        await TestArrangeDbContext.FaziletCountries.AddAsync(germany);
        await TestArrangeDbContext.SaveChangesAsync();

        // ACT
        var cities = await _faziletDBAccess.GetCitiesByCountryID(2, default);

        // ASSERT
        cities.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTimesByDateAndCityID_DifferentCitiesAndTimes_FindCorrectOne()
    {
        // ARRANGE
        var date = new LocalDate(2023, 1, 5);
        ZonedDateTime dateInUtc = date.AtStartOfDayInZone(DateTimeZone.Utc);
        var germany = new FaziletCountry { ID = 1, Name = "Deutschland" };
        var gerCity1 = new FaziletCity { ID = 1, CountryID = germany.ID, Name = "Berlin", Country = germany };
        var gerCity2 = new FaziletCity { ID = 2, CountryID = germany.ID, Name = "Hamburg", Country = germany };
        germany.Cities = [gerCity1, gerCity2];

        var city1Times1 = new FaziletDailyPrayerTimes
        {
            CityID = gerCity1.ID,
            Date = dateInUtc,
            Imsak = dateInUtc.PlusHours(4),
            Fajr = dateInUtc.PlusHours(5),
            Shuruq = dateInUtc.PlusHours(7),
            Dhuhr = dateInUtc.PlusHours(12),
            Asr = dateInUtc.PlusHours(15),
            Maghrib = dateInUtc.PlusHours(18),
            Isha = dateInUtc.PlusHours(20),
        };

        var city1Times2 = new FaziletDailyPrayerTimes
        {
            CityID = gerCity1.ID,
            Date = dateInUtc.Plus(Duration.FromDays(1)),
            Imsak = dateInUtc.PlusHours(24 + 4),
            Fajr = dateInUtc.PlusHours(24 + 5),
            Shuruq = dateInUtc.PlusHours(24 + 7),
            Dhuhr = dateInUtc.PlusHours(24 + 12),
            Asr = dateInUtc.PlusHours(24 + 15),
            Maghrib = dateInUtc.PlusHours(24 + 19),
            Isha = dateInUtc.PlusHours(24 + 23),
        };

        var city2Times1 = new FaziletDailyPrayerTimes
        {
            CityID = gerCity2.ID,
            Date = dateInUtc,
            Imsak = dateInUtc.PlusHours(4),
            Fajr = dateInUtc.PlusHours(5),
            Shuruq = dateInUtc.PlusHours(7),
            Dhuhr = dateInUtc.PlusHours(12),
            Asr = dateInUtc.PlusHours(15),
            Maghrib = dateInUtc.PlusHours(18),
            Isha = dateInUtc.PlusHours(21),
        };

        await TestArrangeDbContext.FaziletCountries.AddAsync(germany);
        await TestArrangeDbContext.FaziletPrayerTimes.AddAsync(city1Times1);
        await TestArrangeDbContext.FaziletPrayerTimes.AddAsync(city2Times1);
        await TestArrangeDbContext.SaveChangesAsync();

        // ACT
        var times = await _faziletDBAccess.GetTimesByDateAndCityID(dateInUtc, gerCity1.ID, default);

        // ASSERT
        times.Should()
            .BeEquivalentTo(city1Times1)
            .And.NotBe(city1Times2);
    }

    [Fact]
    public async Task GetTimesByDateAndCityID_FindForUnknownCity_ReturnsNull()
    {
        // ARRANGE
        var date = new LocalDate(2023, 1, 5);
        ZonedDateTime dateInUtc = date.AtStartOfDayInZone(DateTimeZone.Utc);
        var germany = new FaziletCountry { ID = 1, Name = "Deutschland" };
        var gerCity1 = new FaziletCity { ID = 1, CountryID = germany.ID, Name = "Berlin", Country = germany };
        var gerCity2 = new FaziletCity { ID = 2, CountryID = germany.ID, Name = "Hamburg", Country = germany };
        germany.Cities = [gerCity1, gerCity2];

        var city1Times1 = new FaziletDailyPrayerTimes
        {
            CityID = gerCity1.ID,
            Date = dateInUtc,
            Imsak = dateInUtc.PlusHours(4),
            Fajr = dateInUtc.PlusHours(5),
            Shuruq = dateInUtc.PlusHours(7),
            Dhuhr = dateInUtc.PlusHours(12),
            Asr = dateInUtc.PlusHours(15),
            Maghrib = dateInUtc.PlusHours(18),
            Isha = dateInUtc.PlusHours(20),
        };

        var city1Times2 = new FaziletDailyPrayerTimes
        {
            CityID = gerCity1.ID,
            Date = dateInUtc.Plus(Duration.FromDays(1)),
            Imsak = dateInUtc.PlusHours(24 + 4),
            Fajr = dateInUtc.PlusHours(24 + 5),
            Shuruq = dateInUtc.PlusHours(24 + 7),
            Dhuhr = dateInUtc.PlusHours(24 + 12),
            Asr = dateInUtc.PlusHours(24 + 15),
            Maghrib = dateInUtc.PlusHours(24 + 19),
            Isha = dateInUtc.PlusHours(24 + 23),
        };

        var city2Times1 = new FaziletDailyPrayerTimes
        {
            CityID = gerCity2.ID,
            Date = dateInUtc,
            Imsak = dateInUtc.PlusHours(4),
            Fajr = dateInUtc.PlusHours(5),
            Shuruq = dateInUtc.PlusHours(7),
            Dhuhr = dateInUtc.PlusHours(12),
            Asr = dateInUtc.PlusHours(15),
            Maghrib = dateInUtc.PlusHours(18),
            Isha = dateInUtc.PlusHours(21),
        };

        await TestArrangeDbContext.FaziletCountries.AddAsync(germany);
        await TestArrangeDbContext.FaziletPrayerTimes.AddAsync(city1Times1);
        await TestArrangeDbContext.FaziletPrayerTimes.AddAsync(city1Times2);
        await TestArrangeDbContext.FaziletPrayerTimes.AddAsync(city2Times1);
        await TestArrangeDbContext.SaveChangesAsync();

        // ACT
        var times = await _faziletDBAccess.GetTimesByDateAndCityID(dateInUtc, 5, default);

        // ASSERT
        times.Should().BeNull();
    }

    [Fact]
    public async Task InsertCountries_InsertThreeNewCountries_AllThreeInserted()
    {
        // ARRANGE
        List<FaziletCountry> newCountries =
        [
            new FaziletCountry { ID = 1, Name = "Deutschland" },
            new FaziletCountry { ID = 2, Name = "Österreich" },
            new FaziletCountry { ID = 3, Name = "Schweiz" },
        ];

        // ACT
        await _faziletDBAccess.InsertCountries(newCountries, default);

        // ASSERT
        foreach (var newCountry in newCountries)
        {
            var foundCountry = await TestAssertDbContext.FaziletCountries.FindAsync(newCountry.ID);
            newCountry.Should().BeEquivalentTo(foundCountry);
        }
    }

    [Fact]
    public async Task InsertCities_InsertThreeNewCities_AllThreeInserted()
    {
        // ARRANGE
        var germany = new FaziletCountry { ID = 1, Name = "Deutschland" };
        var austria = new FaziletCountry { ID = 2, Name = "Österreich" };
        await TestArrangeDbContext.FaziletCountries.AddAsync(germany);
        await TestArrangeDbContext.FaziletCountries.AddAsync(austria);
        await TestArrangeDbContext.SaveChangesAsync();

        List<FaziletCity> gerCities =
        [
            new FaziletCity { ID = 1, CountryID = 1, Name = "Berlin" },
            new FaziletCity { ID = 2, CountryID = 1, Name = "Köln" }
        ];
        List<FaziletCity> autCities =
        [
            new FaziletCity { ID = 3, CountryID = 2, Name = "Wien" }
        ];

        // ACT
        await _faziletDBAccess.InsertCities([.. gerCities, .. autCities], default);

        // ASSERT
        foreach (var newCity in gerCities.Concat(autCities))
        {
            var foundCity = await TestAssertDbContext.FaziletCities.FirstAsync(x => x.ID == newCity.ID);
            foundCity.Should().BeEquivalentTo(newCity);
        }
    }

    [Fact]
    public async Task InsertPrayerTimesAsync_InsertThreeNewTimes_AllThreeInserted()
    {
        // ARRANGE
        var date = new LocalDate(2023, 1, 1);
        ZonedDateTime dateInUtc = date.AtStartOfDayInZone(DateTimeZone.Utc);
        var germany = new FaziletCountry { ID = 1, Name = "Deutschland" };
        germany.Cities =
        [
            new FaziletCity { ID = 1, CountryID = 1, Name = "Berlin", Country = germany },
            new FaziletCity { ID = 2, CountryID = 1, Name = "Köln", Country = germany }
        ];
        var austria = new FaziletCountry { ID = 2, Name = "Österreich" };
        austria.Cities =
        [
            new FaziletCity { ID = 3, CountryID = 2, Name = "Wien", Country = austria }
        ];

        await TestArrangeDbContext.FaziletCountries.AddAsync(germany);
        await TestArrangeDbContext.FaziletCountries.AddAsync(austria);
        await TestArrangeDbContext.SaveChangesAsync();

        var time1 = new FaziletDailyPrayerTimes
        {
            CityID = 1,
            Date = dateInUtc.Plus(Duration.FromDays(1)),
            Imsak = dateInUtc,
            Fajr = dateInUtc,
            Shuruq = dateInUtc,
            Dhuhr = dateInUtc,
            Asr = dateInUtc,
            Maghrib = dateInUtc,
            Isha = dateInUtc,
        };
        var time2 = new FaziletDailyPrayerTimes
        {
            CityID = 2,
            Date = dateInUtc.Plus(Duration.FromDays(2)),
            Imsak = dateInUtc,
            Fajr = dateInUtc,
            Shuruq = dateInUtc,
            Dhuhr = dateInUtc,
            Asr = dateInUtc,
            Maghrib = dateInUtc,
            Isha = dateInUtc,
        };
        var time3 = new FaziletDailyPrayerTimes
        {
            CityID = 1,
            Date = dateInUtc.Plus(Duration.FromDays(3)),
            Imsak = dateInUtc,
            Fajr = dateInUtc,
            Shuruq = dateInUtc,
            Dhuhr = dateInUtc,
            Asr = dateInUtc,
            Maghrib = dateInUtc,
            Isha = dateInUtc,
        };

        // ACT
        await _faziletDBAccess.InsertPrayerTimesAsync([time1, time2, time3], default);

        // ASSERT
        (await TestAssertDbContext.FaziletPrayerTimes.FindAsync(time1.ID)).Should().BeEquivalentTo(time1);
        (await TestAssertDbContext.FaziletPrayerTimes.FindAsync(time2.ID)).Should().BeEquivalentTo(time2);
        (await TestAssertDbContext.FaziletPrayerTimes.FindAsync(time3.ID)).Should().BeEquivalentTo(time3);
    }
}
