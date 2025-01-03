using NodaTime;
using PrayerTimeEngine.Core.Domain.Calculators.Mosques.Mawaqit.Services;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.Mawaqit.Models.Entities;
using PrayerTimeEngine.Core.Tests.Common;
using PrayerTimeEngine.Core.Tests.Common.TestData;
using System.Net;

namespace PrayerTimeEngine.Core.Tests.Unit.Domain.MosquePrayerTimes.Providers.Mawaqit;

public class MawaqitApiServiceTests : BaseTest
{
    private readonly MockHttpMessageHandler _mockHttpMessageHandler;
    private readonly MawaqitApiService _mawaqitApiService;

    public MawaqitApiServiceTests()
    {
        _mockHttpMessageHandler = new MockHttpMessageHandler();

        _mockHttpMessageHandler.HandleRequestFunc =
            (request) =>
            {
                if (request.RequestUri.AbsoluteUri.EndsWith("hamza-koln"))
                {
                    Stream responseStream = File.OpenRead(Path.Combine(TestDataHelper.MAWAQIT_TEST_DATA_FILE_PATH, "Mawaqit_ResponsePageContent_20240829_hamza-koln.txt"));

                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StreamContent(responseStream)
                    };
                }

                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound,
                };
            };

        var httpClient = new HttpClient(_mockHttpMessageHandler)
        {
            BaseAddress = new Uri("https://mawaqit.net/fr")
        };
        _mawaqitApiService = new MawaqitApiService(httpClient);
    }

    [Fact]
    public async Task GetPrayerTimesAsync_IrrelevantInput_MockedHttpResults()
    {
        // ARRANGE
        var date = new LocalDate(2024, 8, 29);
        string externalID = "hamza-koln";

        // ACT
        var response = await _mawaqitApiService.GetPrayerTimesAsync(externalID, cancellationToken: default);
        var times = response.ToMawaqitPrayerTimes(externalID).ToList();
        MawaqitMosqueDailyPrayerTimes time = times.FirstOrDefault(x => x.Date == date);

        // ASSERT
        time.Should().NotBeNull();

        time.ID.Should().Be(0);
        time.Date.Should().Be(new LocalDate(2024, 8, 29));
        time.ExternalID.Should().Be(externalID);
        time.InsertInstant.Should().BeNull();

        time.Fajr.Should().Be(new LocalTime(05, 05, 00));
        time.FajrCongregation.Should().Be(new LocalTime(05, 35, 00));
        time.Shuruq.Should().Be(new LocalTime(06, 35, 00));
        time.Dhuhr.Should().Be(new LocalTime(13, 35, 00));
        time.DhuhrCongregation.Should().Be(new LocalTime(13, 45, 00));
        time.Asr.Should().Be(new LocalTime(17, 22, 00));
        time.AsrCongregation.Should().Be(new LocalTime(17, 32, 00));
        time.Maghrib.Should().Be(new LocalTime(20, 30, 00));
        time.MaghribCongregation.Should().Be(new LocalTime(20, 35, 00));
        time.Isha.Should().Be(new LocalTime(22, 06, 00));
        time.IshaCongregation.Should().Be(new LocalTime(22, 16, 00));

        time.Jumuah.Should().Be(new LocalTime(14, 30, 00));
        time.Jumuah2.Should().BeNull();
    }

    [Fact]
    public async Task GetPrayerTimesAsync_GetWholeYear_ValidValuesThroughout()
    {
        // ARRANGE
        var date = new LocalDate(2024, 8, 29);
        string externalID = "hamza-koln";

        // ACT
        var response = await _mawaqitApiService.GetPrayerTimesAsync(externalID, cancellationToken: default);
        var times = response.ToMawaqitPrayerTimes(externalID).ToList();

        // ASSERT
        times.Should().HaveCount(366);
        times.Select(x => x.Date).Should().OnlyHaveUniqueItems();
        times.Min(x => x.Date).Should().Be(new LocalDate(2024, 1, 1));
        times.Max(x => x.Date).Should().Be(new LocalDate(2024, 12, 31));

        times.Should().AllSatisfy(time =>
        {
            time.Should().NotBeNull();
            time.ExternalID.Should().Be(externalID);

            time.Fajr.Should().BeLessThanOrEqualTo(time.FajrCongregation);
            time.Dhuhr.Should().BeLessThanOrEqualTo(time.DhuhrCongregation);
            time.Asr.Should().BeLessThanOrEqualTo(time.AsrCongregation);
            time.Maghrib.Should().BeLessThanOrEqualTo(time.MaghribCongregation);
            time.Isha.Should().BeLessThanOrEqualTo(time.IshaCongregation);

            time.Jumuah.Should().NotBeNull();
            time.Jumuah.Value.Should().BeGreaterThanOrEqualTo(time.Dhuhr);
            time.Jumuah.Value.Should().BeGreaterThanOrEqualTo(time.DhuhrCongregation);
            time.Jumuah2.Should().BeNull();
        });
    }

    [Fact]
    public async Task ValidateData_ValidExternalID_ReturnTrue()
    {
        // ARRANGE
        var date = new LocalDate(2024, 8, 30);
        string externalID = "hamza-koln";

        // ACT
        bool response = await _mawaqitApiService.ValidateData(externalID, cancellationToken: default);

        // ASSERT
        response.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateData_InvalidExternalID_ReturnFalse()
    {
        // ARRANGE
        var date = new LocalDate(2024, 8, 30);
        string externalID = "InvalidExternalID";

        // ACT
        bool response = await _mawaqitApiService.ValidateData(externalID, cancellationToken: default);

        // ASSERT
        response.Should().BeFalse();
    }
}
