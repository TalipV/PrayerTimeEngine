using FluentAssertions;
using NodaTime;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Services;
using PrayerTimeEngine.Core.Tests.Common;
using System.Net;

namespace PrayerTimeEngine.Core.Tests.Unit.Domain.Calculators.Fazilet
{
    public class FaziletApiServiceTests : BaseTest
    {
        private readonly MockHttpMessageHandler _mockHttpMessageHandler;
        private readonly FaziletApiService _faziletApiService;

        public FaziletApiServiceTests()
        {
            _mockHttpMessageHandler = new MockHttpMessageHandler();
            HttpClient httpClient = new HttpClient(_mockHttpMessageHandler) { BaseAddress = new Uri("https://test.com/") };
            _faziletApiService = new FaziletApiService(httpClient);
        }

        [Fact]
        public async Task GetCountries_ReadTestDataFileForCountries_RoughlyValidData()
        {
            // ARRANGE
            _mockHttpMessageHandler.HandleRequestFunc =
                (request) =>
                {
                    string responseText =
                        File.ReadAllText(
                            Path.Combine(
                                TEST_DATA_FILE_PATH,
                                "FaziletTestData",
                                "Fazilet_TestCountriesData.txt"));
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(responseText)
                    };
                };

            // ACT
            var countries = await _faziletApiService.GetCountries();

            // ASSERT
            countries.Should().HaveCount(208);
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
                    string responseText =
                        File.ReadAllText(
                            Path.Combine(
                                TEST_DATA_FILE_PATH,
                                "FaziletTestData",
                                "Fazilet_TestCityData_Austria.txt"));
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(responseText)
                    };
                };

            // ACT
            var cities = await _faziletApiService.GetCitiesByCountryID(1);

            // ASSERT
            cities.Should().HaveCount(161);
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
            LocalDate date = new LocalDate(2023, 7, 29);

            _mockHttpMessageHandler.HandleRequestFunc =
                (request) =>
                {
                    string responseText =
                        File.ReadAllText(
                            Path.Combine(
                                TEST_DATA_FILE_PATH,
                                "FaziletTestData",
                                "Fazilet_TestPrayerTimeData_20230729_Innsbruck.txt"));
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(responseText)
                    };
                };

            // ACT
            var times = await _faziletApiService.GetTimesByCityID(197);

            // ASSERT
            LocalDate assertDate = date.PlusDays(-1);
            times.Should().HaveCount(3);
            times.Should().AllSatisfy(time =>
            {
                time.Should().NotBeNull();
                time.Date.Should().Be(assertDate);
                time.CityID.Should().Be(197);

                assertDate = assertDate.PlusDays(1);
            });
        }
    }
}
