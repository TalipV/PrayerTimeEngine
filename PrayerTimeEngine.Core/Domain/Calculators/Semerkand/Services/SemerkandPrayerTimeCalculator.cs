﻿using AsyncKeyedLock;
using Microsoft.Extensions.Logging;
using NodaTime;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Models;
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

        private HashSet<ETimeType> _unsupportedTimeTypes =
            new HashSet<ETimeType>
            {
                ETimeType.FajrGhalas,
                ETimeType.FajrKaraha,
                ETimeType.DuhaEnd,
                ETimeType.AsrMithlayn,
                ETimeType.AsrKaraha,
                ETimeType.MaghribIshtibaq,
            };

        public async Task<ILookup<ICalculationPrayerTimes, ETimeType>> GetPrayerTimesAsync(
            LocalDate date,
            BaseLocationData locationData,
            List<GenericSettingConfiguration> configurations)
        {
            // check configuration's calcultion sources?

            if (locationData is not SemerkandLocationData semerkandLocationData)
            {
                throw new Exception("Semerkand specific location information was not provided!");
            }

            ICalculationPrayerTimes semerkandPrayerTimes = await getPrayerTimesInternal(date, semerkandLocationData).ConfigureAwait(false);

            // this single calculation entity applies to all the TimeTypes of the configurations
            return configurations
            .Select(x => x.TimeType)
                .ToLookup(x => semerkandPrayerTimes, y => y);
        }

        private async Task<SemerkandPrayerTimes> getPrayerTimesInternal(LocalDate date, SemerkandLocationData semerkandLocationData)
        {
            string countryName = semerkandLocationData.CountryName;
            string cityName = semerkandLocationData.CityName;
            if (await tryGetCountryID(countryName).ConfigureAwait(false) is (bool countrySuccess, int countryID) && !countrySuccess)
                throw new ArgumentException($"{nameof(countryName)} could not be found!");
            if (await tryGetCityID(cityName, countryID).ConfigureAwait(false) is (bool citySuccess, int cityID) && !citySuccess)
                throw new ArgumentException($"{nameof(cityName)} could not be found!");

            SemerkandPrayerTimes prayerTimes = await getPrayerTimesByDateAndCityID(date, semerkandLocationData.TimezoneName, cityID).ConfigureAwait(false)
                ?? throw new Exception($"Prayer times for the {date:D} could not be found for an unknown reason.");

            prayerTimes.NextFajr = (await getPrayerTimesByDateAndCityID(date.PlusDays(1), semerkandLocationData.TimezoneName, cityID).ConfigureAwait(false))?.Fajr;

            return prayerTimes;
        }

        private static readonly AsyncKeyedLocker<(LocalDate date, int cityID)> getPrayerTimesLocker = new(o =>
        {
            o.PoolSize = 20;
            o.PoolInitialFill = 1;
        });

        private async Task<SemerkandPrayerTimes> getPrayerTimesByDateAndCityID(LocalDate date, string timezone, int cityID)
        {
            var lockTuple = (date, cityID);

            using (await getPrayerTimesLocker.LockAsync(lockTuple).ConfigureAwait(false))
            {
                SemerkandPrayerTimes prayerTimes = await semerkandDBAccess.GetTimesByDateAndCityID(date, cityID).ConfigureAwait(false);

                if (prayerTimes == null)
                {
                    List<SemerkandPrayerTimes> prayerTimesLst = await semerkandApiService.GetTimesByCityID(date, timezone, cityID).ConfigureAwait(false);
                    prayerTimesLst.ForEach(async x => await semerkandDBAccess.InsertSemerkandPrayerTimes(x.Date, cityID, x).ConfigureAwait(false));
                    prayerTimes = prayerTimesLst.FirstOrDefault(x => x.Date == date);
                }

                return prayerTimes;
            }
        }

        private static readonly AsyncNonKeyedLocker semaphoreTryGetCityID = new(1);

        private async Task<(bool success, int cityID)> tryGetCityID(string cityName, int countryID)
        {
            // check-then-act has to be thread safe
            using (await semaphoreTryGetCityID.LockAsync().ConfigureAwait(false))
            {
                int? cityID = await semerkandDBAccess.GetCityIDByName(countryID, cityName);

                // city found
                if (cityID != null)
                    return (true, cityID.Value);

                // unknown city
                if (await semerkandDBAccess.HasCityData(countryID).ConfigureAwait(false))
                {
                    return (false, -1);
                }

                // load cities through HTTP request and save them
                Dictionary<string, int> cities = await semerkandApiService.GetCitiesByCountryID(countryID).ConfigureAwait(false);
                await semerkandDBAccess.InsertCities(cities, countryID).ConfigureAwait(false);

                if (cities.TryGetValue(cityName, out int returnValue))
                {
                    return (true, returnValue);
                }

                // there were no cities and loaded cities still didn't contain it
                return (false, -1);
            }
        }

        private static readonly AsyncNonKeyedLocker semaphoreTryGetCountryID = new(1);

        private async Task<(bool success, int countryID)> tryGetCountryID(string countryName)
        {
            // check-then-act has to be thread safe
            using (await semaphoreTryGetCountryID.LockAsync().ConfigureAwait(false))
            {
                int? countryID = await semerkandDBAccess.GetCountryIDByName(countryName);

                // country found
                if (countryID != null)
                    return (true, countryID.Value);

                // unknown country
                if (await semerkandDBAccess.HasCountryData().ConfigureAwait(false))
                {
                    return (false, -1);
                }

                // load countries through HTTP request and save them
                Dictionary<string, int> countries = await semerkandApiService.GetCountries().ConfigureAwait(false);
                await semerkandDBAccess.InsertCountries(countries).ConfigureAwait(false);

                if (countries.TryGetValue(countryName, out int returnValue))
                {
                    return (true, returnValue);
                }

                // there were no countries and loaded countries still didn't contain it
                return (false, -1);
            }
        }

        public async Task<BaseLocationData> GetLocationInfo(CompletePlaceInfo place)
        {
            if (place == null)
                throw new ArgumentNullException(nameof(place));

            // if language is already turkish then use this place

            var turkishPlaceInfo =
                new CompletePlaceInfo(await placeService.GetPlaceBasedOnPlace(place, "tr").ConfigureAwait(false))
                {
                    TimezoneInfo = place.TimezoneInfo
                };

            string countryName = turkishPlaceInfo.Country;
            string cityName = turkishPlaceInfo.City;

            // QUICK FIX...
            countryName = countryName.Replace("İ", "I");
            cityName = cityName.Replace("İ", "I");

            logger.LogDebug("Semerkand search location: {Country}, {City}", countryName, cityName);

            var (success, countryID) = await tryGetCountryID(countryName).ConfigureAwait(false);

            if (success && (await tryGetCityID(cityName, countryID).ConfigureAwait(false)).success)
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
