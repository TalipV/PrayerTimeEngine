using Microsoft.Extensions.Logging;
using NSubstitute;
using PrayerTimeEngine.Core.Common;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Models;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Services;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Services.LocationIQ;
using PrayerTimeEngine.Core.Tests.Common;
using PrayerTimeEngine.Core.Tests.Common.TestData;
using Refit;
using System.Net;

namespace PrayerTimeEngine.Core.Tests.Unit.Domain.PlaceManagement;

public class PlaceServiceTests : BaseTest
{
    private readonly MockHttpMessageHandler _mockHttpMessageHandler;
    private readonly PlaceService _placeService;

    public PlaceServiceTests()
    {
        _mockHttpMessageHandler = new MockHttpMessageHandler();
        var httpClient = new HttpClient(_mockHttpMessageHandler)
        {
            BaseAddress = new Uri("https://eu1.locationiq.com/v1/")
        };
        _placeService = new PlaceService(
            RestService.For<ILocationIQApiService>(httpClient),
            Substitute.For<ISystemInfoService>(),
            Substitute.For<ILogger<PlaceService>>());
    }

    #region SearchPlacesAsync

    [Fact]
    [Trait("Method", "SearchPlacesAsync")]
    public async Task SearchPlacesAsync_WithValidSearchTerm_ReturnsPlaces()
    {
        // ARRANGE
        _mockHttpMessageHandler.HandleRequestFunc = (request) =>
        {
            Stream responseStream = File.OpenRead(Path.Combine(TestDataHelper.LOCATIONIQ_TEST_DATA_FILE_PATH, "SearchedPlacesInfoCologneGrandMosqueAddress.txt"));

            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StreamContent(responseStream)
            };
        };

        // ACT
        List<BasicPlaceInfo> result =
            await _placeService.SearchPlacesAsync(
                searchTerm: "Venloer Str. 160, 50823 Köln, Germany",
                language: "en",
                cancellationToken: default);

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

        result[0].ExternalID.Should().Be("151467218");
        result[0].Longitude.Should().Be(6.92839361M);
        result[0].Latitude.Should().Be(50.9457534M);

        result[1].ExternalID.Should().Be("2603567246");
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
            Stream responseStream = File.OpenRead(Path.Combine(TestDataHelper.LOCATIONIQ_TEST_DATA_FILE_PATH, "PlaceInfoByPlaceData.txt"));

            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StreamContent(responseStream)
            };
        };

        var cologneCentralMosqueInfo =
            new BasicPlaceInfo
            {
                ExternalID = "151467218",
                City = "Cologne",
                CityDistrict = "Ehrenfeld",
                Country = "Germany",
                InfoLanguageCode = "en",
                PostCode = "50823",
                Street = "Venloer Straße 160",
                Longitude = 50.9457534M,
                Latitude = 6.92839361M
            };

        // ACT
        BasicPlaceInfo result = await _placeService.GetPlaceBasedOnPlace(cologneCentralMosqueInfo, "de", default);

        // ASSERT
        result.Should().NotBeNull();
        result.ExternalID.Should().Be("151467218");
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
            Stream responseStream = File.OpenRead(Path.Combine(TestDataHelper.LOCATIONIQ_TEST_DATA_FILE_PATH, "TimezoneInfoCET.txt"));

            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StreamContent(responseStream)
            };
        };

        var somePlaceInfo =
            new BasicPlaceInfo
            {
                ExternalID = "",
                Longitude = 1M,
                Latitude = 1M,
                InfoLanguageCode = "",
                Country = "",
                City = "",
                CityDistrict = "",
                PostCode = "",
                Street = ""
            };

        // ACT
        var result = await _placeService.GetTimezoneInfo(somePlaceInfo, default);

        // ASSERT
        result.TimezoneInfo.Should().NotBeNull();
        result.TimezoneInfo.DisplayName.Should().Be("CET");
        result.TimezoneInfo.Name.Should().Be("Central European Time");
        result.TimezoneInfo.UtcOffsetSeconds.Should().Be(3600);
    }

    #endregion GetTimezoneInfo
}