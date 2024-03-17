using FluentAssertions;
using NodaTime;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Models.Entities;
using PrayerTimeEngine.Core.Tests.Common;
using PrayerTimeEngine.Core.Tests.Common.TestData;
using Refit;
using System.Net;

namespace PrayerTimeEngine.Core.Tests.Unit.Domain.Calculators.Muwaqqit
{
    public class MuwaqqitApiServiceTests : BaseTest
    {
        private readonly MockHttpMessageHandler _mockHttpMessageHandler;
        private readonly IMuwaqqitApiService _muwaqqitApiService;

        public MuwaqqitApiServiceTests()
        {
            _mockHttpMessageHandler = new MockHttpMessageHandler();
            var httpClient = new HttpClient(_mockHttpMessageHandler)
            {
                BaseAddress = new Uri(@"https://www.muwaqqit.com/")
            };
            _muwaqqitApiService = RestService.For<IMuwaqqitApiService>(httpClient);
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
            var response = await _muwaqqitApiService.GetTimesAsync(
                date: date.ToString("yyyy-MM-dd", null),
                longitude: 1M,
                latitude: 1M,
                timezone: europeTimeZone.Id,
                fajrDegree: -12,
                ishaDegree: -12,
                ishtibaqDegree: -8,
                asrKarahaDegree: 3.5,
                cancellationToken: default);
            MuwaqqitPrayerTimes time = response.ToMuwaqqitPrayerTimes();

            // ASSERT
            time.Should().NotBeNull();

            time.ID.Should().Be(0);
            time.Date.Should().Be(new LocalDate(2023, 7, 30));
            time.FajrDegree.Should().Be(-12);
            time.AsrKarahaDegree.Should().Be(3.5);
            time.IshtibaqDegree.Should().Be(-8);
            time.IshaDegree.Should().Be(-12);
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
