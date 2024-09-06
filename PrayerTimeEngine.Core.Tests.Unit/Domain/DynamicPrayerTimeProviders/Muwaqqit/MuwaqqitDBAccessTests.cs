using Microsoft.EntityFrameworkCore;
using NodaTime;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimeProviders.Muwaqqit.Models.Entities;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimeProviders.Muwaqqit.Services;
using PrayerTimeEngine.Core.Tests.Common;
using PrayerTimeEngine.Core.Tests.Common.TestData;

namespace PrayerTimeEngine.Core.Tests.Unit.Domain.DynamicPrayerTimeProviders.Muwaqqit
{
    public class MuwaqqitDBAccessTests : BaseTest
    {
        private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
        private readonly MuwaqqitDBAccess _muwaqqitDBAccess;

        public MuwaqqitDBAccessTests()
        {
            _dbContextFactory = GetHandledDbContextFactory();
            _muwaqqitDBAccess = new MuwaqqitDBAccess(_dbContextFactory);
        }

        [Fact]
        public async Task GetPrayerTimesAsync_ExistingTime_ReturnsCorrectTime()
        {
            // ARRANGE
            var date = new LocalDate(2023, 7, 30);
            var dateTimeZone = TestDataHelper.EUROPE_VIENNA_TIME_ZONE;
            var muwaqqitTime = new MuwaqqitPrayerTimes
            {
                Date = date.AtStartOfDayInZone(dateTimeZone),
                Latitude = 47.2803835M,
                Longitude = 11.41337M,
                FajrDegree = 1,
                IshaDegree = 1,
                IshtibaqDegree = 1,
                AsrKarahaDegree = 1,
                Fajr = new ZonedDateTime(Instant.FromUtc(2023, 7, 30, 2, 27, 04), dateTimeZone),
                NextFajr = new ZonedDateTime(Instant.FromUtc(2023, 7, 31, 2, 28, 04), dateTimeZone),
                Shuruq = new ZonedDateTime(Instant.FromUtc(2023, 7, 30, 3, 49, 53), dateTimeZone),
                Duha = new ZonedDateTime(Instant.FromUtc(2023, 7, 30, 4, 49, 53), dateTimeZone),
                Dhuhr = new ZonedDateTime(Instant.FromUtc(2023, 7, 30, 11, 21, 22), dateTimeZone),
                Asr = new ZonedDateTime(Instant.FromUtc(2023, 7, 30, 15, 25, 53), dateTimeZone),
                AsrMithlayn = new ZonedDateTime(Instant.FromUtc(2023, 7, 30, 16, 25, 53), dateTimeZone),
                AsrKaraha = new ZonedDateTime(Instant.FromUtc(2023, 7, 30, 17, 25, 53), dateTimeZone),
                Maghrib = new ZonedDateTime(Instant.FromUtc(2023, 7, 30, 18, 50, 59), dateTimeZone),
                Ishtibaq = new ZonedDateTime(Instant.FromUtc(2023, 7, 30, 19, 50, 59), dateTimeZone),
                Isha = new ZonedDateTime(Instant.FromUtc(2023, 7, 30, 20, 13, 17), dateTimeZone)
            };

            await TestArrangeDbContext.MuwaqqitPrayerTimes.AddAsync(muwaqqitTime);
            await TestArrangeDbContext.SaveChangesAsync();

            // ACT
            var retrievedTime = await _muwaqqitDBAccess.GetPrayerTimesAsync(
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
            var newMuwaqqitTime = new MuwaqqitPrayerTimes
            {
                Date = date.AtStartOfDayInZone(dateTimeZone),
                Latitude = 47.2803835M,
                Longitude = 11.41337M,
                FajrDegree = 1,
                IshaDegree = 1,
                IshtibaqDegree = 1,
                AsrKarahaDegree = 1,
                Fajr = new ZonedDateTime(Instant.FromUtc(2023, 7, 30, 2, 27, 04), dateTimeZone),
                NextFajr = new ZonedDateTime(Instant.FromUtc(2023, 7, 31, 2, 28, 04), dateTimeZone),
                Shuruq = new ZonedDateTime(Instant.FromUtc(2023, 7, 30, 3, 49, 53), dateTimeZone),
                Duha = new ZonedDateTime(Instant.FromUtc(2023, 7, 30, 4, 49, 53), dateTimeZone),
                Dhuhr = new ZonedDateTime(Instant.FromUtc(2023, 7, 30, 11, 21, 22), dateTimeZone),
                Asr = new ZonedDateTime(Instant.FromUtc(2023, 7, 30, 15, 25, 53), dateTimeZone),
                AsrMithlayn = new ZonedDateTime(Instant.FromUtc(2023, 7, 30, 16, 25, 53), dateTimeZone),
                AsrKaraha = new ZonedDateTime(Instant.FromUtc(2023, 7, 30, 17, 25, 53), dateTimeZone),
                Maghrib = new ZonedDateTime(Instant.FromUtc(2023, 7, 30, 18, 50, 59), dateTimeZone),
                Ishtibaq = new ZonedDateTime(Instant.FromUtc(2023, 7, 30, 19, 50, 59), dateTimeZone),
                Isha = new ZonedDateTime(Instant.FromUtc(2023, 7, 30, 20, 13, 17), dateTimeZone)
            };

            // ACT
            await _muwaqqitDBAccess.InsertPrayerTimesAsync([newMuwaqqitTime], default);

            // ASSERT
            var insertedTime = await TestAssertDbContext.MuwaqqitPrayerTimes.FindAsync(newMuwaqqitTime.ID);
            insertedTime.Should().BeEquivalentTo(newMuwaqqitTime, options => options.IgnoringCyclicReferences());
        }
    }
}