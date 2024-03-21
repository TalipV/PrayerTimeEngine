using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Models.Entities;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Services;
using PrayerTimeEngine.Core.Tests.Common;

namespace PrayerTimeEngine.Core.Tests.Unit.Domain.Calculators.Fazilet
{
    public class FaziletDBAccessTests : BaseTest
    {
        private readonly AppDbContext _appDbContext;
        private readonly FaziletDBAccess _faziletDBAccess;

        public FaziletDBAccessTests()
        {
            _appDbContext = GetHandledDbContext();
            _faziletDBAccess = new FaziletDBAccess(_appDbContext);
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
            var country1 = new FaziletCountry { Name = "Deutschland" };
            var country2 = new FaziletCountry { Name = "Türkei" };
            await _appDbContext.FaziletCountries.AddAsync(country1);
            await _appDbContext.FaziletCountries.AddAsync(country2);
            await _appDbContext.SaveChangesAsync();

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
            var gerCity1 = new FaziletCity { CountryID = germany.ID, Name = "Berlin", Country = germany };
            var gerCity2 = new FaziletCity { CountryID = germany.ID, Name = "Hamburg", Country = germany };
            var autCity1 = new FaziletCity { CountryID = austria.ID, Name = "Wien", Country = austria };
            germany.Cities = [gerCity1, gerCity2];
            austria.Cities = [autCity1];
            await _appDbContext.FaziletCountries.AddAsync(germany);
            await _appDbContext.FaziletCountries.AddAsync(austria);
            await _appDbContext.SaveChangesAsync();

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
            await _appDbContext.FaziletCountries.AddAsync(germany);
            await _appDbContext.SaveChangesAsync();

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
            var gerCity1 = new FaziletCity { CountryID = germany.ID, Name = "Berlin", Country = germany };
            var gerCity2 = new FaziletCity { CountryID = germany.ID, Name = "Hamburg", Country = germany };
            germany.Cities = [gerCity1, gerCity2];
            await _appDbContext.FaziletCountries.AddAsync(germany);
            await _appDbContext.SaveChangesAsync();

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
            ZonedDateTime dateInUTC = date.AtStartOfDayInZone(DateTimeZone.Utc);
            var germany = new FaziletCountry { ID = 1, Name = "Deutschland" };
            var gerCity1 = new FaziletCity { ID = 1, CountryID = germany.ID, Name = "Berlin", Country = germany };
            var gerCity2 = new FaziletCity { ID = 2, CountryID = germany.ID, Name = "Hamburg", Country = germany };
            germany.Cities = [gerCity1, gerCity2];

            var city1Times1 = new FaziletPrayerTimes
            {
                CityID = gerCity1.ID,
                Date = date,
                Imsak = dateInUTC.PlusHours(4),
                Fajr = dateInUTC.PlusHours(5),
                Shuruq = dateInUTC.PlusHours(7),
                Dhuhr = dateInUTC.PlusHours(12),
                Asr = dateInUTC.PlusHours(15),
                Maghrib = dateInUTC.PlusHours(18),
                Isha = dateInUTC.PlusHours(20),
            };

            var city1Times2 = new FaziletPrayerTimes
            {
                CityID = gerCity1.ID,
                Date = date.PlusDays(1),
                Imsak = dateInUTC.PlusHours(24 + 4),
                Fajr = dateInUTC.PlusHours(24 + 5),
                Shuruq = dateInUTC.PlusHours(24 + 7),
                Dhuhr = dateInUTC.PlusHours(24 + 12),
                Asr = dateInUTC.PlusHours(24 + 15),
                Maghrib = dateInUTC.PlusHours(24 + 19),
                Isha = dateInUTC.PlusHours(24 + 23),
            };

            var city2Times1 = new FaziletPrayerTimes
            {
                CityID = gerCity2.ID,
                Date = date,
                Imsak = dateInUTC.PlusHours(4),
                Fajr = dateInUTC.PlusHours(5),
                Shuruq = dateInUTC.PlusHours(7),
                Dhuhr = dateInUTC.PlusHours(12),
                Asr = dateInUTC.PlusHours(15),
                Maghrib = dateInUTC.PlusHours(18),
                Isha = dateInUTC.PlusHours(21),
            };

            await _appDbContext.FaziletCountries.AddAsync(germany);
            await _appDbContext.FaziletPrayerTimes.AddAsync(city1Times1);
            await _appDbContext.FaziletPrayerTimes.AddAsync(city2Times1);
            await _appDbContext.SaveChangesAsync();

            // ACT
            var times = await _faziletDBAccess.GetTimesByDateAndCityID(date, gerCity1.ID, default);

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
            ZonedDateTime dateInUTC = date.AtStartOfDayInZone(DateTimeZone.Utc);
            var germany = new FaziletCountry { ID = 1, Name = "Deutschland" };
            var gerCity1 = new FaziletCity { ID = 1, CountryID = germany.ID, Name = "Berlin", Country = germany };
            var gerCity2 = new FaziletCity { ID = 2, CountryID = germany.ID, Name = "Hamburg", Country = germany };
            germany.Cities = [gerCity1, gerCity2];

            var city1Times1 = new FaziletPrayerTimes
            {
                CityID = gerCity1.ID,
                Date = date,
                Imsak = dateInUTC.PlusHours(4),
                Fajr = dateInUTC.PlusHours(5),
                Shuruq = dateInUTC.PlusHours(7),
                Dhuhr = dateInUTC.PlusHours(12),
                Asr = dateInUTC.PlusHours(15),
                Maghrib = dateInUTC.PlusHours(18),
                Isha = dateInUTC.PlusHours(20),
            };

            var city1Times2 = new FaziletPrayerTimes
            {
                CityID = gerCity1.ID,
                Date = date.PlusDays(1),
                Imsak = dateInUTC.PlusHours(24 + 4),
                Fajr = dateInUTC.PlusHours(24 + 5),
                Shuruq = dateInUTC.PlusHours(24 + 7),
                Dhuhr = dateInUTC.PlusHours(24 + 12),
                Asr = dateInUTC.PlusHours(24 + 15),
                Maghrib = dateInUTC.PlusHours(24 + 19),
                Isha = dateInUTC.PlusHours(24 + 23),
            };

            var city2Times1 = new FaziletPrayerTimes
            {
                CityID = gerCity2.ID,
                Date = date,
                Imsak = dateInUTC.PlusHours(4),
                Fajr = dateInUTC.PlusHours(5),
                Shuruq = dateInUTC.PlusHours(7),
                Dhuhr = dateInUTC.PlusHours(12),
                Asr = dateInUTC.PlusHours(15),
                Maghrib = dateInUTC.PlusHours(18),
                Isha = dateInUTC.PlusHours(21),
            };

            await _appDbContext.FaziletCountries.AddAsync(germany);
            await _appDbContext.FaziletPrayerTimes.AddAsync(city1Times1);
            await _appDbContext.FaziletPrayerTimes.AddAsync(city1Times2);
            await _appDbContext.FaziletPrayerTimes.AddAsync(city2Times1);
            await _appDbContext.SaveChangesAsync();

            // ACT
            var times = await _faziletDBAccess.GetTimesByDateAndCityID(date, 5, default);

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
            await _faziletDBAccess.InsertCountries(
                newCountries.ToDictionary(x => x.Name, x => x.ID), 
                default);

            // ASSERT
            foreach (var newCountry in newCountries)
            {
                var foundCountry = await _appDbContext.FaziletCountries.FindAsync(newCountry.ID);
                newCountry.Should().BeEquivalentTo(foundCountry);
            }
        }

