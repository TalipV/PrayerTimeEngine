using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Services;
using PrayerTimeEngine.Core.Tests.Common;
using System.Net;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Models.Common;

namespace PrayerTimeEngine.Core.Tests.Unit.Domain.PlaceManagement
{
    public class PlaceServiceTests : BaseTest
    {
        private readonly MockHttpMessageHandler _mockHttpMessageHandler;
        private readonly PlaceService _placeService;

        public PlaceServiceTests()
        {
            _mockHttpMessageHandler = new MockHttpMessageHandler();
            HttpClient httpClient = new HttpClient(_mockHttpMessageHandler);
            _placeService = new PlaceService(httpClient, Substitute.For<ILogger<PlaceService>>());
        }

        #region SearchPlacesAsync

        [Fact]
        [Trait("Method", "SearchPlacesAsync")]
        public async Task SearchPlacesAsync_WithValidSearchTerm_ReturnsPlaces()
        {
            // ARRANGE
            _mockHttpMessageHandler.HandleRequestFunc = (request) =>
            {
                string searchedPlacesJsonTxtPath = Path.Combine(TEST_DATA_FILE_PATH, "LocationIQTestData", "SearchedPlacesInfoCologneGrandMosqueAddress.txt");
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(File.ReadAllText(searchedPlacesJsonTxtPath))
                };
            };

            // ACT
            List<BasicPlaceInfo> result = await _placeService.SearchPlacesAsync(searchTerm: "Venloer Str. 160, 50823 Köln, Germany", language: "en");

            // ASSERT
            result.Should().HaveCount(2);
            result.Should().AllSatisfy(placeInfo =>
            {
                placeInfo.Should().NotBeNull();
                placeInfo.City.Should().Be("Cologne");
                placeInfo.CityDistrict.Should().Be("Ehrenfeld");
                placeInfo.Country.Should().Be("Germany");
                placeInfo.DisplayText.Should().Be("Germany, Cologne, Ehrenfeld, Venloer Straße 160");
                placeInfo.InfoLanguageCode.Should().Be("en");
                placeInfo.PostCode.Should().Be("50823");
                placeInfo.Street.Should().Be("Venloer Straße 160");
            });

            result[0].ID.Should().Be("151467218");
            result[0].Longitude.Should().Be(6.92839361M);
            result[0].Latitude.Should().Be(50.9457534M);

            result[1].ID.Should().Be("2603567246");
            result[1].Longitude.Should().Be(6.9281253M);
            result[1].Latitude.Should().Be(50.9460574M);
        }

        #endregion SearchPlacesAsync

        #region GetPlaceBasedOnPlace

        [Fact]
        [Trait("Method", "GetPlaceBasedOnPlace")]
        public async Task GetPlaceBasedOnPlace_WithValidPlace_ReturnsPlaceInfo()
        {
            // ARRANGE
            _mockHttpMessageHandler.HandleRequestFunc = (request) =>
            {
                string placeInfoByPlaceDataJsonPath = Path.Combine(TEST_DATA_FILE_PATH, "LocationIQTestData", "PlaceInfoByPlaceData.txt");
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(File.ReadAllText(placeInfoByPlaceDataJsonPath))
                };
            };

            var cologneCentralMosqueInfo = new BasicPlaceInfo
            (
                id: "151467218",
                city: "Cologne",
                cityDistrict: "Ehrenfeld",
                country: "Germany",
                infoLanguageCode: "en",
                postCode: "50823",
                street: "Venloer Straße 160",
                latitude: 50.9457534M,
                longitude: 6.92839361M
            );

            // ACT
            BasicPlaceInfo result = await _placeService.GetPlaceBasedOnPlace(cologneCentralMosqueInfo, "de");

            // ASSERT
            result.Should().NotBeNull();
            result.ID.Should().Be("151467218");
            result.Longitude.Should().Be(6.928393614248124M);
            result.Latitude.Should().Be(50.9457534M);
            result.City.Should().Be("Köln");
            result.CityDistrict.Should().Be("Ehrenfeld");
            result.Country.Should().Be("Deutschland");
            result.DisplayText.Should().Be("Deutschland, Köln, Ehrenfeld, Venloer Straße 160");
            result.InfoLanguageCode.Should().Be("de");
            result.PostCode.Should().Be("50823");
            result.Street.Should().Be("Venloer Straße 160");
        }

        #endregion GetPlaceBasedOnPlace

        #region GetTimezoneInfo

        [Fact]
        [Trait("Method", "GetTimezoneInfo")]
        public async Task GetTimezoneInfo_WithValidPlace_ReturnsCorrectTimezoneInfo()
        {
            // ARRANGE
            _mockHttpMessageHandler.HandleRequestFunc = (request) =>
            {
                string timezoneJsonTxtPath = Path.Combine(TEST_DATA_FILE_PATH, "LocationIQTestData", "TimezoneInfoCET.txt");
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(File.ReadAllText(timezoneJsonTxtPath))
                };
            };

            BasicPlaceInfo somePlaceInfo = new BasicPlaceInfo("", 1M, 1M, "", "", "", "", "", "");

            // ACT
            var result = await _placeService.GetTimezoneInfo(somePlaceInfo);

            // ASSERT
            result.TimezoneInfo.Should().NotBeNull();
            result.TimezoneInfo.DisplayName.Should().Be("CET");
            result.TimezoneInfo.Name.Should().Be("Central European Time");
            result.TimezoneInfo.UtcOffsetSeconds.Should().Be(3600);
        }

        #endregion GetTimezoneInfo
    }
}