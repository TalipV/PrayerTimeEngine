using NodaTime;
using NSubstitute;
using PrayerTimeEngine.Core.Data.WebSocket.Interfaces;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.MyMosq.Models.Entities;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.MyMosq.Services;
using PrayerTimeEngine.Core.Tests.Common;
using PrayerTimeEngine.Core.Tests.Common.TestData;

namespace PrayerTimeEngine.Core.Tests.Unit.Domain.DynamicPrayerTimeProviders.MyMosq;

public class MyMosqApiServiceTests : BaseTest
{
    private readonly IWebSocketClient _mockWebSocketClient;
    private readonly IWebSocketClientFactory _mockWebSocketClientFactory;
    private readonly MyMosqApiService _myMosqApiService;

    public MyMosqApiServiceTests()
    {
        _mockWebSocketClient = SubstitutionHelper.GetMockedMyMosqWebSocketClient();
        _mockWebSocketClientFactory = Substitute.For<IWebSocketClientFactory>();
        _mockWebSocketClientFactory.CreateWebSocketClient().Returns(_mockWebSocketClient);
        _myMosqApiService = new MyMosqApiService(_mockWebSocketClientFactory);
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
        MyMosqPrayerTimes time = times.FirstOrDefault(x => x.Date == date);

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

        time.Jumuah.Should().Be(new LocalTime(0, 0, 0));
        time.Jumuah2.Should().Be(new LocalTime(0, 0, 0));
    }
}
