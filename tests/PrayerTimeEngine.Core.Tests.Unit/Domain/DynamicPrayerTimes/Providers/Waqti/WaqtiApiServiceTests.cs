using System.Net;
using NodaTime;
using PrayerTimeEngine.Core.Data.JsonSerialization;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Waqti.Models.DTOs;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Waqti.Models.Entities;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Waqti.Services;
using PrayerTimeEngine.Core.Tests.Common;
using PrayerTimeEngine.Core.Tests.Common.TestData;
using Refit;

namespace PrayerTimeEngine.Core.Tests.Unit.Domain.DynamicPrayerTimes.Providers.Waqti;

public class WaqtiApiServiceTests : BaseTest
{
    private readonly MockHttpMessageHandler _mockHttpMessageHandler;
    private readonly IWaqtiApiClient _waqtiApiClient;

    public WaqtiApiServiceTests()
    {
        _mockHttpMessageHandler = new MockHttpMessageHandler();
        var httpClient = new HttpClient(_mockHttpMessageHandler)
        {
            BaseAddress = new Uri(@"https://www.waqti.app/")
        };
        _waqtiApiClient = RestService.ForGenerated<IWaqtiApiClient>(httpClient, RefitJsonContext.Default);
    }

    private static WaqtiMonthPrayerTimesRequestDTO CreateInnsbruckRequest(string method) =>
        new()
        {
            Latitude = 47.2692124M,
            Longitude = 11.4041024M,
            Elevation = 0,
            Timezone = "Europe/Vienna",
            Year = 2023,
            Month = 7,
            Method = method,
        };

