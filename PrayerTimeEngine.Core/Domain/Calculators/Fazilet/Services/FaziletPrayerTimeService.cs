using Microsoft.Extensions.Logging;
using PrayerTimeEngine.Core.Common;
using PrayerTimeEngine.Core.Domain.Model;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Models;
using PrayerTimeEngine.Core.Domain.Configuration.Models;
using PrayerTimeEngine.Core.Domain.PlacesService.Models;
using PrayerTimeEngine.Core.Domain.CalculationService.Interfaces;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.PlacesService.Interfaces;

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

        public async Task<ILookup<ICalculationPrayerTimes, ETimeType>> GetPrayerTimesAsync(
            DateTime date,
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

            ICalculationPrayerTimes faziletPrayerTimes = await getPrayerTimesInternal(date, countryName, cityName);

            // this single calculation entity applies to all the TimeTypes of the configurations
            return configurations
                .Select(x => x.TimeType)
                .ToLookup(x => faziletPrayerTimes, y => y);
        }

        private async Task<FaziletPrayerTimes> getPrayerTimesInternal(DateTime date, string countryName, string cityName)
        {
            if (await tryGetCountryID(countryName) is (bool countrySuccess, int countryID) countryResult && !countrySuccess)
                throw new ArgumentException($"{nameof(countryName)} could not be found!");
            if (await tryGetCityID(cityName, countryID) is (bool citySuccess, int cityID) cityResult && !citySuccess)
                throw new ArgumentException($"{nameof(cityName)} could not be found!");

            FaziletPrayerTimes prayerTimes = await getPrayerTimesByDateAndCityID(date, cityID)
                ?? throw new Exception($"Prayer times for the {date:D} could not be found for an unknown reason.");

            prayerTimes.NextFajr = (await getPrayerTimesByDateAndCityID(date.AddDays(1), cityID))?.Fajr;

            return prayerTimes;
        }

        private readonly AsyncDuplicateLock getPrayerTimesLocker = new();

        private async Task<FaziletPrayerTimes> getPrayerTimesByDateAndCityID(DateTime date, int cityID)
        {
            var lockTuple = (date, cityID);

            using (await getPrayerTimesLocker.LockAsync(lockTuple))
            {
                FaziletPrayerTimes prayerTimes = await _faziletDBAccess.GetTimesByDateAndCityID(date, cityID);

                if (prayerTimes == null)
                {
                    List<FaziletPrayerTimes> prayerTimesLst = await _faziletApiService.GetTimesByCityID(cityID);
                    prayerTimesLst.ForEach(async x => await _faziletDBAccess.InsertFaziletPrayerTimes(x.Date.Date, cityID, x));
                    prayerTimes = prayerTimesLst.FirstOrDefault(x => x.Date == date.Date);
                }

                return prayerTimes;
            }
        }

        private readonly SemaphoreSlim semaphoreTryGetCityID = new(1, 1);

        private async Task<(bool success, int cityID)> tryGetCityID(string cityName, int countryID)
        {
            // check-then-act has to be thread safe
            await semaphoreTryGetCityID.WaitAsync();

            try
            {
                // We only check if it is empty because a selection of countries missing is not expected.
                if ((await _faziletDBAccess.GetCitiesByCountryID(countryID)).Count == 0)
                {
                    // load cities through HTTP request
                    Dictionary<string, int> cities = await _faziletApiService.GetCitiesByCountryID(countryID);

                    // save cities to db
                    await _faziletDBAccess.InsertCities(cities, countryID);
                }
            }
            finally
            {
                semaphoreTryGetCityID.Release();
            }

            if ((await _faziletDBAccess.GetCitiesByCountryID(countryID)).TryGetValue(cityName, out int cityID))
                return (true, cityID);
            else
                return (false, -1);
        }

        private SemaphoreSlim semaphoreTryGetCountryID = new SemaphoreSlim(1, 1);

        private async Task<(bool success, int countryID)> tryGetCountryID(string countryName)
        {
            // check-then-act has to be thread safe
            await semaphoreTryGetCountryID.WaitAsync();

            try
            {

                // We only check if it is empty because a selection of countries missing is not expected.
                if ((await _faziletDBAccess.GetCountries()).Count == 0)
                {
                    // load countries through HTTP request
                    Dictionary<string, int> countries = await _faziletApiService.GetCountries();

                    // save countries to db
                    await _faziletDBAccess.InsertCountries(countries);
                }
            }
            finally
            {
                semaphoreTryGetCountryID.Release();
            }

            if ((await _faziletDBAccess.GetCountries()).TryGetValue(countryName, out int countryID))
                return (true, countryID);
            else
                return (false, -1);
        }

        public async Task<BaseLocationData> GetLocationInfo(LocationIQPlace place)
        {
            if (place == null)
                throw new ArgumentNullException(nameof(place));

            // if language is already turkish then use this place

            LocationIQPlace turkishPlaceInfo = await _placeService.GetPlaceByID(place, "tr");
            string countryName = turkishPlaceInfo.address.country;
            string cityName = turkishPlaceInfo.address.city;

            // QUICK FIX...
            countryName = countryName.Replace("İ", "I");
            cityName = cityName.Replace("İ", "I");

            var (success, countryID) = await tryGetCountryID(countryName);

            _logger.LogDebug("Fazilet search location: {Country}, {City}", countryName, cityName);

            if (success && (await tryGetCityID(cityName, countryID)).success)
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
