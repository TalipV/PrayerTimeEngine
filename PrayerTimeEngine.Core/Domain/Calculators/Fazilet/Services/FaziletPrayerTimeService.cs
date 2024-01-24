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
        ) : IPrayerTimeService
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
            if (await tryGetCountryID(countryName) is (bool countrySuccess, int countryID) && !countrySuccess)
                throw new ArgumentException($"{nameof(countryName)} could not be found!");
            if (await tryGetCityID(cityName, countryID) is (bool citySuccess, int cityID) && !citySuccess)
                throw new ArgumentException($"{nameof(cityName)} could not be found!");

            FaziletPrayerTimes prayerTimes = await getPrayerTimesByDateAndCityID(date, cityID).ConfigureAwait(false)
                ?? throw new Exception($"Prayer times for the {date:D} could not be found for an unknown reason.");

            prayerTimes.NextFajr = (await getPrayerTimesByDateAndCityID(date.PlusDays(1), cityID).ConfigureAwait(false))?.Fajr;

            return prayerTimes;
        }

        private readonly AsyncKeyedLocker<(LocalDate date, int cityID)> getPrayerTimesLocker = new(o =>
        {
            o.PoolSize = 20;
            o.PoolInitialFill = 1;
        });

        private async Task<FaziletPrayerTimes> getPrayerTimesByDateAndCityID(LocalDate date, int cityID)
        {
            var lockTuple = (date, cityID);

            using (await getPrayerTimesLocker.LockAsync(lockTuple).ConfigureAwait(false))
            {
                FaziletPrayerTimes prayerTimes = await faziletDBAccess.GetTimesByDateAndCityID(date, cityID).ConfigureAwait(false);

                if (prayerTimes == null)
                {
                    List<FaziletPrayerTimes> prayerTimesLst = await faziletApiService.GetTimesByCityID(cityID).ConfigureAwait(false);
                    prayerTimesLst.ForEach(async x => await faziletDBAccess.InsertFaziletPrayerTimesIfNotExists(x.Date, cityID, x).ConfigureAwait(false));
                    prayerTimes = prayerTimesLst.FirstOrDefault(x => x.Date == date);
                }

                return prayerTimes;
            }
        }

        private readonly AsyncNonKeyedLocker semaphoreTryGetCityID = new(1);

        private async Task<(bool success, int cityID)> tryGetCityID(string cityName, int countryID)
        {
            // check-then-act has to be thread safe
            using (await semaphoreTryGetCityID.LockAsync().ConfigureAwait(false))
            {
                // We only check if it is empty because a selection of countries missing is not expected.
                if ((await faziletDBAccess.GetCitiesByCountryID(countryID).ConfigureAwait(false)).Count == 0)
                {
                    // load cities through HTTP request
                    Dictionary<string, int> cities = await faziletApiService.GetCitiesByCountryID(countryID).ConfigureAwait(false);

                    // save cities to db
                    await faziletDBAccess.InsertCities(cities, countryID).ConfigureAwait(false);
                }
            }

            if ((await faziletDBAccess.GetCitiesByCountryID(countryID).ConfigureAwait(false)).FirstOrDefault(x => x.Name == cityName)?.ID is int cityID)
                return (true, cityID);
            else
                return (false, -1);
        }

        private readonly AsyncNonKeyedLocker semaphoreTryGetCountryID = new(1);

        private async Task<(bool success, int countryID)> tryGetCountryID(string countryName)
        {
            // check-then-act has to be thread safe
            using (await semaphoreTryGetCountryID.LockAsync().ConfigureAwait(false))
            {
                // We only check if it is empty because a selection of countries missing is not expected.
                if ((await faziletDBAccess.GetCountries().ConfigureAwait(false)).Count == 0)
                {
                    // load countries through HTTP request
                    Dictionary<string, int> countries = await faziletApiService.GetCountries().ConfigureAwait(false);

                    // save countries to db
                    await faziletDBAccess.InsertCountries(countries).ConfigureAwait(false);
                }
            }

            if ((await faziletDBAccess.GetCountries().ConfigureAwait(false)).FirstOrDefault(x => x.Name == countryName)?.ID is int countryID)
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
                new CompletePlaceInfo(await placeService.GetPlaceBasedOnPlace(place, "tr").ConfigureAwait(false)) 
                { 
                    TimezoneInfo = place.TimezoneInfo
                };
            
            string countryName = turkishPlaceInfo.Country;
            string cityName = turkishPlaceInfo.City;

            // QUICK FIX...
            countryName = countryName.Replace("İ", "I");
            cityName = cityName.Replace("İ", "I");

            var (success, countryID) = await tryGetCountryID(countryName).ConfigureAwait(false);

            logger.LogDebug("Fazilet search location: {Country}, {City}", countryName, cityName);

            if (success && (await tryGetCityID(cityName, countryID).ConfigureAwait(false)).success)
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
