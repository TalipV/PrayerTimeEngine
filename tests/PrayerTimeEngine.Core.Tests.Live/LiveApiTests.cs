using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Interfaces;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Models.DTOs;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Muwaqqit.Interfaces;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Muwaqqit.Models.DTOs;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Interfaces;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Models.DTOs;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.Mawaqit.Interfaces;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers.Mawaqit.Models.DTOs;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Models;

namespace PrayerTimeEngine.Core.Tests.Live;

// Deliberately simple smoke tests against the real external APIs: they only check that each API
// answers and the answer still roughly has the expected shape. They run nightly (see the
// live-api-tests workflow) so an outage or a breaking change is noticed before the app needs
// the next day's data. MyMosq has its own, older tests in MyMosqLiveApiTests.
public class LiveApiTests : LiveApiTestBase
{
    private static readonly LocalDate s_today = SystemClock.Instance.GetCurrentInstant().InUtc().Date;

    [Fact]
    public async Task Fazilet_GetTimesForInnsbruck_ContainsTodayAndTomorrow()
    {
        // ARRANGE
        IFaziletApiService apiService = ServiceProvider.GetRequiredService<IFaziletApiService>();

        // ACT
        FaziletGetCountriesResponseDTO countries = await apiService.GetCountries(default);
        FaziletCountryResponseDTO austria = countries.Countries.Single(x => x.Name == "Avusturya");
        List<FaziletCityResponseDTO> cities = await apiService.GetCitiesByCountryID(austria.ID, default);
        FaziletCityResponseDTO innsbruck = cities.Single(x => x.Name == "Innsbruck");
        FaziletGetTimesByCityIDResponseDTO times = await apiService.GetTimesByCityID(innsbruck.ID, default);

        // ASSERT
        times.Timezone.Should().NotBeNull();
        times.PrayerTimes.Select(x => x.Date).Should().Contain([s_today, s_today.PlusDays(1)]);
    }

    [Fact]
    public async Task Semerkand_GetTimesForInnsbruck_ContainsToday()
    {
        // ARRANGE
        ISemerkandApiService apiService = ServiceProvider.GetRequiredService<ISemerkandApiService>();

        // ACT
        List<SemerkandCountryResponseDTO> countries = await apiService.GetCountries(default);
        SemerkandCountryResponseDTO austria = countries.Single(x => x.Name == "Avusturya");
        List<SemerkandCityResponseDTO> cities = await apiService.GetCitiesByCountryID(austria.ID, default);
        SemerkandCityResponseDTO innsbruck = cities.Single(x => x.Name == "Innsbruck");
        List<SemerkandPrayerTimesResponseDTO> times = await apiService.GetTimesByCityID(s_today.Year, innsbruck.ID, default);

        // ASSERT
        SemerkandPrayerTimesResponseDTO todaysTimes = times.Single(x => x.DayOfYear == s_today.DayOfYear);
        todaysTimes.Fajr.Should().NotBeNull();
        todaysTimes.Maghrib.Should().NotBeNull();
    }

    [Fact]
    public async Task Muwaqqit_GetTimesForInnsbruck_ReturnsTimes()
    {
        // ARRANGE
        IMuwaqqitApiService apiService = ServiceProvider.GetRequiredService<IMuwaqqitApiService>();

        // ACT
        MuwaqqitPrayerTimesResponseDTO times = await apiService.GetPrayerTimesAsync(
            date: s_today.ToString("yyyy-MM-dd", null),
            longitude: 11.4041024M,
            latitude: 47.2692124M,
            timezone: "Europe/Vienna",
            fajrDegree: -12,
            asrKarahaDegree: 4.5,
            ishtibakDegree: -8,
            ishaDegree: -15.5,
            cancellationToken: default);

        // ASSERT
        times.Fajr.Should().NotBeNull();
        times.Dhuhr.Should().NotBeNull();
        times.Fajr.Value.Date.Should().Be(s_today);
    }

    [Fact]
    public async Task Mawaqit_GetTimesForHamzaMosque_ReturnsTimes()
    {
        // ARRANGE
        IMawaqitApiService apiService = ServiceProvider.GetRequiredService<IMawaqitApiService>();

        // ACT
        MawaqitResponseDTO response = await apiService.GetPrayerTimesAsync("hamza-koln", default);

        // ASSERT
        response.TimeZone.Should().NotBeNullOrWhiteSpace();
        response.PrayerTimeDays.Should().NotBeEmpty();
    }

    [Fact]
    public async Task LocationIQ_SearchInnsbruck_FindsPlaceWithTimezone()
    {
        // ARRANGE
        IPlaceService placeService = ServiceProvider.GetRequiredService<IPlaceService>();

        // ACT
        List<BasicPlaceInfo> places = await placeService.SearchPlacesAsync("Innsbruck", "de", default);
        ProfilePlaceInfo placeWithTimezone = await placeService.GetTimezoneInfo(places.First(), default);

        // ASSERT
        places.Should().NotBeEmpty();
        placeWithTimezone.TimezoneInfo.Should().NotBeNull();
    }
}
