using AsyncKeyedLock;
using Microsoft.Extensions.Logging;
using NodaTime;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Interfaces;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Models;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Models.Entities;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Models;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Services;

public class FaziletDynamicPrayerTimeProvider(
        IFaziletDBAccess faziletDBAccess,
        IFaziletApiService faziletApiService,
        IPlaceService placeService,
        ILogger<FaziletDynamicPrayerTimeProvider> logger
    ) : IDynamicPrayerTimeProvider
{
    public HashSet<ETimeType> GetUnsupportedTimeTypes()
    {
        return _unsupportedTimeTypes;
    }

    private readonly HashSet<ETimeType> _unsupportedTimeTypes =
        [
            ETimeType.FajrGhalas,
            ETimeType.FajrKaraha,
            ETimeType.DuhaStart,
            ETimeType.DuhaQuarterOfDay,
            ETimeType.DuhaEnd,
            ETimeType.AsrMithlayn,
            ETimeType.AsrKaraha,
            ETimeType.MaghribIshtibaq,
        ];

    public async Task<List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)>> GetPrayerTimesAsync(
        ZonedDateTime date,
        BaseLocationData locationData,
        List<GenericSettingConfiguration> configurations,
        CancellationToken cancellationToken)
    {
        // check configuration's calculation sources?

        if (locationData is not FaziletLocationData faziletLocationData)
        {
            throw new Exception("Fazilet specific location information was not provided!");
        }

        string countryName = faziletLocationData.CountryName;
        string cityName = faziletLocationData.CityName;

        FaziletDailyPrayerTimes faziletPrayerTimes =
            await getPrayerTimesInternal(
                date,
                countryName,
                cityName, cancellationToken).ConfigureAwait(false);

        return configurations
            .Select(x => (x.TimeType, faziletPrayerTimes.GetZonedDateTimeForTimeType(x.TimeType)))
            .ToList();
    }

    private async Task<FaziletDailyPrayerTimes> getPrayerTimesInternal(ZonedDateTime date, string countryName, string cityName, CancellationToken cancellationToken)
    {
        int countryID = await getCountryID(countryName, throwIfNotFound: true, cancellationToken).ConfigureAwait(false);
        int cityID = await getCityID(cityName, countryID, throwIfNotFound: true, cancellationToken).ConfigureAwait(false);

        FaziletDailyPrayerTimes prayerTimes = await getPrayerTimesByDateAndCityID(date, cityID, cancellationToken).ConfigureAwait(false)
            ?? throw new Exception($"Prayer times for the {date} could not be found for an unknown reason.");

        prayerTimes.NextFajr = (await getPrayerTimesByDateAndCityID(date.Plus(Duration.FromDays(1)), cityID, cancellationToken).ConfigureAwait(false))?.Fajr;

        return prayerTimes;
    }

    private static readonly AsyncKeyedLocker<(ZonedDateTime date, int cityID)> getPrayerTimesLocker = new(o =>
    {
        o.MaxCount = 1;
        o.PoolSize = 20;
        o.PoolInitialFill = 1;
    });

    private async Task<FaziletDailyPrayerTimes> getPrayerTimesByDateAndCityID(ZonedDateTime date, int cityID, CancellationToken cancellationToken)
    {
        var lockTuple = (date, cityID);

        using (await getPrayerTimesLocker.LockAsync(lockTuple, cancellationToken).ConfigureAwait(false))
        {
            FaziletDailyPrayerTimes prayerTimes = await faziletDBAccess.GetTimesByDateAndCityID(date, cityID, cancellationToken).ConfigureAwait(false);

            if (prayerTimes is null)
            {
                var prayerTimesResponseDTO = await faziletApiService.GetTimesByCityID(cityID, cancellationToken).ConfigureAwait(false);
                var timeZone = prayerTimesResponseDTO.Timezone;
                var prayerTimesLst = prayerTimesResponseDTO.PrayerTimes.Select(x => x.ToFaziletPrayerTimes(cityID, timeZone)).ToList();
                await faziletDBAccess.InsertPrayerTimesAsync(prayerTimesLst, cancellationToken).ConfigureAwait(false);

                prayerTimes = prayerTimesLst.FirstOrDefault(x => x.Date.Date == date.Date);
            }

            return prayerTimes;
        }
    }

    private static readonly AsyncNonKeyedLocker semaphoreTryGetCityID = new(1);

    private async Task<int> getCityID(string cityName, int countryID, bool throwIfNotFound, CancellationToken cancellationToken)
    {
        // check-then-act has to be thread safe
        using (await semaphoreTryGetCityID.LockAsync(cancellationToken).ConfigureAwait(false))
        {
            int? cityID = await faziletDBAccess.GetCityIDByName(countryID, cityName, cancellationToken).ConfigureAwait(false);

            // city found
            if (cityID is not null)
                return cityID.Value;

            // unknown city
            if (await faziletDBAccess.HasCityData(countryID, cancellationToken).ConfigureAwait(false))
            {
                return throwIfNotFound
                    ? throw new ArgumentException($"{nameof(cityName)} could not be found!")
                    : -1;
            }

            // load cities through HTTP request and save them
            var cityDTOs = await faziletApiService.GetCitiesByCountryID(countryID, cancellationToken).ConfigureAwait(false);
            var cities = cityDTOs.Select(x => new FaziletCity { Name = x.Name, ID = x.ID, CountryID = countryID }).ToList();
            await faziletDBAccess.InsertCities(cities, cancellationToken).ConfigureAwait(false);

            if (cities.FirstOrDefault(x => x.Name == cityName)?.ID is int returnValue)
            {
                return returnValue;
            }

            // there were no cities and loaded cities still didn't contain it
            return throwIfNotFound
                ? throw new ArgumentException($"{nameof(cityName)} could not be found!")
                : -1;
        }
    }

    private static readonly AsyncNonKeyedLocker semaphoreTryGetCountryID = new(1);

    private async Task<int> getCountryID(string countryName, bool throwIfNotFound, CancellationToken cancellationToken)
    {
        // check-then-act has to be thread safe
        using (await semaphoreTryGetCountryID.LockAsync(cancellationToken).ConfigureAwait(false))
        {
            int? countryID = await faziletDBAccess.GetCountryIDByName(countryName, cancellationToken).ConfigureAwait(false);

            // country found
            if (countryID is not null)
                return countryID.Value;

            // unknown country
            if (await faziletDBAccess.HasCountryData(cancellationToken).ConfigureAwait(false))
            {
                return throwIfNotFound
                    ? throw new ArgumentException($"{nameof(countryName)} could not be found!")
                    : -1;
            }

            var countriesDTOs = (await faziletApiService.GetCountries(cancellationToken).ConfigureAwait(false)).Countries;
            var countries = countriesDTOs.Select(x => new FaziletCountry { Name = x.Name, ID = x.ID }).ToList();
            await faziletDBAccess.InsertCountries(countries, cancellationToken).ConfigureAwait(false);

            if (countries.FirstOrDefault(x => x.Name == countryName)?.ID is int returnValue)
            {
                return returnValue;
            }

            // there were no countries and loaded countries still didn't contain it
            return throwIfNotFound
                ? throw new ArgumentException($"{nameof(countryName)} could not be found!")
                : -1;
        }
    }

    public async Task<BaseLocationData> GetLocationInfo(ProfilePlaceInfo place, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(place);

        string countryName = place.Country;
        string placeName = place.City ?? place.State;

        BaseLocationData locationInfo = await getLocationInfoInternal(
            place.Country,
            place.City ?? place.State ?? "",
            cancellationToken).ConfigureAwait(false);

        if (locationInfo != null)
        {
            return locationInfo;
        }
        else if (place.InfoLanguageCode?.ToLower() == "tr")
        {
            // we have already tried turkish
            return null;
        }

        BasicPlaceInfo turkishPlaceInfo = await placeService.GetPlaceBasedOnPlace(place, "tr", cancellationToken).ConfigureAwait(false);
        
        string turkishCountryName = turkishPlaceInfo.Country;
        string turkishPlaceName = turkishPlaceInfo.City ?? turkishPlaceInfo.State;

        return await getLocationInfoInternal(turkishCountryName, turkishPlaceName, cancellationToken).ConfigureAwait(false)
            ?? await getLocationInfoInternal(countryName, turkishPlaceName, cancellationToken).ConfigureAwait(false)
            ?? await getLocationInfoInternal(turkishCountryName, placeName, cancellationToken).ConfigureAwait(false);
    }

    private async Task<BaseLocationData> getLocationInfoInternal(
        string countryName,
        string cityName,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Fazilet search location: {Country}, {City}", countryName, cityName);

        if (string.IsNullOrWhiteSpace(countryName) || string.IsNullOrWhiteSpace(cityName))
        {
            logger.LogDebug("No Fazilet location search result because of incomplete input data");
            return null;
        }

        int countryID = await getCountryIDWithAlternativeCountryNames(countryName, cancellationToken).ConfigureAwait(false);
        if (countryID == -1)
        {
            logger.LogDebug("No Fazilet location search result because country could not be found");
            return null;
        }

        int cityID = await getCityIDWithAlternativeCityNames(cityName, countryID, cancellationToken).ConfigureAwait(false);
        if (cityID == -1)
        {
            logger.LogDebug("No Fazilet location search result because city could not be found");
            return null;
        }

        logger.LogDebug("Fazilet found location: {Country}, {City}", countryName, cityName);

        return new FaziletLocationData
        {
            CountryName = countryName,
            CityName = cityName
        };
    }

    // TODO: I need tests which test whether the measures with the alternative city/country names works in all the different scenarios
    // like switching the country and/or city to the turkish version (i.e. both, neither, only city, only country)
    // or the simple Replace("İ", "I")

    private async Task<int> getCountryIDWithAlternativeCountryNames(string countryName, CancellationToken cancellationToken)
    {
        int countryID = -1;
        string[] toBeTriedCountryNames = [countryName.Replace("İ", "I"), countryName];

        foreach (string toBeTriedCountryName in toBeTriedCountryNames)
        {
            countryID = await getCountryID(
                toBeTriedCountryName,
                throwIfNotFound: false,
                cancellationToken).ConfigureAwait(false);

            if (countryID != -1)
            {
                break;
            }
        }

        return countryID;
    }

    private async Task<int> getCityIDWithAlternativeCityNames(string cityName, int countryID, CancellationToken cancellationToken)
    {
        int cityID = -1;
        string[] toBeTriedCityNames;

        if (cityName == "Mekke")
            toBeTriedCityNames = ["Mekke-i Mükerreme"];
        else if (cityName == "Medine")
            toBeTriedCityNames = ["Medîne-i Münevvere"];
        else
            toBeTriedCityNames = [cityName.Replace("İ", "I"), cityName];

        foreach (string toBeTriedCityName in toBeTriedCityNames)
        {
            cityID = await getCityID(
                toBeTriedCityName,
                countryID,
                throwIfNotFound: false,
                cancellationToken).ConfigureAwait(false);

            if (cityID != -1)
            {
                break;
            }
        }

        return cityID;
    }
}
