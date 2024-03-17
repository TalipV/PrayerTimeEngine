using Microsoft.Extensions.Logging;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Models;
using PrayerTimeEngine.Core.Common.Enum;
using NodaTime;
using AsyncKeyedLock;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Models.Common;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Interfaces;

namespace PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Services
{
    public class FaziletPrayerTimeCalculator(
            IFaziletDBAccess faziletDBAccess,
            IFaziletApiService faziletApiService,
            IPlaceService placeService,
            ILogger<FaziletPrayerTimeCalculator> logger
        ) : IPrayerTimeCalculator
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
                ETimeType.DuhaEnd,
                ETimeType.AsrMithlayn,
                ETimeType.AsrKaraha,
                ETimeType.MaghribIshtibaq,
            ];

        public async Task<List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)>> GetPrayerTimesAsync(
            LocalDate date,
            BaseLocationData locationData,
            List<GenericSettingConfiguration> configurations, 
            CancellationToken cancellationToken)
        {
            // check configuration's calcultion sources?

            if (locationData is not FaziletLocationData faziletLocationData)
            {
                throw new Exception("Fazilet specific location information was not provided!");
            }

            string countryName = faziletLocationData.CountryName;
            string cityName = faziletLocationData.CityName;

            ICalculationPrayerTimes faziletPrayerTimes = await getPrayerTimesInternal(date, countryName, cityName, cancellationToken).ConfigureAwait(false);

            return configurations
                .Select(x => (x.TimeType, faziletPrayerTimes.GetZonedDateTimeForTimeType(x.TimeType)))
                .ToList();
        }

        private async Task<FaziletPrayerTimes> getPrayerTimesInternal(LocalDate date, string countryName, string cityName, CancellationToken cancellationToken)
        {
            if (await tryGetCountryID(countryName, cancellationToken).ConfigureAwait(false) is (bool countrySuccess, int countryID) && !countrySuccess)
                throw new ArgumentException($"{nameof(countryName)} could not be found!");
            if (await tryGetCityID(cityName, countryID, cancellationToken).ConfigureAwait(false) is (bool citySuccess, int cityID) && !citySuccess)
                throw new ArgumentException($"{nameof(cityName)} could not be found!");

            FaziletPrayerTimes prayerTimes = await getPrayerTimesByDateAndCityID(date, cityID, cancellationToken).ConfigureAwait(false)
                ?? throw new Exception($"Prayer times for the {date:D} could not be found for an unknown reason.");

            prayerTimes.NextFajr = (await getPrayerTimesByDateAndCityID(date.PlusDays(1), cityID, cancellationToken).ConfigureAwait(false))?.Fajr;

            return prayerTimes;
        }

        private static readonly AsyncKeyedLocker<(LocalDate date, int cityID)> getPrayerTimesLocker = new(o =>
        {
            o.MaxCount = 1;
            o.PoolSize = 20;
            o.PoolInitialFill = 1;
        });

        private async Task<FaziletPrayerTimes> getPrayerTimesByDateAndCityID(LocalDate date, int cityID, CancellationToken cancellationToken)
        {
            var lockTuple = (date, cityID);

            using (await getPrayerTimesLocker.LockAsync(lockTuple).ConfigureAwait(false))
            {
                FaziletPrayerTimes prayerTimes = await faziletDBAccess.GetTimesByDateAndCityID(date, cityID, cancellationToken).ConfigureAwait(false);

                if (prayerTimes == null)
                {
                    List<FaziletPrayerTimes> prayerTimesLst = await faziletApiService.GetTimesByCityID(cityID, cancellationToken).ConfigureAwait(false);

                    foreach (var times in prayerTimesLst)
                    {
                        await faziletDBAccess.InsertFaziletPrayerTimesIfNotExists(times.Date, cityID, times, cancellationToken).ConfigureAwait(false);
                    }

                    prayerTimes = prayerTimesLst.FirstOrDefault(x => x.Date == date);
                }

                return prayerTimes;
            }
        }

        private static readonly AsyncNonKeyedLocker semaphoreTryGetCityID = new(1);

        private async Task<(bool success, int cityID)> tryGetCityID(string cityName, int countryID, CancellationToken cancellationToken)
        {
            // check-then-act has to be thread safe
            using (await semaphoreTryGetCityID.LockAsync(cancellationToken).ConfigureAwait(false))
            {
                int? cityID = await faziletDBAccess.GetCityIDByName(countryID, cityName, cancellationToken).ConfigureAwait(false);

                // city found
                if (cityID != null)
                    return (true, cityID.Value);

                // unknown city
                if (await faziletDBAccess.HasCityData(countryID, cancellationToken).ConfigureAwait(false))
                {
                    return (false, -1);
                }

                // load cities through HTTP request and save them
                Dictionary<string, int> cities = await faziletApiService.GetCitiesByCountryID(countryID, cancellationToken).ConfigureAwait(false);
                await faziletDBAccess.InsertCities(cities, countryID, cancellationToken).ConfigureAwait(false);

                if (cities.TryGetValue(cityName, out int returnValue))
                {
                    return (true, returnValue);
                }

                // there were no cities and loaded cities still didn't contain it
                return (false, -1);
            }
        }

        private static readonly AsyncNonKeyedLocker semaphoreTryGetCountryID = new(1);

        private async Task<(bool success, int countryID)> tryGetCountryID(string countryName, CancellationToken cancellationToken)
        {
            // check-then-act has to be thread safe
            using (await semaphoreTryGetCountryID.LockAsync(cancellationToken).ConfigureAwait(false))
            {
                int? countryID = await faziletDBAccess.GetCountryIDByName(countryName, cancellationToken).ConfigureAwait(false);

                // country found
                if (countryID != null)
                    return (true, countryID.Value);

                // unknown country
                if (await faziletDBAccess.HasCountryData(cancellationToken).ConfigureAwait(false))
                {
                    return (false, -1);
                }

                // load countries through HTTP request and save them
                Dictionary<string, int> countries = await faziletApiService.GetCountries(cancellationToken).ConfigureAwait(false);
                await faziletDBAccess.InsertCountries(countries, cancellationToken).ConfigureAwait(false);

                if (countries.TryGetValue(countryName, out int returnValue))
                {
                    return (true, returnValue);
                }

                // there were no countries and loaded countries still didn't contain it
                return (false, -1);
            }
        }

        public async Task<BaseLocationData> GetLocationInfo(CompletePlaceInfo place, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(place);

            // if language is already turkish then use this place

            var turkishPlaceInfo =
                new CompletePlaceInfo(await placeService.GetPlaceBasedOnPlace(place, "tr", cancellationToken).ConfigureAwait(false))
                {
                    TimezoneInfo = place.TimezoneInfo
                };

            string countryName = turkishPlaceInfo.Country;
            string cityName = turkishPlaceInfo.City;

            // QUICK FIX...
            countryName = countryName.Replace("İ", "I");
            cityName = cityName.Replace("İ", "I");

            var (success, countryID) = await tryGetCountryID(countryName, cancellationToken).ConfigureAwait(false);

            logger.LogDebug("Fazilet search location: {Country}, {City}", countryName, cityName);

            if (success && (await tryGetCityID(cityName, countryID, cancellationToken).ConfigureAwait(false)).success)
            {
                logger.LogDebug("Fazilet found location: {Country}, {City}", countryName, cityName);

                return new FaziletLocationData
                {
                    CountryName = countryName,
                    CityName = cityName
                };
            }

            return null;
        }
    }
}
