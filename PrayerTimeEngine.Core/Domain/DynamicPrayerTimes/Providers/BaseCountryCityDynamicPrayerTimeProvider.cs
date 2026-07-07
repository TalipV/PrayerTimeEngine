using Microsoft.Extensions.Logging;
using NodaTime;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Models;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers;

/// <summary>
/// Base class for providers (e.g. Fazilet and Semerkand) which determine their location data
/// by looking up the country name and then the city name within that country.
///
/// The place info usually doesn't exactly match the names the provider knows, so multiple
/// name versions are tried: the exact names, the names with turkish 'İ' replaced by 'I',
/// the turkish versions of the names (retrieved through <see cref="IPlaceService"/>) and
/// provider specific special cases (e.g. "Mekke-i Mükerreme" for "Mekke" for Fazilet).
/// </summary>
public abstract class BaseCountryCityDynamicPrayerTimeProvider(
        IPlaceService placeService,
        ILogger logger
    ) : IDynamicPrayerTimeProvider
{
    public abstract Task<List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)>> GetPrayerTimesAsync(
        ZonedDateTime date, 
        BaseLocationData locationData, 
        List<GenericSettingConfiguration> configurations, 
        CancellationToken cancellationToken);

    public abstract HashSet<ETimeType> GetUnsupportedTimeTypes();

    /// <summary>
    /// Determines the ID of the country with the given name. Loads and stores the country data of the API if none is stored yet.
    /// The name has to match exactly, alternative name versions are handled by this base class.
    /// </summary>
    protected abstract Task<int> GetCountryID(string countryName, bool throwIfNotFound, CancellationToken cancellationToken);

    /// <summary>
    /// Determines the ID of the city with the given name within the given country. Loads and stores the city data of the API if none is stored yet.
    /// The name has to match exactly, alternative name versions are handled by this base class.
    /// </summary>
    protected abstract Task<int> GetCityID(string cityName, int countryID, bool throwIfNotFound, CancellationToken cancellationToken);

    /// <summary>
    /// Creates the provider specific location data for successfully found country and city names.
    /// </summary>
    protected abstract BaseLocationData CreateLocationData(string countryName, string cityName, ProfilePlaceInfo place);

    public async Task<BaseLocationData> GetLocationInfo(ProfilePlaceInfo place, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(place);

        logger.LogDebug("{ProviderTypeName} search location: {Country}, {City}", GetType().Name, place.Country, place.City ?? place.State);

        // country and city are resolved independently of each other, i.e. when the country
        // was already found through its native name then a problem with the city name
        // only leads to new attempts for the city name

        BasicPlaceInfo turkishPlaceInfo = null;

        // lazy retrieval because the place service is rate limited and only necessary when the native names don't suffice
        async Task<BasicPlaceInfo> getTurkishPlaceInfoAsync()
        {
            // the native names are already the turkish ones
            if (place.InfoLanguageCode?.ToLower() == "tr")
                return null;

            return turkishPlaceInfo ??= await placeService.GetPlaceBasedOnPlace(place, "tr", cancellationToken).ConfigureAwait(false);
        }

        (int countryID, string matchedCountryName) = await findCountry(place.Country, getTurkishPlaceInfoAsync, cancellationToken).ConfigureAwait(false);
        if (countryID == -1)
        {
            logger.LogDebug("No {ProviderTypeName} location search result because country could not be found", GetType().Name);
            return null;
        }

        (int cityID, string matchedCityName) = await findCity(place.City ?? place.State, countryID, getTurkishPlaceInfoAsync, cancellationToken).ConfigureAwait(false);
        if (cityID == -1)
        {
            logger.LogDebug("No {ProviderTypeName} location search result because city could not be found", GetType().Name);
            return null;
        }

        logger.LogDebug("{ProviderTypeName} found location: {Country}, {City}", GetType().Name, matchedCountryName, matchedCityName);

        // note: the matched name versions are stored (instead of the input names)
        // because later look ups (e.g. for the prayer time retrieval) require exact matches
        return CreateLocationData(matchedCountryName, matchedCityName, place);
    }

    private async Task<(int CountryID, string MatchedCountryName)> findCountry(
        string countryName,
        Func<Task<BasicPlaceInfo>> getTurkishPlaceInfoAsync,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(countryName))
        {
            (int CountryID, string MatchedCountryName) result = await findCountryIDByNameVariants(countryName, cancellationToken).ConfigureAwait(false);

            if (result.CountryID != -1)
                return result;
        }

        string turkishCountryName = (await getTurkishPlaceInfoAsync().ConfigureAwait(false))?.Country;

        if (!string.IsNullOrWhiteSpace(turkishCountryName) && turkishCountryName != countryName)
        {
            return await findCountryIDByNameVariants(turkishCountryName, cancellationToken).ConfigureAwait(false);
        }

        return (-1, null);
    }

    private async Task<(int CityID, string MatchedCityName)> findCity(
        string cityName,
        int countryID,
        Func<Task<BasicPlaceInfo>> getTurkishPlaceInfoAsync,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(cityName))
        {
            (int CityID, string MatchedCityName) result = await findCityIDByNameVariants(cityName, countryID, cancellationToken).ConfigureAwait(false);

            if (result.CityID != -1)
                return result;
        }

        BasicPlaceInfo turkishPlaceInfo = await getTurkishPlaceInfoAsync().ConfigureAwait(false);
        string turkishCityName = turkishPlaceInfo?.City ?? turkishPlaceInfo?.State;

        if (!string.IsNullOrWhiteSpace(turkishCityName) && turkishCityName != cityName)
        {
            return await findCityIDByNameVariants(turkishCityName, countryID, cancellationToken).ConfigureAwait(false);
        }

        return (-1, null);
    }

    private async Task<(int CountryID, string MatchedCountryName)> findCountryIDByNameVariants(string countryName, CancellationToken cancellationToken)
    {
        foreach (string countryNameVariant in GetCountryNameVariants(countryName).Distinct())
        {
            int countryID = await GetCountryID(countryNameVariant, throwIfNotFound: false, cancellationToken).ConfigureAwait(false);

            if (countryID != -1)
            {
                return (countryID, countryNameVariant);
            }
        }

        return (-1, null);
    }

    private async Task<(int CityID, string MatchedCityName)> findCityIDByNameVariants(string cityName, int countryID, CancellationToken cancellationToken)
    {
        foreach (string cityNameVariant in GetCityNameVariants(cityName).Distinct())
        {
            int cityID = await GetCityID(cityNameVariant, countryID, throwIfNotFound: false, cancellationToken).ConfigureAwait(false);

            if (cityID != -1)
            {
                return (cityID, cityNameVariant);
            }
        }

        return (-1, null);
    }

    protected virtual IEnumerable<string> GetCountryNameVariants(string countryName)
    {
        yield return countryName.Replace("İ", "I");
        yield return countryName;
    }

    protected virtual IEnumerable<string> GetCityNameVariants(string cityName)
    {
        yield return cityName.Replace("İ", "I");
        yield return cityName;
    }
}
