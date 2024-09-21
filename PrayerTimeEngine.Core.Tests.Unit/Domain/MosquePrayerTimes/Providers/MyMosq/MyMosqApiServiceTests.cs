using NodaTime;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.MyMosq.Models.Entities;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.MyMosq.Services;
using PrayerTimeEngine.Core.Tests.Common;
using PrayerTimeEngine.Core.Tests.Common.TestData;

namespace PrayerTimeEngine.Core.Tests.Unit.Domain.MosquePrayerTimes.Providers.MyMosq;

public class MyMosqApiServiceTests : BaseTest
{
    private readonly MyMosqApiService _myMosqApiService;

    public MyMosqApiServiceTests()
    {
        _myMosqApiService = SubstitutionHelper.GetMockedMyMosqApiService();
    }

    [Fact]
    public async Task GetPrayerTimesAsync_IrrelevantInput_MockedHttpResults()
    {
        // ARRANGE
        var date = new LocalDate(2024, 8, 30);
        string externalID = "1239";

        // ACT
        var response = await _myMosqApiService.GetPrayerTimesAsync(date, externalID, cancellationToken: default);
        var times = response.Select(x => x.ToMyMosqPrayerTimes(externalID)).ToList();
        MyMosqMosqueDailyPrayerTimes time = times.FirstOrDefault(x => x.Date == date);

        // ASSERT
        time.Should().NotBeNull();

        time.Date.Should().Be(new LocalDate(2024, 8, 30));
        time.ExternalID.Should().Be(externalID);

        time.Fajr.Should().Be(new LocalTime(04, 38, 00));
        time.FajrCongregation.Should().Be(new LocalTime(05, 51, 00));
        time.Shuruq.Should().Be(new LocalTime(06, 36, 00));
        time.Dhuhr.Should().Be(new LocalTime(14, 30, 00));    // WHY? BECAUSE OF JUMU'AH? THEN WHY IS THE JUMU'AH FIELD 0:00?
        time.DhuhrCongregation.Should().Be(new LocalTime(13, 38, 00));
        time.Asr.Should().Be(new LocalTime(17, 22, 00));
        time.AsrCongregation.Should().Be(new LocalTime(17, 22, 00));
        time.Maghrib.Should().Be(new LocalTime(20, 30, 00));
        time.MaghribCongregation.Should().Be(new LocalTime(20, 30, 00));
        time.Isha.Should().Be(new LocalTime(22, 00, 00));
        time.IshaCongregation.Should().Be(new LocalTime(22, 00, 00));

        time.Jumuah.Should().Be(new LocalTime(14, 30, 0));
        time.Jumuah2.Should().BeNull();
    }

    [Fact]
    public async Task GetPrayerTimesAsync_GetWholeYear_ValidValuesThroughout()
    {
        // ARRANGE
        var date = new LocalDate(2024, 8, 30);
        string externalID = "1239";

        // ACT
        var response = await _myMosqApiService.GetPrayerTimesAsync(date, externalID, cancellationToken: default);
        var times = response.Select(x => x.ToMyMosqPrayerTimes(externalID)).ToList();

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
            time.Asr.Should().BeLessThanOrEqualTo(time.AsrCongregation);
            time.Maghrib.Should().BeLessThanOrEqualTo(time.MaghribCongregation);

            // BUG!!?
            time.Dhuhr.Should().BeLessThanOrEqualTo(time.DhuhrCongregation);
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
        string externalID = "1239";

        // ACT
        bool response = await _myMosqApiService.ValidateData(externalID, cancellationToken: default);

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
        bool response = await _myMosqApiService.ValidateData(externalID, cancellationToken: default);

        // ASSERT
        response.Should().BeFalse();
    }
}
