using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using PrayerTimeEngine.Core.Tests.Common;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Models.Entities;

namespace PrayerTimeEngine.Core.Tests.Unit.Domain.Calculators.Semerkand
{
    public class SemerkandDBAccessTests : BaseTest
    {
        private readonly AppDbContext _appDbContext;
        private readonly SemerkandDBAccess _semerkandDBAccess;

        public SemerkandDBAccessTests()
        {
            _appDbContext = GetHandledDbContext();
            _semerkandDBAccess = new SemerkandDBAccess(_appDbContext);
        }

        [Fact]
        public async Task GetCountries_NoCountryPresent_SingleCountryReturned()
        {
            // ARRANGE
            
            // ACT
            var countries = await _semerkandDBAccess.GetCountries(default);
            
            // ASSERT
            countries.Should().BeEmpty();
        }

        [Fact]
        public async Task GetCountries_TwoCountriesPresent_SingleCountryReturned()
        {
            // ARRANGE
            var country1 = new SemerkandCountry { Name = "Deutschland" };
            var country2 = new SemerkandCountry { Name = "Türkei" };
            await _appDbContext.SemerkandCountries.AddAsync(country1);
            await _appDbContext.SemerkandCountries.AddAsync(country2);
            await _appDbContext.SaveChangesAsync();
            
            // ACT
            var countries = await _semerkandDBAccess.GetCountries(default);
            
            // ASSERT
            countries.Should().HaveCount(2);
            countries.OrderBy(x => x.Name).First().Should().BeEquivalentTo(country1);
            countries.OrderBy(x => x.Name).Last().Should().BeEquivalentTo(country2);
        }

        [Fact]
        public async Task GetCitiesByCountryID_RequestForCountryA_OnlyReturnForCountryA()
        {
            // ARRANGE
            var germany = new SemerkandCountry { ID = 1, Name = "Deutschland" };
            var austria = new SemerkandCountry { ID = 2, Name = "Österreich" };
            var gerCity1 = new SemerkandCity { CountryID = germany.ID, Name = "Berlin", Country = germany};
            var gerCity2 = new SemerkandCity { CountryID = germany.ID, Name = "Hamburg", Country = germany};
            var autCity1 = new SemerkandCity { CountryID = austria.ID, Name = "Wien", Country = austria};
            germany.Cities = [gerCity1, gerCity2];
            austria.Cities = [autCity1];
            await _appDbContext.SemerkandCountries.AddAsync(germany);
            await _appDbContext.SemerkandCountries.AddAsync(austria);
            await _appDbContext.SaveChangesAsync();
            
            // ACT
            var germanCities = await _semerkandDBAccess.GetCitiesByCountryID(germany.ID, default);

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
            var germany = new SemerkandCountry { ID = 1, Name = "Deutschland" };
            await _appDbContext.SemerkandCountries.AddAsync(germany);
            await _appDbContext.SaveChangesAsync();
            
            // ACT
            var germanCities = await _semerkandDBAccess.GetCitiesByCountryID(germany.ID, default);

            // ASSERT
            germanCities.Should().BeEmpty();
        }

        [Fact]
        public async Task GetCitiesByCountryID_UnknownCountry_EmptyResult()
        {
            // ARRANGE
            var germany = new SemerkandCountry { ID = 1, Name = "Deutschland" };
            var gerCity1 = new SemerkandCity { CountryID = germany.ID, Name = "Berlin", Country = germany};
            var gerCity2 = new SemerkandCity { CountryID = germany.ID, Name = "Hamburg", Country = germany};
            germany.Cities = [gerCity1, gerCity2];
            await _appDbContext.SemerkandCountries.AddAsync(germany);
            await _appDbContext.SaveChangesAsync();
            
            // ACT
            var cities = await _semerkandDBAccess.GetCitiesByCountryID(2, default);

            // ASSERT
            cities.Should().BeEmpty();
        }

