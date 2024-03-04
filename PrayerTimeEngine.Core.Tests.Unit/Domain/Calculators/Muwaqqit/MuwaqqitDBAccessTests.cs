using FluentAssertions;
using NodaTime;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Models;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Services;
using PrayerTimeEngine.Core.Tests.Common;

namespace PrayerTimeEngine.Core.Tests.Unit.Domain.Calculators.Muwaqqit
{
    public class MuwaqqitDBAccessTests : BaseTest
    {
        private readonly AppDbContext _appDbContext;
        private readonly MuwaqqitDBAccess _muwaqqitDBAccess;

        public MuwaqqitDBAccessTests()
        {
            _appDbContext = GetHandledDbContext();
            _muwaqqitDBAccess = new MuwaqqitDBAccess(_appDbContext);
        }

        [Fact]
        public async Task GetTimesAsync_ExistingTime_ReturnsCorrectTime()
        {
            // ARRANGE
            LocalDate date = new LocalDate(2023, 7, 30);
            var muwaqqitTime = new MuwaqqitPrayerTimes
            {
                Date = date,
                Latitude = 47.2803835M,
                Longitude = 11.41337M,
                FajrDegree = 1,
                IshaDegree = 1,
                IshtibaqDegree = 1,
                AsrKarahaDegree = 1,
                Fajr = new ZonedDateTime(Instant.FromUtc(2023, 7, 30, 2, 27, 04), DateTimeZoneProviders.Tzdb["Europe/Vienna"]),
                NextFajr = new ZonedDateTime(Instant.FromUtc(2023, 7, 31, 2, 28, 04), DateTimeZoneProviders.Tzdb["Europe/Vienna"]),
                Shuruq = new ZonedDateTime(Instant.FromUtc(2023, 7, 30, 3, 49, 53), DateTimeZoneProviders.Tzdb["Europe/Vienna"]),
                Duha = new ZonedDateTime(Instant.FromUtc(2023, 7, 30, 4, 49, 53), DateTimeZoneProviders.Tzdb["Europe/Vienna"]),
                Dhuhr = new ZonedDateTime(Instant.FromUtc(2023, 7, 30, 11, 21, 22), DateTimeZoneProviders.Tzdb["Europe/Vienna"]),
                Asr = new ZonedDateTime(Instant.FromUtc(2023, 7, 30, 15, 25, 53), DateTimeZoneProviders.Tzdb["Europe/Vienna"]),
                AsrMithlayn = new ZonedDateTime(Instant.FromUtc(2023, 7, 30, 16, 25, 53), DateTimeZoneProviders.Tzdb["Europe/Vienna"]),
                AsrKaraha = new ZonedDateTime(Instant.FromUtc(2023, 7, 30, 17, 25, 53), DateTimeZoneProviders.Tzdb["Europe/Vienna"]),
                Maghrib = new ZonedDateTime(Instant.FromUtc(2023, 7, 30, 18, 50, 59), DateTimeZoneProviders.Tzdb["Europe/Vienna"]),
                Ishtibaq = new ZonedDateTime(Instant.FromUtc(2023, 7, 30, 19, 50, 59), DateTimeZoneProviders.Tzdb["Europe/Vienna"]),
                Isha = new ZonedDateTime(Instant.FromUtc(2023, 7, 30, 20, 13, 17), DateTimeZoneProviders.Tzdb["Europe/Vienna"])
            };

            await _appDbContext.MuwaqqitPrayerTimes.AddAsync(muwaqqitTime);
            await _appDbContext.SaveChangesAsync();

            // ACT
            var retrievedTime = await _muwaqqitDBAccess.GetTimesAsync(
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
        public async Task InsertMuwaqqitPrayerTimesAsync_NewTime_TimeInDb()
        {
            // ARRANGE
            LocalDate date = new LocalDate(2023, 7, 31);
            var newMuwaqqitTime = new MuwaqqitPrayerTimes
            {
                Date = date,
                Latitude = 47.2803835M,
                Longitude = 11.41337M,
                FajrDegree = 1,
                IshaDegree = 1,
                IshtibaqDegree = 1,
                AsrKarahaDegree = 1,
                Fajr = new ZonedDateTime(Instant.FromUtc(2023, 7, 30, 2, 27, 04), DateTimeZoneProviders.Tzdb["Europe/Vienna"]),
                NextFajr = new ZonedDateTime(Instant.FromUtc(2023, 7, 31, 2, 28, 04), DateTimeZoneProviders.Tzdb["Europe/Vienna"]),
                Shuruq = new ZonedDateTime(Instant.FromUtc(2023, 7, 30, 3, 49, 53), DateTimeZoneProviders.Tzdb["Europe/Vienna"]),
                Duha = new ZonedDateTime(Instant.FromUtc(2023, 7, 30, 4, 49, 53), DateTimeZoneProviders.Tzdb["Europe/Vienna"]),
                Dhuhr = new ZonedDateTime(Instant.FromUtc(2023, 7, 30, 11, 21, 22), DateTimeZoneProviders.Tzdb["Europe/Vienna"]),
                Asr = new ZonedDateTime(Instant.FromUtc(2023, 7, 30, 15, 25, 53), DateTimeZoneProviders.Tzdb["Europe/Vienna"]),
                AsrMithlayn = new ZonedDateTime(Instant.FromUtc(2023, 7, 30, 16, 25, 53), DateTimeZoneProviders.Tzdb["Europe/Vienna"]),
                AsrKaraha = new ZonedDateTime(Instant.FromUtc(2023, 7, 30, 17, 25, 53), DateTimeZoneProviders.Tzdb["Europe/Vienna"]),
                Maghrib = new ZonedDateTime(Instant.FromUtc(2023, 7, 30, 18, 50, 59), DateTimeZoneProviders.Tzdb["Europe/Vienna"]),
                Ishtibaq = new ZonedDateTime(Instant.FromUtc(2023, 7, 30, 19, 50, 59), DateTimeZoneProviders.Tzdb["Europe/Vienna"]),
                Isha = new ZonedDateTime(Instant.FromUtc(2023, 7, 30, 20, 13, 17), DateTimeZoneProviders.Tzdb["Europe/Vienna"])
            };

            // ACT
            await _muwaqqitDBAccess.InsertMuwaqqitPrayerTimesAsync(newMuwaqqitTime, default);

            // ASSERT
            var insertedTime = await _appDbContext.MuwaqqitPrayerTimes.FindAsync(newMuwaqqitTime.ID);
            insertedTime.Should().BeEquivalentTo(newMuwaqqitTime, options => options.IgnoringCyclicReferences());
        }
    }
}