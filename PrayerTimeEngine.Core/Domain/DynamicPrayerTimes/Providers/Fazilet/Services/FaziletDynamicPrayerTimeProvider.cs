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
        IFaziletRepository faziletRepository,
        IFaziletApiService faziletApiService,
        IPlaceService placeService,
        ILogger<FaziletDynamicPrayerTimeProvider> logger
    ) : BaseCountryCityDynamicPrayerTimeProvider(placeService, logger)
{
    public override HashSet<ETimeType> GetUnsupportedTimeTypes()
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

    public override async Task<List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)>> GetPrayerTimesAsync(
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
            .Select(x => (x.TimeType, ZonedDateTime: faziletPrayerTimes.GetZonedDateTimeForTimeType(x.TimeType)))
            .Where(x => x.ZonedDateTime is not null)    // missing times are simply not returned
            .Select(x => (x.TimeType, x.ZonedDateTime.Value))
            .ToList();
    }

    private async Task<FaziletDailyPrayerTimes> getPrayerTimesInternal(ZonedDateTime date, string countryName, string cityName, CancellationToken cancellationToken)
    {
        int countryID = await GetCountryID(countryName, throwIfNotFound: true, cancellationToken).ConfigureAwait(false);
        int cityID = await GetCityID(cityName, countryID, throwIfNotFound: true, cancellationToken).ConfigureAwait(false);

        FaziletDailyPrayerTimes prayerTimes = await getPrayerTimesByDateAndCityID(date, cityID, cancellationToken).ConfigureAwait(false)
            ?? throw new Exception($"Prayer times for the {date} could not be found for an unknown reason.");

        prayerTimes.NextFajr = (await getPrayerTimesByDateAndCityID(date.Plus(Duration.FromDays(1)), cityID, cancellationToken).ConfigureAwait(false))?.Fajr;

        return prayerTimes;
    }

    // locked per city (without the date) so that parallel calculations of multiple days
    // don't trigger redundant API fetches: the first one fills the db cache, the others then hit it
    private static readonly AsyncKeyedLocker<int> getPrayerTimesLocker = new(o =>
    {
        o.MaxCount = 1;
        o.PoolSize = 20;
        o.PoolInitialFill = 1;
    });

    private async Task<FaziletDailyPrayerTimes> getPrayerTimesByDateAndCityID(ZonedDateTime date, int cityID, CancellationToken cancellationToken)
    {
        using (await getPrayerTimesLocker.LockAsync(cityID, cancellationToken).ConfigureAwait(false))
        {
            FaziletDailyPrayerTimes prayerTimes = await faziletRepository.GetTimesByDateAndCityID(date, cityID, cancellationToken).ConfigureAwait(false);

            if (prayerTimes is null)
            {
                var prayerTimesResponseDTO = await faziletApiService.GetTimesByCityID(cityID, cancellationToken).ConfigureAwait(false);
                var timeZone = prayerTimesResponseDTO.Timezone;
                var prayerTimesLst = prayerTimesResponseDTO.PrayerTimes.Select(x => x.ToFaziletPrayerTimes(cityID, timeZone)).ToList();
                await faziletRepository.InsertPrayerTimesAsync(prayerTimesLst, cancellationToken).ConfigureAwait(false);

                prayerTimes = prayerTimesLst.FirstOrDefault(x => x.Date.Date == date.Date);
            }

            return prayerTimes;
        }
    }

    private static readonly AsyncNonKeyedLocker semaphoreTryGetCityID = new(1);

    protected override async Task<int> GetCityID(string cityName, int countryID, bool throwIfNotFound, CancellationToken cancellationToken)
    {
        // check-then-act has to be thread safe
        using (await semaphoreTryGetCityID.LockAsync(cancellationToken).ConfigureAwait(false))
        {
            int? cityID = await faziletRepository.GetCityIDByName(countryID, cityName, cancellationToken).ConfigureAwait(false);

            // city found
            if (cityID is not null)
                return cityID.Value;

            // unknown city
            if (await faziletRepository.HasCityData(countryID, cancellationToken).ConfigureAwait(false))
            {
                return throwIfNotFound
                    ? throw new ArgumentException($"{nameof(cityName)} could not be found!")
                    : -1;
            }

            // load cities through HTTP request and save them
            var cityDTOs = await faziletApiService.GetCitiesByCountryID(countryID, cancellationToken).ConfigureAwait(false);
            var cities = cityDTOs.Select(x => new FaziletCity { Name = x.Name, ID = x.ID, CountryID = countryID }).ToList();
            await faziletRepository.InsertCities(cities, cancellationToken).ConfigureAwait(false);

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

    protected override async Task<int> GetCountryID(string countryName, bool throwIfNotFound, CancellationToken cancellationToken)
    {
        // check-then-act has to be thread safe
        using (await semaphoreTryGetCountryID.LockAsync(cancellationToken).ConfigureAwait(false))
        {
            int? countryID = await faziletRepository.GetCountryIDByName(countryName, cancellationToken).ConfigureAwait(false);

            // country found
            if (countryID is not null)
                return countryID.Value;

            // unknown country
            if (await faziletRepository.HasCountryData(cancellationToken).ConfigureAwait(false))
            {
                return throwIfNotFound
                    ? throw new ArgumentException($"{nameof(countryName)} could not be found!")
                    : -1;
            }

            var countriesDTOs = (await faziletApiService.GetCountries(cancellationToken).ConfigureAwait(false)).Countries;
            var countries = countriesDTOs.Select(x => new FaziletCountry { Name = x.Name, ID = x.ID }).ToList();
            await faziletRepository.InsertCountries(countries, cancellationToken).ConfigureAwait(false);

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

    protected override BaseLocationData CreateLocationData(string countryName, string cityName, ProfilePlaceInfo place)
    {
        return new FaziletLocationData
        {
            CountryName = countryName,
            CityName = cityName
        };
    }

    protected override IEnumerable<string> GetCityNameVariants(string cityName)
    {
        return cityName switch
        {
            "Mekke" => ["Mekke-i Mükerreme"],
            "Medine" => ["Medîne-i Münevvere"],
            _ => base.GetCityNameVariants(cityName),
        };
    }
}