        [Fact]
        public async Task InsertCities_InsertThreeNewCities_AllThreeInserted()
        {
            // ARRANGE
            var germany = new FaziletCountry { ID = 1, Name = "Deutschland" };
            var austria = new FaziletCountry { ID = 2, Name = "Österreich" };
            await _appDbContext.FaziletCountries.AddAsync(germany);
            await _appDbContext.FaziletCountries.AddAsync(austria);
            await _appDbContext.SaveChangesAsync();

            List<FaziletCity> gerCities =
            [
                new FaziletCity { ID = 1, CountryID = 1, Name = "Berlin", Country = germany },
                new FaziletCity { ID = 2, CountryID = 1, Name = "Köln", Country = germany }
            ];
            List<FaziletCity> autCities =
            [
                new FaziletCity { ID = 3, CountryID = 2, Name = "Wien", Country = austria }
            ];

            // ACT
            await _faziletDBAccess.InsertCities(gerCities.ToDictionary(x => x.Name, x => x.ID), 1, default);
            await _faziletDBAccess.InsertCities(autCities.ToDictionary(x => x.Name, x => x.ID), 2, default);

            // ASSERT
            foreach (var newCity in gerCities.Concat(autCities))
            {
                var foundCity =
                    await _appDbContext.FaziletCities
                        .Include(x => x.Country)
                        .FirstAsync(x => x.ID == newCity.ID);
                foundCity.Should().BeEquivalentTo(newCity);
            }
        }

        [Fact]
        public async Task InsertFaziletPrayerTimes_InsertThreeNewTimes_AllThreeInserted()
        {
            // ARRANGE
            var date = new LocalDate(2023, 1, 1);
            ZonedDateTime dateInUTC = date.AtStartOfDayInZone(DateTimeZone.Utc);
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

            await _appDbContext.FaziletCountries.AddAsync(germany);
            await _appDbContext.FaziletCountries.AddAsync(austria);
            await _appDbContext.SaveChangesAsync();

            var time1 = new FaziletPrayerTimes
            {
                CityID = 1,
                Date = date.PlusDays(1),
                Imsak = dateInUTC,
                Fajr = dateInUTC,
                Shuruq = dateInUTC,
                Dhuhr = dateInUTC,
                Asr = dateInUTC,
                Maghrib = dateInUTC,
                Isha = dateInUTC,
            };
            var time2 = new FaziletPrayerTimes
            {
                CityID = 2,
                Date = date.PlusDays(2),
                Imsak = dateInUTC,
                Fajr = dateInUTC,
                Shuruq = dateInUTC,
                Dhuhr = dateInUTC,
                Asr = dateInUTC,
                Maghrib = dateInUTC,
                Isha = dateInUTC,
            };
            var time3 = new FaziletPrayerTimes
            {
                CityID = 1,
                Date = date.PlusDays(3),
                Imsak = dateInUTC,
                Fajr = dateInUTC,
                Shuruq = dateInUTC,
                Dhuhr = dateInUTC,
                Asr = dateInUTC,
                Maghrib = dateInUTC,
                Isha = dateInUTC,
            };

            // ACT
            await _faziletDBAccess.InsertFaziletPrayerTimes(date, 1, time1, default);
            await _faziletDBAccess.InsertFaziletPrayerTimes(date, 2, time2, default);
            await _faziletDBAccess.InsertFaziletPrayerTimes(date, 1, time3, default);

            // ASSERT
            (await _appDbContext.FaziletPrayerTimes.FindAsync(time1.ID)).Should().BeEquivalentTo(time1);
            (await _appDbContext.FaziletPrayerTimes.FindAsync(time2.ID)).Should().BeEquivalentTo(time2);
            (await _appDbContext.FaziletPrayerTimes.FindAsync(time3.ID)).Should().BeEquivalentTo(time3);
        }
    }
}
