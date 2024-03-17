using FluentAssertions;
using NodaTime;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Models.Entities;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Services;
using PrayerTimeEngine.Core.Tests.Common;
using PrayerTimeEngine.Core.Tests.Common.TestData;
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
            var httpClient = new HttpClient(_mockHttpMessageHandler);
            _muwaqqitApiService = new MuwaqqitApiService(httpClient);
        }

        [Fact]
        public async Task GetTimesAsync_IrrelevantInput_MockedHttpResults()
        {
            // ARRANGE
            var date = new LocalDate(2023, 7, 29);
            DateTimeZone europeTimeZone = DateTimeZoneProviders.Tzdb["Europe/Vienna"];

            _mockHttpMessageHandler.HandleRequestFunc =
                (request) =>
                {
                    Stream responseStream = File.OpenRead(Path.Combine(TestDataHelper.MUWAQQIT_TEST_DATA_FILE_PATH, "Muwaqqit_TestPrayerTimeData_20230730_Innsbruck_Config1.txt"));

                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StreamContent(responseStream)
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

            time.Asr           .Should().Be(new LocalDateTime(2023, 7, 30, 17, 25, 53).InZoneStrictly(europeTimeZone));
            time.AsrKaraha     .Should().Be(new LocalDateTime(2023, 7, 30, 20, 23, 53).InZoneStrictly(europeTimeZone));
            time.AsrMithlayn   .Should().Be(new LocalDateTime(2023, 7, 30, 18, 33, 27).InZoneStrictly(europeTimeZone));
            time.Dhuhr         .Should().Be(new LocalDateTime(2023, 7, 30, 13, 21, 22).InZoneStrictly(europeTimeZone));
            time.Duha          .Should().Be(new LocalDateTime(2023, 7, 30, 06, 17, 04).InZoneStrictly(europeTimeZone));
            time.Fajr          .Should().Be(new LocalDateTime(2023, 7, 30, 04, 27, 04).InZoneStrictly(europeTimeZone));
            time.Isha          .Should().Be(new LocalDateTime(2023, 7, 30, 22, 13, 17).InZoneStrictly(europeTimeZone));
            time.Ishtibaq      .Should().Be(new LocalDateTime(2023, 7, 30, 21, 41, 46).InZoneStrictly(europeTimeZone));
            time.Maghrib       .Should().Be(new LocalDateTime(2023, 7, 30, 20, 50, 59).InZoneStrictly(europeTimeZone));
            time.NextFajr      .Should().Be(new LocalDateTime(2023, 7, 31, 04, 28, 47).InZoneStrictly(europeTimeZone));
            time.Shuruq        .Should().Be(new LocalDateTime(2023, 7, 30, 05, 49, 53).InZoneStrictly(europeTimeZone));
        }
    }
}
