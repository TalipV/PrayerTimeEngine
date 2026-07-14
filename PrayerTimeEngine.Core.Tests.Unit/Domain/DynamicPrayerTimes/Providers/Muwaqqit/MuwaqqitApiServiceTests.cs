using NodaTime;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Muwaqqit.Interfaces;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Muwaqqit.Models.Entities;
using PrayerTimeEngine.Core.Tests.Common;
using PrayerTimeEngine.Core.Tests.Common.TestData;
using Refit;
using System.Net;

namespace PrayerTimeEngine.Core.Tests.Unit.Domain.DynamicPrayerTimes.Providers.Muwaqqit;

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
    public async Task GetPrayerTimesAsync_IrrelevantInput_MockedHttpResults()
    {
        // ARRANGE
        var date = new LocalDate(2023, 7, 29);
        DateTimeZone timeZone = TestDataHelper.EUROPE_VIENNA_TIME_ZONE;

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
        var response = await _muwaqqitApiService.GetPrayerTimesAsync(
            date: date.ToString("yyyy-MM-dd", null),
            longitude: 1M,
            latitude: 1M,
            timezone: timeZone.Id,
            fajrDegree: -12,
            ishaDegree: -12,
            ishtibaqDegree: -8,
            asrKarahaDegree: 3.5,
            cancellationToken: default);
        MuwaqqitDailyPrayerTimes time = response.ToMuwaqqitPrayerTimes();

        // ASSERT
        time.Should().NotBeNull();

        time.ID.Should().Be(0);
        time.Date.Should().Be(new LocalDate(2023, 7, 30));
        time.TimeZone.Should().Be(timeZone);
        time.FajrDegree.Should().Be(-12);
        time.AsrKarahaDegree.Should().Be(3.5);
        time.IshtibaqDegree.Should().Be(-8);
        time.IshaDegree.Should().Be(-12);
        time.Latitude.Should().Be(47.2803835M);
        time.Longitude.Should().Be(11.41337M);
        time.InsertInstant.Should().BeNull();

        time.Asr.Should().Be(Instant.FromUtc(2023, 7, 30, 15, 25, 53));
        time.AsrKaraha.Should().Be(Instant.FromUtc(2023, 7, 30, 18, 23, 53));
        time.AsrMithlayn.Should().Be(Instant.FromUtc(2023, 7, 30, 16, 33, 27));
        time.Dhuhr.Should().Be(Instant.FromUtc(2023, 7, 30, 11, 21, 22));
        time.Duha.Should().Be(Instant.FromUtc(2023, 7, 30, 4, 17, 4));
        time.Fajr.Should().Be(Instant.FromUtc(2023, 7, 30, 2, 27, 4));
        time.Isha.Should().Be(Instant.FromUtc(2023, 7, 30, 20, 13, 17));
        time.Ishtibaq.Should().Be(Instant.FromUtc(2023, 7, 30, 19, 41, 46));
        time.Maghrib.Should().Be(Instant.FromUtc(2023, 7, 30, 18, 50, 59));
        time.NextFajr.Should().Be(Instant.FromUtc(2023, 7, 31, 2, 28, 47));
        time.Shuruq.Should().Be(Instant.FromUtc(2023, 7, 30, 3, 49, 53));
    }
}