        [Fact]
        public async Task GetTimesByDateAndCityID_DifferentCitiesAndTimes_FindCorrectOne()
        {
            // ARRANGE
            var date = new LocalDate(2023, 1, 5);
            ZonedDateTime dateInUTC = date.AtStartOfDayInZone(DateTimeZone.Utc);
            var germany = new SemerkandCountry { ID = 1, Name = "Deutschland" };
            var gerCity1 = new SemerkandCity { ID = 1, CountryID = germany.ID, Name = "Berlin", Country = germany};
            var gerCity2 = new SemerkandCity { ID = 2, CountryID = germany.ID, Name = "Hamburg", Country = germany};
            germany.Cities = [gerCity1, gerCity2];

            var city1Times1 = new SemerkandPrayerTimes
            {
                CityID = gerCity1.ID,
                DayOfYear = 5,
                Date = date,
                Fajr = dateInUTC.PlusHours(5),
                Shuruq = dateInUTC.PlusHours(7),
                Dhuhr = dateInUTC.PlusHours(12),
                Asr = dateInUTC.PlusHours(15),
                Maghrib = dateInUTC.PlusHours(18),
                Isha = dateInUTC.PlusHours(20),
            };
            
            var city1Times2 = new SemerkandPrayerTimes
            {
                CityID = gerCity1.ID,
                DayOfYear = 6,
                Date = date.PlusDays(1),
                Fajr = dateInUTC.PlusHours(24 + 5),
                Shuruq = dateInUTC.PlusHours(24 + 7),
                Dhuhr = dateInUTC.PlusHours(24 + 12),
                Asr = dateInUTC.PlusHours(24 + 15),
                Maghrib = dateInUTC.PlusHours(24 + 19),
                Isha = dateInUTC.PlusHours(24 + 23),
            };
            
            var city2Times1 = new SemerkandPrayerTimes
            {
                CityID = gerCity2.ID,
                DayOfYear = 5,
                Date = date,
                Fajr = dateInUTC.PlusHours(5),
                Shuruq = dateInUTC.PlusHours(7),
                Dhuhr = dateInUTC.PlusHours(12),
                Asr = dateInUTC.PlusHours(15),
                Maghrib = dateInUTC.PlusHours(18),
                Isha = dateInUTC.PlusHours(21),
            };
            
            await _appDbContext.SemerkandCountries.AddAsync(germany);
            await _appDbContext.SemerkandPrayerTimes.AddAsync(city1Times1);
            await _appDbContext.SemerkandPrayerTimes.AddAsync(city2Times1);
            await _appDbContext.SaveChangesAsync();
            
            // ACT
            var times = await _semerkandDBAccess.GetTimesByDateAndCityID(date, gerCity1.ID, default);

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
            var germany = new SemerkandCountry { ID = 1, Name = "Deutschland" };
            var gerCity1 = new SemerkandCity { ID = 1, CountryID = germany.ID, Name = "Berlin", Country = germany};
            var gerCity2 = new SemerkandCity { ID = 2, CountryID = germany.ID, Name = "Hamburg", Country = germany};
            germany.Cities = [gerCity1, gerCity2];

            var city1Times1 = new SemerkandPrayerTimes
            {
                CityID = gerCity1.ID,
                DayOfYear = 5,
                Date = date,
                Fajr = dateInUTC.PlusHours(5),
                Shuruq = dateInUTC.PlusHours(7),
                Dhuhr = dateInUTC.PlusHours(12),
                Asr = dateInUTC.PlusHours(15),
                Maghrib = dateInUTC.PlusHours(18),
                Isha = dateInUTC.PlusHours(20),
            };

            var city1Times2 = new SemerkandPrayerTimes
            {
                CityID = gerCity1.ID,
                DayOfYear = 6,
                Date = date.PlusDays(1),
                Fajr = dateInUTC.PlusHours(24 + 5),
                Shuruq = dateInUTC.PlusHours(24 + 7),
                Dhuhr = dateInUTC.PlusHours(24 + 12),
                Asr = dateInUTC.PlusHours(24 + 15),
                Maghrib = dateInUTC.PlusHours(24 + 19),
                Isha = dateInUTC.PlusHours(24 + 23),
            };
            
            var city2Times1 = new SemerkandPrayerTimes
            {
                CityID = gerCity2.ID,
                DayOfYear = 5,
                Date = date,
                Fajr = dateInUTC.PlusHours(5),
                Shuruq = dateInUTC.PlusHours(7),
                Dhuhr = dateInUTC.PlusHours(12),
                Asr = dateInUTC.PlusHours(15),
                Maghrib = dateInUTC.PlusHours(18),
                Isha = dateInUTC.PlusHours(21),
            };
            
            await _appDbContext.SemerkandCountries.AddAsync(germany);
            await _appDbContext.SemerkandPrayerTimes.AddAsync(city1Times1);
            await _appDbContext.SemerkandPrayerTimes.AddAsync(city1Times2);
            await _appDbContext.SemerkandPrayerTimes.AddAsync(city2Times1);
            await _appDbContext.SaveChangesAsync();
            
            // ACT
            var times = await _semerkandDBAccess.GetTimesByDateAndCityID(date, 5, default);

            // ASSERT
            times.Should().BeNull();
        }

