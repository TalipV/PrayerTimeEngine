using AsyncKeyedLock;
using Microsoft.Extensions.Logging;
using NodaTime;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Interfaces;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Models;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Models.DTOs;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Models.Entities;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Models;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Services;

public class SemerkandDynamicPrayerTimeProvider(
        ISemerkandDBAccess semerkandDBAccess,
        ISemerkandApiService semerkandApiService,
        IPlaceService placeService,
        ILogger<SemerkandDynamicPrayerTimeProvider> logger
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

        if (locationData is not SemerkandLocationData semerkandLocationData)
        {
            throw new Exception("Semerkand specific location information was not provided!");
        }

        string countryName = semerkandLocationData.CountryName;
        string cityName = semerkandLocationData.CityName;
        string timezoneName = semerkandLocationData.TimezoneName;

        if (timezoneName != date.Zone.Id)
        {
            throw new Exception("Time requested for timezone that differs from provided location data!");
        }

        SemerkandDailyPrayerTimes semerkandPrayerTimes = await getPrayerTimesInternal(date, countryName, cityName, timezoneName, cancellationToken).ConfigureAwait(false);

        return configurations
            .Select(x => (x.TimeType, ZonedDateTime: semerkandPrayerTimes.GetZonedDateTimeForTimeType(x.TimeType)))
            .Where(x => x.ZonedDateTime is not null)    // missing times are simply not returned
            .Select(x => (x.TimeType, x.ZonedDateTime.Value))
            .ToList();
    }

    private async Task<SemerkandDailyPrayerTimes> getPrayerTimesInternal(ZonedDateTime date, string countryName, string cityName, string timezoneName, CancellationToken cancellationToken)
    {
        int countryID = await GetCountryID(countryName, throwIfNotFound: true, cancellationToken).ConfigureAwait(false);
        int cityID = await GetCityID(cityName, countryID, throwIfNotFound: true, cancellationToken).ConfigureAwait(false);

        SemerkandDailyPrayerTimes prayerTimes =
            await getPrayerTimesByDateAndCityID(
                date,
                timezoneName,
                cityID,
                cancellationToken).ConfigureAwait(false)
            ?? throw new Exception($"Prayer times for the {date} could not be found for an unknown reason.");

        prayerTimes.NextFajr = (await getPrayerTimesByDateAndCityID(date.Plus(Duration.FromDays(1)), timezoneName, cityID, cancellationToken).ConfigureAwait(false))?.Fajr;

        return prayerTimes;
    }

    private static readonly AsyncKeyedLocker<(ZonedDateTime date, int cityID)> getPrayerTimesLocker = new(o =>
    {
        o.PoolSize = 20;
        o.PoolInitialFill = 1;
    });

    internal const int MAX_EXTENT_OF_RETRIEVED_DAYS = 5;

    private async Task<SemerkandDailyPrayerTimes> getPrayerTimesByDateAndCityID(ZonedDateTime date, string timezone, int cityID, CancellationToken cancellationToken)
    {
        var lockTuple = (date, cityID);

        using (await getPrayerTimesLocker.LockAsync(lockTuple, cancellationToken).ConfigureAwait(false))
        {
            SemerkandDailyPrayerTimes prayerTimes =
                await semerkandDBAccess.GetTimesByDateAndCityID(
                    date,
                    cityID,
                    cancellationToken).ConfigureAwait(false);

            if (prayerTimes is null)
            {
                List<SemerkandPrayerTimesResponseDTO> timesResponseDTOs =
                    await semerkandApiService.GetTimesByCityID(
                        date.Year,
                        cityID,
                        cancellationToken).ConfigureAwait(false);

                var dateTimeZone = DateTimeZoneProviders.Tzdb[timezone];
                var firstDayOfYear = new LocalDate(date.Year, 1, 1);

                List<SemerkandDailyPrayerTimes> prayerTimesLst =
                    timesResponseDTOs
                        .Select(x => x.ToSemerkandPrayerTimes(cityID, dateTimeZone, firstDayOfYear))
                        .Where(x => date.Date <= x.Date.Date && x.Date.Date < date.Plus(Duration.FromDays(MAX_EXTENT_OF_RETRIEVED_DAYS)).Date)
                        .ToList();

                await semerkandDBAccess.InsertPrayerTimesAsync(prayerTimesLst, cancellationToken).ConfigureAwait(false);

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
            int? cityID = await semerkandDBAccess.GetCityIDByName(countryID, cityName, cancellationToken).ConfigureAwait(false);

            // city found
            if (cityID is not null)
                return cityID.Value;

            // unknown city
            if (await semerkandDBAccess.HasCityData(countryID, cancellationToken).ConfigureAwait(false))
            {
                return throwIfNotFound
                    ? throw new ArgumentException($"{nameof(cityName)} could not be found!")
                    : -1;
            }

            // load cities through HTTP request and save them
            var cityResponseDTOs = await semerkandApiService.GetCitiesByCountryID(countryID, cancellationToken).ConfigureAwait(false);
            var cities = cityResponseDTOs.Select(x => new SemerkandCity { ID = x.ID, Name = x.Name, CountryID = countryID });
            await semerkandDBAccess.InsertCities(cities, cancellationToken).ConfigureAwait(false);

            return cityResponseDTOs.FirstOrDefault(x => x.Name == cityName)?.ID
                ?? (throwIfNotFound
                ? throw new ArgumentException($"{nameof(cityName)} could not be found!")
                : -1);
        }
    }

    private static readonly AsyncNonKeyedLocker semaphoreTryGetCountryID = new(1);

    protected override async Task<int> GetCountryID(string countryName, bool throwIfNotFound, CancellationToken cancellationToken)
    {
        // check-then-act has to be thread safe
        using (await semaphoreTryGetCountryID.LockAsync(cancellationToken).ConfigureAwait(false))
        {
            int? countryID = await semerkandDBAccess.GetCountryIDByName(countryName, cancellationToken).ConfigureAwait(false);

            // country found
            if (countryID is not null)
                return countryID.Value;

            // unknown country
            if (await semerkandDBAccess.HasCountryData(cancellationToken).ConfigureAwait(false))
            {
                return throwIfNotFound
                    ? throw new ArgumentException($"{nameof(countryName)} could not be found!")
                    : -1;
            }

            // load countries through HTTP request and save them
            var countryResponseDTOs = await semerkandApiService.GetCountries(cancellationToken).ConfigureAwait(false);
            var countries = countryResponseDTOs.Select(x => new SemerkandCountry { ID = x.ID, Name = x.Name });
            await semerkandDBAccess.InsertCountries(countries, cancellationToken).ConfigureAwait(false);

            return countryResponseDTOs.FirstOrDefault(x => x.Name == countryName)?.ID
                ?? (throwIfNotFound
                ? throw new ArgumentException($"{nameof(countryName)} could not be found!")
                : -1);
        }
    }

    protected override BaseLocationData CreateLocationData(string countryName, string cityName, ProfilePlaceInfo place)
    {
        return new SemerkandLocationData
        {
            CountryName = countryName,
            CityName = cityName,
            TimezoneName = place.TimezoneInfo.Name,
        };
    }
}
