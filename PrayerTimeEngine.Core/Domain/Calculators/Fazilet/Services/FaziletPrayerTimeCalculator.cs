using Microsoft.Extensions.Logging;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Models;
using PrayerTimeEngine.Core.Common.Enum;
using NodaTime;
using AsyncKeyedLock;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Models.Entities;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Models;

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

            FaziletPrayerTimes faziletPrayerTimes = 
                await getPrayerTimesInternal(
                    date, 
                    countryName, 
                    cityName, cancellationToken).ConfigureAwait(false);

            return configurations
                .Select(x => (x.TimeType, faziletPrayerTimes.GetZonedDateTimeForTimeType(x.TimeType)))
                .ToList();
        }

        private async Task<FaziletPrayerTimes> getPrayerTimesInternal(LocalDate date, string countryName, string cityName, CancellationToken cancellationToken)
        {
            int countryID = await getCountryID(countryName, throwIfNotFound: true, cancellationToken).ConfigureAwait(false);
            int cityID = await getCityID(cityName, countryID, throwIfNotFound: true, cancellationToken).ConfigureAwait(false);

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
                    var prayerTimesResponseDTO = await faziletApiService.GetTimesByCityID(cityID, cancellationToken).ConfigureAwait(false);
                    var timeZone = prayerTimesResponseDTO.Timezone;
                    var prayerTimesLst = prayerTimesResponseDTO.PrayerTimes.Select(x => x.ToFaziletPrayerTimes(cityID, timeZone)).ToList();

                    foreach (var times in prayerTimesLst)
                    {
                        await faziletDBAccess.InsertFaziletPrayerTimes(
                            times.Date, cityID, times, 
                            cancellationToken).ConfigureAwait(false);
                    }

                    prayerTimes = prayerTimesLst.FirstOrDefault(x => x.Date == date);
                }

                return prayerTimes;
            }
        }

        private static readonly AsyncNonKeyedLocker semaphoreTryGetCityID = new(1);

        private async Task<int> getCityID(string cityName, int countryID, bool throwIfNotFound,  CancellationToken cancellationToken)
        {
            // check-then-act has to be thread safe
            using (await semaphoreTryGetCityID.LockAsync(cancellationToken).ConfigureAwait(false))
            {
                int? cityID = await faziletDBAccess.GetCityIDByName(countryID, cityName, cancellationToken).ConfigureAwait(false);

                // city found
                if (cityID != null)
                    return cityID.Value;

                // unknown city
                if (await faziletDBAccess.HasCityData(countryID, cancellationToken).ConfigureAwait(false))
                {
                    return throwIfNotFound
                        ? throw new ArgumentException($"{nameof(cityName)} could not be found!")
                        : -1;
                }

                // load cities through HTTP request and save them
                var cityDTOs = await faziletApiService.GetCitiesByCountryID(countryID, cancellationToken).ConfigureAwait(false);
                var cities = cityDTOs.Select(x => new FaziletCity { Name = x.Name, ID = x.ID, CountryID = countryID });
                await faziletDBAccess.InsertCities(cities, cancellationToken).ConfigureAwait(false);

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

        private async Task<int> getCountryID(string countryName, bool throwIfNotFound, CancellationToken cancellationToken)
        {
            // check-then-act has to be thread safe
            using (await semaphoreTryGetCountryID.LockAsync(cancellationToken).ConfigureAwait(false))
            {
                int? countryID = await faziletDBAccess.GetCountryIDByName(countryName, cancellationToken).ConfigureAwait(false);

                // country found
                if (countryID != null)
                    return countryID.Value;

                // unknown country
                if (await faziletDBAccess.HasCountryData(cancellationToken).ConfigureAwait(false))
                {
                    return throwIfNotFound 
                        ? throw new ArgumentException($"{nameof(countryName)} could not be found!") 
                        : - 1;
                }

                var countriesDTOs = (await faziletApiService.GetCountries(cancellationToken).ConfigureAwait(false)).Countries;
                var countries = countriesDTOs.Select(x => new FaziletCountry { Name = x.Name, ID = x.ID });

                await faziletDBAccess.InsertCountries(countries, cancellationToken).ConfigureAwait(false);

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

            logger.LogDebug("Fazilet search location: {Country}, {City}", countryName, cityName);

            int countryID = await getCountryID(countryName, throwIfNotFound: false, cancellationToken).ConfigureAwait(false);
            if (countryID != -1 
                && (await getCityID(cityName, countryID, throwIfNotFound: false, cancellationToken).ConfigureAwait(false)) != -1)
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