    private void RespondWithTestFile(string fileName, Action<HttpRequestMessage, string>? inspectRequest = null)
    {
        _mockHttpMessageHandler.HandleRequestFunc =
            (request) =>
            {
                inspectRequest?.Invoke(request, request.Content!.ReadAsStringAsync().GetAwaiter().GetResult());

                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StreamContent(File.OpenRead(Path.Combine(TestDataHelper.WAQTI_TEST_DATA_FILE_PATH, fileName)))
                };
            };
    }

    [Fact]
    public async Task GetMonthPrayerTimesAsync_InnsbruckRequest_PostsRequestBodyToMonthEndpoint()
    {
        // ARRANGE
        HttpMethod? requestMethod = null;
        string? requestUri = null;
        string? requestBody = null;
        string? requestContentType = null;
        RespondWithTestFile(
            "Waqti_TestPrayerTimeData_202307_Innsbruck_Mwl2007.txt",
            (request, body) =>
            {
                requestMethod = request.Method;
                requestUri = request.RequestUri!.AbsoluteUri;
                requestBody = body;
                requestContentType = request.Content!.Headers.ContentType?.MediaType;
            });

        // ACT
        await _waqtiApiClient.GetMonthPrayerTimesAsync(CreateInnsbruckRequest("Mwl2007"), default);

        // ASSERT
        requestMethod.Should().Be(HttpMethod.Post);
        requestUri.Should().Be("https://www.waqti.app/api/quarkus/prayer/times_monthCustom");
        requestContentType.Should().Be("application/json");
        requestBody.Should().Be(
            """{"latitude":47.2692124,"longitude":11.4041024,"elevation":0,"timezone":"Europe/Vienna","year":2023,"month":7,"method":"Mwl2007"}""");
    }

    [Theory]
    [InlineData(7.5, 10.0, @"""overrides"":{""fajrAngle"":7.5,""ishaaAngle"":10}")]
    [InlineData(4.5, null, @"""overrides"":{""fajrAngle"":4.5}")]
    [InlineData(null, -4.5, @"""overrides"":{""ishaaAngle"":-4.5}")]
    public async Task GetMonthPrayerTimesAsync_AngleOverrides_OnlySetAnglesSent(double? fajrAngle, double? ishaaAngle, string expectedOverridesJson)
    {
        // ARRANGE
        string? requestBody = null;
        RespondWithTestFile(
            "Waqti_TestPrayerTimeData_202307_Innsbruck_Mwl2007_Fajr7.5_Ishaa10.txt",
            (request, body) => requestBody = body);

        WaqtiMonthPrayerTimesRequestDTO baseRequest = CreateInnsbruckRequest("Mwl2007");
        var request = new WaqtiMonthPrayerTimesRequestDTO
        {
            Latitude = baseRequest.Latitude,
            Longitude = baseRequest.Longitude,
            Elevation = baseRequest.Elevation,
            Timezone = baseRequest.Timezone,
            Year = baseRequest.Year,
            Month = baseRequest.Month,
            Method = baseRequest.Method,
            Overrides = new WaqtiOverridesRequestDTO { FajrAngle = fajrAngle, IshaaAngle = ishaaAngle },
        };

        // ACT
        await _waqtiApiClient.GetMonthPrayerTimesAsync(request, default);

        // ASSERT
        requestBody.Should().Be(
            @"{""latitude"":47.2692124,""longitude"":11.4041024,""elevation"":0,""timezone"":""Europe/Vienna"",""year"":2023,""month"":7,""method"":""Mwl2007""," + expectedOverridesJson + "}");
    }

    [Fact]
    public async Task GetMonthPrayerTimesAsync_AngleOverrides_RawTimesOfTheAnglesMapped()
    {
        // ARRANGE
        RespondWithTestFile("Waqti_TestPrayerTimeData_202307_Innsbruck_Mwl2007_Fajr7.5_Ishaa10.txt");

        // ACT
        List<WaqtiDailyPrayerTimesResponseDTO> response =
            await _waqtiApiClient.GetMonthPrayerTimesAsync(CreateInnsbruckRequest("Mwl2007"), default);
        WaqtiDailyPrayerTimes time =
            response
                .Single(x => x.Date == new LocalDate(2023, 7, 30))
                .ToWaqtiPrayerTimes(longitude: 11.4041024M, latitude: 47.2692124M, fajrAngle: 7.5, ishaaAngle: 10);

        // ASSERT (raw times: the published ones would be 03:00 and 19:59 because of safety minutes and rounding)
        time.Fajr.Should().Be(Instant.FromUtc(2023, 7, 30, 3, 2, 30));
        time.Isha.Should().Be(Instant.FromUtc(2023, 7, 30, 19, 56, 51));
    }

    [Fact]
    public async Task GetMonthPrayerTimesAsync_InnsbruckJuly2023_EveryDayOfTheMonthReturned()
    {
        // ARRANGE
        RespondWithTestFile("Waqti_TestPrayerTimeData_202307_Innsbruck_Mwl2007.txt");

        // ACT
        List<WaqtiDailyPrayerTimesResponseDTO> response =
            await _waqtiApiClient.GetMonthPrayerTimesAsync(CreateInnsbruckRequest("Mwl2007"), default);

        // ASSERT
        response.Should().HaveCount(31);
        response.Select(x => x.Date).Should().BeEquivalentTo(
            Enumerable.Range(1, 31).Select(day => new LocalDate(2023, 7, day)),
            options => options.WithStrictOrdering());
        response.Should().OnlyContain(x => x.Method == "Mwl2007");
        response.Should().OnlyContain(x => x.Timezone == TestDataHelper.EUROPE_VIENNA_TIME_ZONE);
    }

    [Fact]
    public async Task GetMonthPrayerTimesAsync_Mwl2007_MappedToCorrectPrayerTimes()
    {
        // ARRANGE
        RespondWithTestFile("Waqti_TestPrayerTimeData_202307_Innsbruck_Mwl2007.txt");

        // ACT
        List<WaqtiDailyPrayerTimesResponseDTO> response =
            await _waqtiApiClient.GetMonthPrayerTimesAsync(CreateInnsbruckRequest("Mwl2007"), default);
        WaqtiDailyPrayerTimes time =
            response
                .Single(x => x.Date == new LocalDate(2023, 7, 30))
                .ToWaqtiPrayerTimes(longitude: 11.4041024M, latitude: 47.2692124M);

        // ASSERT
        time.Should().NotBeNull();

        time.ID.Should().Be(0);
        time.Date.Should().Be(new LocalDate(2023, 7, 30));
        time.TimeZone.Should().Be(TestDataHelper.EUROPE_VIENNA_TIME_ZONE);
        time.Method.Should().Be("Mwl2007");
        time.Latitude.Should().Be(47.2692124M);
        time.Longitude.Should().Be(11.4041024M);
        time.InsertInstant.Should().BeNull();

        time.Fajr.Should().Be(Instant.FromUtc(2023, 7, 30, 1, 28, 0));
        time.Shuruq.Should().Be(Instant.FromUtc(2023, 7, 30, 3, 47, 0));
        time.Duha.Should().Be(Instant.FromUtc(2023, 7, 30, 4, 24, 0));
        time.Dhuhr.Should().Be(Instant.FromUtc(2023, 7, 30, 11, 25, 0));
        time.Asr.Should().Be(Instant.FromUtc(2023, 7, 30, 15, 29, 0));
        time.AsrMithlayn.Should().Be(Instant.FromUtc(2023, 7, 30, 16, 37, 0));
        time.Maghrib.Should().Be(Instant.FromUtc(2023, 7, 30, 18, 53, 0));
        time.Isha.Should().Be(Instant.FromUtc(2023, 7, 30, 21, 1, 0));
        time.NextFajr.Should().Be(Instant.FromUtc(2023, 7, 31, 1, 30, 0));
    }

    [Fact]
    public async Task GetMonthPrayerTimesAsync_Diyanet_MappedToCorrectPrayerTimes()
    {
        // ARRANGE
        RespondWithTestFile("Waqti_TestPrayerTimeData_202307_Innsbruck_Diyanet.txt");

        // ACT
        List<WaqtiDailyPrayerTimesResponseDTO> response =
            await _waqtiApiClient.GetMonthPrayerTimesAsync(CreateInnsbruckRequest("Diyanet"), default);
        WaqtiDailyPrayerTimes time =
            response
                .Single(x => x.Date == new LocalDate(2023, 7, 30))
                .ToWaqtiPrayerTimes(longitude: 11.4041024M, latitude: 47.2692124M);

        // ASSERT
        time.Method.Should().Be("Diyanet");

        time.Fajr.Should().Be(Instant.FromUtc(2023, 7, 30, 1, 30, 0));
        time.Shuruq.Should().Be(Instant.FromUtc(2023, 7, 30, 3, 42, 0));
        time.Duha.Should().Be(Instant.FromUtc(2023, 7, 30, 4, 24, 0));
        time.Dhuhr.Should().Be(Instant.FromUtc(2023, 7, 30, 11, 27, 0));
        time.Asr.Should().Be(Instant.FromUtc(2023, 7, 30, 15, 31, 0));
        time.AsrMithlayn.Should().Be(Instant.FromUtc(2023, 7, 30, 16, 39, 0));
        time.Maghrib.Should().Be(Instant.FromUtc(2023, 7, 30, 18, 57, 0));
        time.Isha.Should().Be(Instant.FromUtc(2023, 7, 30, 20, 58, 0));
        time.NextFajr.Should().Be(Instant.FromUtc(2023, 7, 31, 1, 32, 0));
    }

    [Fact]
    public async Task GetMonthPrayerTimesAsync_ServerError_ThrowsApiException()
    {
        // ARRANGE
        _mockHttpMessageHandler.HandleRequestFunc =
            (request) => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                RequestMessage = request,
                Content = new StringContent("500 - Internal Server Error")
            };

        // ACT
        Func<Task> action = () => _waqtiApiClient.GetMonthPrayerTimesAsync(CreateInnsbruckRequest("UnknownMethod"), default);

        // ASSERT
        await action.Should().ThrowAsync<ApiException>();
    }
}
