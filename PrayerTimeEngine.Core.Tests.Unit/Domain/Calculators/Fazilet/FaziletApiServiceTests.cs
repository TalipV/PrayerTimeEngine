using FluentAssertions;
using NodaTime;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Services;
using PrayerTimeEngine.Core.Tests.Common;
using PrayerTimeEngine.Core.Tests.Common.TestData;
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
            var httpClient = new HttpClient(_mockHttpMessageHandler) { BaseAddress = new Uri("https://test.com/") };
            _faziletApiService = new FaziletApiService(httpClient);
        }

        [Fact]
        public async Task GetCountries_ReadTestDataFileForCountries_RoughlyValidData()
        {
            // ARRANGE
            _mockHttpMessageHandler.HandleRequestFunc =
                (request) =>
                {
                    Stream responseStream = File.OpenRead(Path.Combine(TestDataHelper.FAZILET_TEST_DATA_FILE_PATH, "Fazilet_TestCountriesData.txt"));

                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StreamContent(responseStream)
                    };
                };

            // ACT
            var countries = (await _faziletApiService.GetCountries(default)).Countries;

            // ASSERT
            countries.Should().HaveCount(208);
            countries.Should().AllSatisfy(country =>
            {
                country.Should().NotBeNull();
                country.Name.Should().NotBeNullOrWhiteSpace();
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
                    Stream responseStream = File.OpenRead(Path.Combine(TestDataHelper.FAZILET_TEST_DATA_FILE_PATH, "Fazilet_TestCityData_Austria.txt"));

                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StreamContent(responseStream)
                    };
                };

            // ACT
            var cities = await _faziletApiService.GetCitiesByCountryID(1, default);

            // ASSERT
            cities.Should().HaveCount(161);
            cities.Should().AllSatisfy(city =>
            {
                city.Should().NotBeNull();
                city.Name.Should().NotBeNullOrWhiteSpace();
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
                    Stream responseStream = File.OpenRead(Path.Combine(TestDataHelper.FAZILET_TEST_DATA_FILE_PATH, "Fazilet_TestPrayerTimeData_20230729_Innsbruck.txt"));

                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StreamContent(responseStream)
                    };
                };

            // ACT
            var times = await _faziletApiService.GetTimesByCityID(197, default);

            // ASSERT
            LocalDate assertDate = date.PlusDays(-1);
            times.PrayerTimes.Should().HaveCount(3);
            times.PrayerTimes.Should().AllSatisfy(time =>
            {
                time.Should().NotBeNull();
                time.Date.Should().Be(assertDate);
                assertDate = assertDate.PlusDays(1);
            });
        }
    }
}
