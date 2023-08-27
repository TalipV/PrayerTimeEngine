using Microsoft.Extensions.Logging;
using PrayerTimeEngine.Common.Enum;
using PrayerTimeEngine.Core.Common;
using PrayerTimeEngine.Domain.CalculationService.Interfaces;
using PrayerTimeEngine.Domain.Calculators.Semerkand.Interfaces;
using PrayerTimeEngine.Domain.Calculators.Semerkand.Models;
using PrayerTimeEngine.Domain.ConfigStore.Models;
using PrayerTimeEngine.Domain.LocationService.Models;
using PrayerTimeEngine.Domain.Model;
using PrayerTimeEngine.Domain.NominatimLocation.Interfaces;

namespace PrayerTimeEngine.Domain.Calculators.Semerkand.Services
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

        public async Task<ILookup<ICalculationPrayerTimes, ETimeType>> GetPrayerTimesAsync(
            DateTime date,
            BaseLocationData locationData,
            List<GenericSettingConfiguration> configurations)
        {
            // check configuration's calcultion sources?

            if (locationData is not SemerkandLocationData semerkandLocationData)
            {
                throw new Exception("Semerkand specific location information was not provided!");
            }

            string countryName = semerkandLocationData.CountryName;
            string cityName = semerkandLocationData.CityName;

            ICalculationPrayerTimes semerkandPrayerTimes = await getPrayerTimesInternal(date, countryName, cityName);

            // this single calculation entity applies to all the TimeTypes of the configurations
            return configurations
            .Select(x => x.TimeType)
                .ToLookup(x => semerkandPrayerTimes, y => y);
        }

        private async Task<SemerkandPrayerTimes> getPrayerTimesInternal(DateTime date, string countryName, string cityName)
        {
            if((await tryGetCountryID(countryName)) is (bool countrySuccess, int countryID) countryResult && !countrySuccess)
                throw new ArgumentException($"{nameof(countryName)} could not be found!");
            if ((await tryGetCityID(cityName, countryID)) is (bool citySuccess, int cityID) cityResult && !citySuccess)
                throw new ArgumentException($"{nameof(cityName)} could not be found!");

            SemerkandPrayerTimes prayerTimes = await getPrayerTimesByDateAndCityID(date, cityID)
                ?? throw new Exception($"Prayer times for the {date:D} could not be found for an unknown reason.");

            prayerTimes.NextFajr = (await getPrayerTimesByDateAndCityID(date.AddDays(1), cityID))?.Fajr;

            return prayerTimes;
        }

        private readonly AsyncDuplicateLock getPrayerTimesLocker = new();

        private async Task<SemerkandPrayerTimes> getPrayerTimesByDateAndCityID(DateTime date, int cityID)
        {
            var lockTuple = (date, cityID);

            using (await getPrayerTimesLocker.LockAsync(lockTuple))
            {
                SemerkandPrayerTimes prayerTimes = await _semerkandDBAccess.GetTimesByDateAndCityID(date, cityID);

                if (prayerTimes == null)
                {
                    List<SemerkandPrayerTimes> prayerTimesLst = await _semerkandApiService.GetTimesByCityID(date, cityID);
                    prayerTimesLst.ForEach(async x => await _semerkandDBAccess.InsertSemerkandPrayerTimes(x.Date.Date, cityID, x));
                    prayerTimes = prayerTimesLst.FirstOrDefault(x => x.Date == date.Date);
                }

                return prayerTimes;
            }
        }


        private SemaphoreSlim semaphoreTryGetCityID = new SemaphoreSlim(1, 1);

        private async Task<(bool success, int cityID)> tryGetCityID(string cityName, int countryID)
        {
            // check-then-act has to be thread safe
            await semaphoreTryGetCityID.WaitAsync();

            try
            {
                // We only check if it is empty because a selection of countries missing is not expected.
                if ((await _semerkandDBAccess.GetCitiesByCountryID(countryID)).Count == 0)
                {
                    // load cities through HTTP request
                    Dictionary<string, int> cities = await _semerkandApiService.GetCitiesByCountryID(countryID);

                    // save cities to db
                    await _semerkandDBAccess.InsertCities(cities, countryID);
                }
            }
            finally
            {
                semaphoreTryGetCityID.Release();
            }

            if ((await _semerkandDBAccess.GetCitiesByCountryID(countryID)).TryGetValue(cityName, out int cityID))
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
                if ((await _semerkandDBAccess.GetCountries()).Count == 0)
                {
                    // load countries through HTTP request
                    Dictionary<string, int> countries = await _semerkandApiService.GetCountries();

                    // save countries to db
                    await _semerkandDBAccess.InsertCountries(countries);
                }
            }
            finally
            {
                semaphoreTryGetCountryID.Release();
            }

            if ((await _semerkandDBAccess.GetCountries()).TryGetValue(countryName, out int countryID))
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

            _logger.LogDebug("Semerkand search location: {Country}, {City}", countryName, cityName);

            var (success, countryID) = await this.tryGetCountryID(countryName);

            if (success && (await this.tryGetCityID(cityName, countryID)).success)
            {
                _logger.LogDebug("Semerkand found location: {Country}, {City}", countryName, cityName);

                return new SemerkandLocationData
                {
                    CountryName = countryName,
                    CityName = cityName
                };
            }

            return null;
        }
    }
}
