using AsyncKeyedLock;
using Microsoft.Extensions.Logging;
using NodaTime;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Models;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Models.DTOs;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Models.Entities;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Models.Common;

namespace PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Services
{
    public class SemerkandPrayerTimeCalculator(
            ISemerkandDBAccess semerkandDBAccess,
            ISemerkandApiService semerkandApiService,
            IPlaceService placeService,
            ILogger<SemerkandPrayerTimeCalculator> logger
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

            if (locationData is not SemerkandLocationData semerkandLocationData)
            {
                throw new Exception("Semerkand specific location information was not provided!");
            }

            string countryName = semerkandLocationData.CountryName;
            string cityName = semerkandLocationData.CityName;
            string timezoneName = semerkandLocationData.TimezoneName;

            ICalculationPrayerTimes semerkandPrayerTimes = await getPrayerTimesInternal(date, countryName, cityName, timezoneName, cancellationToken).ConfigureAwait(false);
            
            return configurations
                .Select(x => (x.TimeType, semerkandPrayerTimes.GetZonedDateTimeForTimeType(x.TimeType)))
                .ToList();
        }

        private async Task<SemerkandPrayerTimes> getPrayerTimesInternal(LocalDate date, string countryName, string cityName, string timezoneName, CancellationToken cancellationToken)
        {
            if (await tryGetCountryID(countryName, cancellationToken).ConfigureAwait(false) is (bool countrySuccess, int countryID) && !countrySuccess)
                throw new ArgumentException($"{nameof(countryName)} could not be found!");
            if (await tryGetCityID(cityName, countryID, cancellationToken).ConfigureAwait(false) is (bool citySuccess, int cityID) && !citySuccess)
                throw new ArgumentException($"{nameof(cityName)} could not be found!");

            SemerkandPrayerTimes prayerTimes = await getPrayerTimesByDateAndCityID(date, timezoneName, cityID, cancellationToken).ConfigureAwait(false)
                ?? throw new Exception($"Prayer times for the {date:D} could not be found for an unknown reason.");

            prayerTimes.NextFajr = (await getPrayerTimesByDateAndCityID(date.PlusDays(1), timezoneName, cityID, cancellationToken).ConfigureAwait(false))?.Fajr;

            return prayerTimes;
        }

        private static readonly AsyncKeyedLocker<(LocalDate date, int cityID)> getPrayerTimesLocker = new(o =>
        {
            o.PoolSize = 20;
            o.PoolInitialFill = 1;
        });

        internal const int EXTENT_OF_DAYS_RETRIEVED = 5;

        private async Task<SemerkandPrayerTimes> getPrayerTimesByDateAndCityID(LocalDate date, string timezone, int cityID, CancellationToken cancellationToken)
        {
            var lockTuple = (date, cityID);

            using (await getPrayerTimesLocker.LockAsync(lockTuple).ConfigureAwait(false))
            {
                SemerkandPrayerTimes prayerTimes = await semerkandDBAccess.GetTimesByDateAndCityID(date, cityID, cancellationToken).ConfigureAwait(false);

                if (prayerTimes == null)
                {
                    DateTimeZone dateTimeZone = DateTimeZoneProviders.Tzdb[timezone];
                    List<SemerkandPrayerTimesResponseDTO> timesResponseDTOs = await semerkandApiService.GetTimesByCityID(date, cityID, cancellationToken).ConfigureAwait(false);

                    LocalDate firstDayOfYear = new LocalDate(date.Year, 1, 1);
                    
                    List<SemerkandPrayerTimes> prayerTimesLst = 
                        timesResponseDTOs
                            .Select(x => x.ToSemerkandPrayerTimes(cityID, dateTimeZone, firstDayOfYear))
                            .Where(x => date <= x.Date && x.Date < date.PlusDays(EXTENT_OF_DAYS_RETRIEVED))
                            .ToList();
                    
                    foreach (var times in prayerTimesLst)
                    {
                        await semerkandDBAccess.InsertSemerkandPrayerTimes(times.Date, cityID, times, cancellationToken).ConfigureAwait(false);
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
            using (await semaphoreTryGetCityID.LockAsync().ConfigureAwait(false))
            {
                int? cityID = await semerkandDBAccess.GetCityIDByName(countryID, cityName, cancellationToken).ConfigureAwait(false);

                // city found
                if (cityID != null)
                    return (true, cityID.Value);

                // unknown city
                if (await semerkandDBAccess.HasCityData(countryID, cancellationToken).ConfigureAwait(false))
                {
                    return (false, -1);
                }

                // load cities through HTTP request and save them
                var cityResponseDTOs = await semerkandApiService.GetCitiesByCountryID(countryID, cancellationToken).ConfigureAwait(false);
                Dictionary<string, int> cities = cityResponseDTOs.DistinctBy(x => x.Name).ToDictionary(x => x.Name, x => x.ID);
                await semerkandDBAccess.InsertCities(cities, countryID, cancellationToken).ConfigureAwait(false);

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
                int? countryID = await semerkandDBAccess.GetCountryIDByName(countryName, cancellationToken).ConfigureAwait(false);

                // country found
                if (countryID != null)
                    return (true, countryID.Value);

                // unknown country
                if (await semerkandDBAccess.HasCountryData(cancellationToken).ConfigureAwait(false))
                {
                    return (false, -1);
                }

                // load countries through HTTP request and save them
                var countryResponseDTOs = await semerkandApiService.GetCountries(cancellationToken).ConfigureAwait(false);
                Dictionary<string, int> countries = countryResponseDTOs.DistinctBy(x => x.Name).ToDictionary(x => x.Name, x => x.ID);
                await semerkandDBAccess.InsertCountries(countries, cancellationToken).ConfigureAwait(false);

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

            logger.LogDebug("Semerkand search location: {Country}, {City}", countryName, cityName);

            var (success, countryID) = await tryGetCountryID(countryName, cancellationToken).ConfigureAwait(false);

            if (success && (await tryGetCityID(cityName, countryID, cancellationToken).ConfigureAwait(false)).success)
            {
                logger.LogDebug("Semerkand found location: {Country}, {City}", countryName, cityName);

                return new SemerkandLocationData
                {
                    CountryName = countryName,
                    CityName = cityName,
                    TimezoneName = place.TimezoneInfo.Name
                };
            }

            return null;
        }
    }
}