        [Fact]
        public async Task InsertCountries_InsertThreeNewCountries_AllThreeInserted()
        {
            // ARRANGE
            List<SemerkandCountry> newCountries = 
            [
                new SemerkandCountry { ID = 1, Name = "Deutschland"},
                new SemerkandCountry { ID = 2, Name = "Österreich"},
                new SemerkandCountry { ID = 3, Name = "Schweiz"},
            ];
                
            // ACT
            await _semerkandDBAccess.InsertCountries(newCountries, default);

            // ASSERT
            foreach (var newCountry in newCountries)
            {
                var foundCountry = await _appDbContext.SemerkandCountries.FindAsync(newCountry.ID);
                newCountry.Should().BeEquivalentTo(foundCountry);
            }
        }

        [Fact]
        public async Task InsertCities_InsertThreeNewCities_AllThreeInserted()
        {
            // ARRANGE
            var germany = new SemerkandCountry { ID = 1, Name = "Deutschland" };
            var austria = new SemerkandCountry { ID = 2, Name = "Österreich" };
            await _appDbContext.SemerkandCountries.AddAsync(germany);
            await _appDbContext.SemerkandCountries.AddAsync(austria);
            await _appDbContext.SaveChangesAsync();

            List<SemerkandCity> gerCities =
            [
                new SemerkandCity { ID = 1, Name = "Berlin", CountryID = 1 },
                new SemerkandCity { ID = 2, Name = "Köln", CountryID = 1 }
            ];
            List<SemerkandCity> autCities =
            [
                new SemerkandCity { ID = 3, Name = "Wien", CountryID = 2 }
            ];

            // ACT
            await _semerkandDBAccess.InsertCities(gerCities, default);
            await _semerkandDBAccess.InsertCities(autCities, default);

            // ASSERT
            foreach (var newCity in gerCities.Concat(autCities))
            {
                var foundCity = 
                    await _appDbContext.SemerkandCities
                        .Include(x => x.Country)
                        .FirstAsync(x => x.ID == newCity.ID);
                foundCity.Should().BeEquivalentTo(newCity);
            }
        }

        [Fact]
        public async Task InsertSemerkandPrayerTimes_InsertThreeNewTimes_AllThreeInserted()
        {
            // ARRANGE
            var date = new LocalDate(2023, 1, 1);
            ZonedDateTime dateInUTC = date.AtStartOfDayInZone(DateTimeZone.Utc);
            var germany = 
                new SemerkandCountry 
                { 
                    ID = 1, 
                    Name = "Deutschland"
                };
            germany.Cities =
                [
                    new SemerkandCity { ID = 1, CountryID = 1, Name = "Berlin", Country = germany },
                    new SemerkandCity { ID = 2, CountryID = 1, Name = "Köln", Country = germany }
                ];

            var austria = 
                new SemerkandCountry 
                { 
                    ID = 2, 
                    Name = "Österreich"
                };
            austria.Cities =
                [
                    new SemerkandCity { ID = 3, CountryID = 2, Name = "Wien", Country = austria }
                ];

            await _appDbContext.SemerkandCountries.AddAsync(germany);
            await _appDbContext.SemerkandCountries.AddAsync(austria);
            await _appDbContext.SaveChangesAsync();

            var time1 = new SemerkandPrayerTimes
            {
                CityID = 1,
                DayOfYear = 2,
                Date = date.PlusDays(1),
                Fajr = dateInUTC,
                Shuruq = dateInUTC,
                Dhuhr = dateInUTC,
                Asr = dateInUTC,
                Maghrib = dateInUTC,
                Isha = dateInUTC,
            }; 
            var time2 = new SemerkandPrayerTimes
            {
                CityID = 2,
                DayOfYear = 3,
                Date = date.PlusDays(2),
                Fajr = dateInUTC,
                Shuruq = dateInUTC,
                Dhuhr = dateInUTC,
                Asr = dateInUTC,
                Maghrib = dateInUTC,
                Isha = dateInUTC,
            };
            var time3 = new SemerkandPrayerTimes
            {
                CityID = 1,
                DayOfYear = 4,
                Date = date.PlusDays(3),
                Fajr = dateInUTC,
                Shuruq = dateInUTC,
                Dhuhr = dateInUTC,
                Asr = dateInUTC,
                Maghrib = dateInUTC,
                Isha = dateInUTC,
            };
            
            // ACT
            await _semerkandDBAccess.InsertSemerkandPrayerTimes(date, 1, time1, default);
            await _semerkandDBAccess.InsertSemerkandPrayerTimes(date, 2, time2, default);
            await _semerkandDBAccess.InsertSemerkandPrayerTimes(date, 1, time3, default);

            // ASSERT
            (await _appDbContext.SemerkandPrayerTimes.FindAsync(time1.ID)).Should().BeEquivalentTo(time1);
            (await _appDbContext.SemerkandPrayerTimes.FindAsync(time2.ID)).Should().BeEquivalentTo(time2);
            (await _appDbContext.SemerkandPrayerTimes.FindAsync(time3.ID)).Should().BeEquivalentTo(time3);
        }
    }
}
