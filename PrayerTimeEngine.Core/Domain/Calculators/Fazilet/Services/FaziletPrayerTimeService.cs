using Microsoft.Extensions.Logging;
using PrayerTimeEngine.Core.Common;
using PrayerTimeEngine.Core.Domain.Model;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Models;
using PrayerTimeEngine.Core.Domain.Configuration.Models;
using PrayerTimeEngine.Core.Domain.CalculationService.Interfaces;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.PlacesService.Interfaces;
using NodaTime;
using MethodTimer;
using PrayerTimeEngine.Core.Domain.PlacesService.Models.Common;

namespace PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Services
{
    public class FaziletPrayerTimeCalculator : IPrayerTimeService
    {
        private readonly IFaziletDBAccess _faziletDBAccess;
        private readonly IFaziletApiService _faziletApiService;
        private readonly ILocationService _placeService;
        private readonly ILogger<FaziletPrayerTimeCalculator> _logger;

        public FaziletPrayerTimeCalculator(
            IFaziletDBAccess faziletDBAccess,
            IFaziletApiService faziletApiService,
            ILocationService placeService,
            ILogger<FaziletPrayerTimeCalculator> logger)
        {
            _faziletDBAccess = faziletDBAccess;
            _faziletApiService = faziletApiService;
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

            if (locationData is not FaziletLocationData faziletLocationData)
            {
                throw new Exception("Fazilet specific location information was not provided!");
            }

            string countryName = faziletLocationData.CountryName;
            string cityName = faziletLocationData.CityName;

            ICalculationPrayerTimes faziletPrayerTimes = await getPrayerTimesInternal(date, countryName, cityName).ConfigureAwait(false);

            // this single calculation entity applies to all the TimeTypes of the configurations
            return configurations
                .Select(x => x.TimeType)
                .ToLookup(x => faziletPrayerTimes, y => y);
        }

        private async Task<FaziletPrayerTimes> getPrayerTimesInternal(LocalDate date, string countryName, string cityName)
        {
            if (await tryGetCountryID(countryName) is (bool countrySuccess, int countryID) countryResult && !countrySuccess)
                throw new ArgumentException($"{nameof(countryName)} could not be found!");
            if (await tryGetCityID(cityName, countryID) is (bool citySuccess, int cityID) cityResult && !citySuccess)
                throw new ArgumentException($"{nameof(cityName)} could not be found!");

            FaziletPrayerTimes prayerTimes = await getPrayerTimesByDateAndCityID(date, cityID).ConfigureAwait(false)
                ?? throw new Exception($"Prayer times for the {date:D} could not be found for an unknown reason.");

            prayerTimes.NextFajr = (await getPrayerTimesByDateAndCityID(date.PlusDays(1), cityID).ConfigureAwait(false))?.Fajr;

            return prayerTimes;
        }

        private readonly AsyncDuplicateLock getPrayerTimesLocker = new();

        private async Task<FaziletPrayerTimes> getPrayerTimesByDateAndCityID(LocalDate date, int cityID)
        {
            var lockTuple = (date, cityID);

            using (await getPrayerTimesLocker.LockAsync(lockTuple).ConfigureAwait(false))
            {
                FaziletPrayerTimes prayerTimes = await _faziletDBAccess.GetTimesByDateAndCityID(date, cityID).ConfigureAwait(false);

                if (prayerTimes == null)
                {
                    List<FaziletPrayerTimes> prayerTimesLst = await _faziletApiService.GetTimesByCityID(cityID).ConfigureAwait(false);
                    prayerTimesLst.ForEach(async x => await _faziletDBAccess.InsertFaziletPrayerTimesIfNotExists(x.Date, cityID, x).ConfigureAwait(false));
                    prayerTimes = prayerTimesLst.FirstOrDefault(x => x.Date == date);
                }

                return prayerTimes;
            }
        }

        private readonly SemaphoreSlim semaphoreTryGetCityID = new(1, 1);

        [Time]
        private async Task<(bool success, int cityID)> tryGetCityID(string cityName, int countryID)
        {
            // check-then-act has to be thread safe
            await semaphoreTryGetCityID.WaitAsync().ConfigureAwait(false);

            try
            {
                // We only check if it is empty because a selection of countries missing is not expected.
                if ((await _faziletDBAccess.GetCitiesByCountryID(countryID).ConfigureAwait(false)).Count == 0)
                {
                    // load cities through HTTP request
                    Dictionary<string, int> cities = await _faziletApiService.GetCitiesByCountryID(countryID).ConfigureAwait(false);

                    // save cities to db
                    await _faziletDBAccess.InsertCities(cities, countryID).ConfigureAwait(false);
                }
            }
            finally
            {
                semaphoreTryGetCityID.Release();
            }

            if ((await _faziletDBAccess.GetCitiesByCountryID(countryID).ConfigureAwait(false)).TryGetValue(cityName, out int cityID))
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
                if ((await _faziletDBAccess.GetCountries().ConfigureAwait(false)).Count == 0)
                {
                    // load countries through HTTP request
                    Dictionary<string, int> countries = await _faziletApiService.GetCountries().ConfigureAwait(false);

                    // save countries to db
                    await _faziletDBAccess.InsertCountries(countries).ConfigureAwait(false);
                }
            }
            finally
            {
                semaphoreTryGetCountryID.Release();
            }

            if ((await _faziletDBAccess.GetCountries().ConfigureAwait(false)).TryGetValue(countryName, out int countryID))
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

            var (success, countryID) = await tryGetCountryID(countryName).ConfigureAwait(false);

            _logger.LogDebug("Fazilet search location: {Country}, {City}", countryName, cityName);

            if (success && (await tryGetCityID(cityName, countryID).ConfigureAwait(false)).success)
            {
                _logger.LogDebug("Fazilet found location: {Country}, {City}", countryName, cityName);

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
