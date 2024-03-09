using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Services;
using PrayerTimeEngine.Core.Tests.Common;
using System.Net;
using FluentAssertions;
using NodaTime;

namespace PrayerTimeEngine.Core.Tests.Unit.Domain.Calculators.Semerkand
{
    public class SemerkandApiServiceTests : BaseTest
    {
        private readonly MockHttpMessageHandler _mockHttpMessageHandler;
        private readonly SemerkandApiService _semerkandApiService;

        public SemerkandApiServiceTests()
        {
            _mockHttpMessageHandler = new MockHttpMessageHandler();
            var httpClient = new HttpClient(_mockHttpMessageHandler);
            _semerkandApiService = new SemerkandApiService(httpClient);
        }

        [Fact]
        public async Task GetCountries_ReadTestDataFileForCountries_RoughlyValidData()
        {
            // ARRANGE
            _mockHttpMessageHandler.HandleRequestFunc =
                (request) =>
                {
                    Stream responseStream = File.OpenRead(Path.Combine(SEMERKAND_TEST_DATA_FILE_PATH, "Semerkand_TestCountriesData.txt"));

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
                country.Key.Should().NotBeNullOrWhiteSpace();
                country.Value.Should().BeGreaterThan(0);
            });
        }

        [Fact]
        public async Task GetCitiesByCountryID_ReadTestDataFileForCountries_RoughlyValidData()
        {
            // ARRANGE
            _mockHttpMessageHandler.HandleRequestFunc =
                (request) =>
                {
                    Stream responseStream = File.OpenRead(Path.Combine(SEMERKAND_TEST_DATA_FILE_PATH, "Semerkand_TestCityData_Austria.txt"));

                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StreamContent(responseStream)
                    };
                };
            
            // ACT
            var cities = await _semerkandApiService.GetCitiesByCountryID(1, default);

            // ASSERT
            cities.Should().HaveCount(195);
            cities.Should().AllSatisfy(city =>
            {
                city.Should().NotBeNull();
                city.Key.Should().NotBeNullOrWhiteSpace();
                city.Value.Should().BeGreaterThan(0);
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
                    Stream responseStream = File.OpenRead(Path.Combine(SEMERKAND_TEST_DATA_FILE_PATH, "Semerkand_TestPrayerTimeData_20230729_Innsbruck.txt"));

                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StreamContent(responseStream)
                    };
                };
            
            // ACT
            var times = 
                await _semerkandApiService.GetTimesByCityID(
                    date: date,
                    timezoneName: "Europe/Vienna",
                    cityID: 197,
                    cancellationToken: default);

            // ASSERT
            times.Should().HaveCount(SemerkandApiService.EXTENT_OF_DAYS_RETRIEVED);
            times.Should().AllSatisfy(time =>
            {
                time.Should().NotBeNull();
                time.Date.Should().Be(new LocalDate(2023, 1, 1).PlusDays(time.DayOfYear - 1));
                time.CityID.Should().Be(197);
            });
        }
    }
}
