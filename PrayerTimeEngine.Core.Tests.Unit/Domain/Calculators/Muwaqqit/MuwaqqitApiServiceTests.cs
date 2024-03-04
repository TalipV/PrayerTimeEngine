using FluentAssertions;
using NodaTime;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Models;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Services;
using PrayerTimeEngine.Core.Tests.Common;
using System.Net;

namespace PrayerTimeEngine.Core.Tests.Unit.Domain.Calculators.Muwaqqit
{
    public class MuwaqqitApiServiceTests : BaseTest
    {
        private readonly MockHttpMessageHandler _mockHttpMessageHandler;
        private readonly MuwaqqitApiService _muwaqqitApiService;

        public MuwaqqitApiServiceTests()
        {
            _mockHttpMessageHandler = new MockHttpMessageHandler();
            HttpClient httpClient = new HttpClient(_mockHttpMessageHandler);
            _muwaqqitApiService = new MuwaqqitApiService(httpClient);
        }

        [Fact]
        public async Task GetTimesAsync_X_X()
        {
            // ARRANGE
            LocalDate date = new LocalDate(2023, 7, 29);
            DateTimeZone europeTimeZone = DateTimeZoneProviders.Tzdb["Europe/Vienna"];

            _mockHttpMessageHandler.HandleRequestFunc =
                (request) =>
                {
                    string responseText =
                        File.ReadAllText(
                            Path.Combine(
                                TEST_DATA_FILE_PATH,
                                "MuwaqqitTestData",
                                "Muwaqqit_TestPrayerTimeData_20230730_Innsbruck_Config1.txt"));
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(responseText)
                    };
                };

            // ACT
            MuwaqqitPrayerTimes time = await _muwaqqitApiService.GetTimesAsync(
                date: date,
                longitude: 1M,
                latitude: 1M,
                fajrDegree: 1,
                ishaDegree: 1,
                ishtibaqDegree: 1,
                asrKarahaDegree: 1,
                timezone: europeTimeZone.Id,
                cancellationToken: default);

            // ASSERT
            time.Should().NotBeNull();

            time.ID.Should().Be(0);
            time.Date.Should().Be(new LocalDate(2023, 7, 30));
            time.FajrDegree.Should().Be(1);
            time.AsrKarahaDegree.Should().Be(1);
            time.IshtibaqDegree.Should().Be(1);
            time.IshaDegree.Should().Be(1);
            time.Latitude.Should().Be(47.2803835M);
            time.Longitude.Should().Be(11.41337M);
            time.InsertInstant.Should().BeNull();

            time.Asr.Should().Be(new ZonedDateTime(Instant.FromUtc(2023, 7, 30, 15, 25, 53), europeTimeZone));
            time.AsrKaraha.Should().Be(new ZonedDateTime(Instant.FromUtc(2023, 7, 30, 18, 23, 53), europeTimeZone));
            time.AsrMithlayn.Should().Be(new ZonedDateTime(Instant.FromUtc(2023, 7, 30, 16, 33, 27), europeTimeZone));
            time.Dhuhr.Should().Be(new ZonedDateTime(Instant.FromUtc(2023, 7, 30, 11, 21, 22), europeTimeZone));
            time.Duha.Should().Be(new ZonedDateTime(Instant.FromUtc(2023, 7, 30, 4, 17, 04), europeTimeZone));
            time.Fajr.Should().Be(new ZonedDateTime(Instant.FromUtc(2023, 7, 30, 2, 27, 04), europeTimeZone));
            time.Isha.Should().Be(new ZonedDateTime(Instant.FromUtc(2023, 7, 30, 20, 13, 17), europeTimeZone));
            time.Ishtibaq.Should().Be(new ZonedDateTime(Instant.FromUtc(2023, 7, 30, 19, 41, 46), europeTimeZone));
            time.Maghrib.Should().Be(new ZonedDateTime(Instant.FromUtc(2023, 7, 30, 18, 50, 59), europeTimeZone));
            time.NextFajr.Should().Be(new ZonedDateTime(Instant.FromUtc(2023, 7, 31, 2, 28, 47), europeTimeZone));
            time.Shuruq.Should().Be(new ZonedDateTime(Instant.FromUtc(2023, 7, 30, 3, 49, 53), europeTimeZone));
        }
    }
}
