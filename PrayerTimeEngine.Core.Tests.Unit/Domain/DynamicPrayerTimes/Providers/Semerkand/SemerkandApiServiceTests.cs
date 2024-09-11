using PrayerTimeEngine.Core.Tests.Common;
using System.Net;
using NodaTime;
using PrayerTimeEngine.Core.Tests.Common.TestData;
using Refit;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Interfaces;

namespace PrayerTimeEngine.Core.Tests.Unit.Domain.DynamicPrayerTimes.Providers.Semerkand;

public class SemerkandApiServiceTests : BaseTest
{
    private readonly MockHttpMessageHandler _mockHttpMessageHandler;
    private readonly ISemerkandApiService _semerkandApiService;

    public SemerkandApiServiceTests()
    {
        _mockHttpMessageHandler = new MockHttpMessageHandler();
        var httpClient = new HttpClient(_mockHttpMessageHandler) { BaseAddress = new Uri("https://semerkandtakvimi.semerkandmobile.com/") };
        _semerkandApiService = RestService.For<ISemerkandApiService>(httpClient);
    }

    [Fact]
    public async Task GetCountries_ReadTestDataFileForCountries_RoughlyValidData()
    {
        // ARRANGE
        _mockHttpMessageHandler.HandleRequestFunc =
            (request) =>
            {
                Stream responseStream = File.OpenRead(Path.Combine(TestDataHelper.SEMERKAND_TEST_DATA_FILE_PATH, "Semerkand_TestCountriesData.txt"));

                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StreamContent(responseStream)
                };
            };

        // ACT
        var countries = await _semerkandApiService.GetCountries(default);

        // ASSERT
        countries.Should().HaveCount(206);
        countries.Should().AllSatisfy(country =>
        {
            country.Should().NotBeNull();
            country.Name.Should().NotBeNullOrWhiteSpace();
            country.DisplayName.Should().NotBeNullOrWhiteSpace();
            country.ID.Should().BeGreaterThan(0);
        });
    }

    [Fact]
    public async Task GetCitiesByCountryID_ReadTestDataFileForCountries_RoughlyValidData()
    {
        // ARRANGE
        _mockHttpMessageHandler.HandleRequestFunc =
            (request) =>
            {
                Stream responseStream = File.OpenRead(Path.Combine(TestDataHelper.SEMERKAND_TEST_DATA_FILE_PATH, "Semerkand_TestCityData_Austria.txt"));

                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StreamContent(responseStream)
                };
            };

        // ACT
        var cities = await _semerkandApiService.GetCitiesByCountryID(1, default);

        // ASSERT
        cities.Should().HaveCount(204);
        cities.Should().AllSatisfy(city =>
        {
            city.Should().NotBeNull();
            city.Name.Should().NotBeNullOrWhiteSpace();
            city.DisplayName.Should().NotBeNullOrWhiteSpace();
            city.ID.Should().BeGreaterThan(0);
        });
    }

    [Fact]
    public async Task GetTimesByCityID_ReadTestDataFileForCountries_RoughlyValidData()
    {
        // ARRANGE
        var date = new LocalDate(2023, 7, 29);

        _mockHttpMessageHandler.HandleRequestFunc =
            (request) =>
            {
                Stream responseStream = File.OpenRead(Path.Combine(TestDataHelper.SEMERKAND_TEST_DATA_FILE_PATH, "Semerkand_TestPrayerTimeData_20230729_Innsbruck.txt"));

                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StreamContent(responseStream)
                };
            };

        // ACT
        var times =
            await _semerkandApiService.GetTimesByCityID(
                year: date.Year,
                cityID: 197,
                cancellationToken: default);

        // ASSERT
        int dayOfYear = 1;

        times.Should().HaveCount(365);
        times.Should().AllSatisfy(time =>
        {
            time.Should().NotBeNull();
            time.DayOfYear.Should().Be(dayOfYear++);
        });
    }
}
