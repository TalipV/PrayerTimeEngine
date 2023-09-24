﻿using MethodTimer;
using Microsoft.Extensions.Logging;
using NodaTime;
using PrayerTimeEngine.Core.Common;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.CalculationService.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Models;
using PrayerTimeEngine.Core.Domain.Configuration.Models;
using PrayerTimeEngine.Core.Domain.Model;
using PrayerTimeEngine.Core.Domain.PlacesService.Interfaces;
using PrayerTimeEngine.Core.Domain.PlacesService.Models.Common;

namespace PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Services
{
    public class SemerkandPrayerTimeCalculator : IPrayerTimeService
    {
        private readonly ISemerkandDBAccess _semerkandDBAccess;
        private readonly ISemerkandApiService _semerkandApiService;
        private readonly ILocationService _placeService;
        private readonly ILogger<SemerkandPrayerTimeCalculator> _logger;

        public SemerkandPrayerTimeCalculator(
            ISemerkandDBAccess semerkandDBAccess,
            ISemerkandApiService semerkandApiService,
            ILocationService placeService,
            ILogger<SemerkandPrayerTimeCalculator> logger)
        {
            _semerkandDBAccess = semerkandDBAccess;
            _semerkandApiService = semerkandApiService;
            _placeService = placeService;
            _logger = logger;
        }

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

        [Time]
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

            if (await tryGetCountryID(countryName).ConfigureAwait(false) is (bool countrySuccess, int countryID) countryResult && !countrySuccess)
                throw new ArgumentException($"{nameof(countryName)} could not be found!");
            if (await tryGetCityID(cityName, countryID).ConfigureAwait(false) is (bool citySuccess, int cityID) cityResult && !citySuccess)
                throw new ArgumentException($"{nameof(cityName)} could not be found!");

            SemerkandPrayerTimes prayerTimes = await getPrayerTimesByDateAndCityID(date, semerkandLocationData.TimezoneName, cityID).ConfigureAwait(false)
                ?? throw new Exception($"Prayer times for the {date:D} could not be found for an unknown reason.");

            prayerTimes.NextFajr = (await getPrayerTimesByDateAndCityID(date.PlusDays(1), semerkandLocationData.TimezoneName, cityID).ConfigureAwait(false))?.Fajr;

            return prayerTimes;
        }

        private readonly AsyncDuplicateLock getPrayerTimesLocker = new();

        [Time]
        private async Task<SemerkandPrayerTimes> getPrayerTimesByDateAndCityID(LocalDate date, string timezone, int cityID)
        {
            var lockTuple = (date, cityID);

            using (await getPrayerTimesLocker.LockAsync(lockTuple).ConfigureAwait(false))
            {
                SemerkandPrayerTimes prayerTimes = await _semerkandDBAccess.GetTimesByDateAndCityID(date, cityID).ConfigureAwait(false);

                if (prayerTimes == null)
                {
                    List<SemerkandPrayerTimes> prayerTimesLst = await _semerkandApiService.GetTimesByCityID(date, timezone, cityID).ConfigureAwait(false);
                    prayerTimesLst.ForEach(async x => await _semerkandDBAccess.InsertSemerkandPrayerTimes(x.Date, cityID, x).ConfigureAwait(false));
                    prayerTimes = prayerTimesLst.FirstOrDefault(x => x.Date == date);
                }

                return prayerTimes;
            }
        }


        private SemaphoreSlim semaphoreTryGetCityID = new SemaphoreSlim(1, 1);

        [Time]
        private async Task<(bool success, int cityID)> tryGetCityID(string cityName, int countryID)
        {
            // check-then-act has to be thread safe
            await semaphoreTryGetCityID.WaitAsync().ConfigureAwait(false);

            try
            {
                // We only check if it is empty because a selection of countries missing is not expected.
                if ((await _semerkandDBAccess.GetCitiesByCountryID(countryID).ConfigureAwait(false)).Count == 0)
                {
                    // load cities through HTTP request
                    Dictionary<string, int> cities = await _semerkandApiService.GetCitiesByCountryID(countryID).ConfigureAwait(false);

                    // save cities to db
                    await _semerkandDBAccess.InsertCities(cities, countryID).ConfigureAwait(false);
                }
            }
            finally
            {
                semaphoreTryGetCityID.Release();
            }

            if ((await _semerkandDBAccess.GetCitiesByCountryID(countryID).ConfigureAwait(false)).TryGetValue(cityName, out int cityID))
                return (true, cityID);
            else
                return (false, -1);
        }

        private SemaphoreSlim semaphoreTryGetCountryID = new SemaphoreSlim(1, 1);

        [Time]
        private async Task<(bool success, int countryID)> tryGetCountryID(string countryName)
        {
            // check-then-act has to be thread safe
            await semaphoreTryGetCountryID.WaitAsync().ConfigureAwait(false);

            try
            {
                // We only check if it is empty because a selection of countries missing is not expected.
                if ((await _semerkandDBAccess.GetCountries().ConfigureAwait(false)).Count == 0)
                {
                    // load countries through HTTP request
                    Dictionary<string, int> countries = await _semerkandApiService.GetCountries().ConfigureAwait(false);

                    // save countries to db
                    await _semerkandDBAccess.InsertCountries(countries).ConfigureAwait(false);
                }
            }
            finally
            {
                semaphoreTryGetCountryID.Release();
            }

            if ((await _semerkandDBAccess.GetCountries().ConfigureAwait(false)).TryGetValue(countryName, out int countryID))
                return (true, countryID);
            else
                return (false, -1);
        }

        public async Task<BaseLocationData> GetLocationInfo(CompletePlaceInfo place)
        {
            if (place == null)
                throw new ArgumentNullException(nameof(place));

            // if language is already turkish then use this place

            var turkishPlaceInfo =
                new CompletePlaceInfo(await _placeService.GetPlaceBasedOnPlace(place, "tr").ConfigureAwait(false))
                {
                    TimezoneInfo = place.TimezoneInfo
                };

            string countryName = turkishPlaceInfo.Country;
            string cityName = turkishPlaceInfo.City;

            // QUICK FIX...
            countryName = countryName.Replace("İ", "I");
            cityName = cityName.Replace("İ", "I");

            _logger.LogDebug("Semerkand search location: {Country}, {City}", countryName, cityName);

            var (success, countryID) = await tryGetCountryID(countryName).ConfigureAwait(false);

            if (success && (await tryGetCityID(cityName, countryID).ConfigureAwait(false)).success)
            {
                _logger.LogDebug("Semerkand found location: {Country}, {City}", countryName, cityName);

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
