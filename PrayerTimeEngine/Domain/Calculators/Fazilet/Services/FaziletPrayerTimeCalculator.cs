using PrayerTimeEngine.Common.Enum;
using PrayerTimeEngine.Domain.Calculators;
using PrayerTimeEngine.Domain.CalculationService.Interfaces;
using PrayerTimeEngine.Domain.ConfigStore.Models;
using PrayerTimeEngine.Domain.Calculators.Fazilet.Interfaces;
using PrayerTimeEngine.Domain.Calculators.Fazilet.Models;

namespace PrayerTimeEngine.Domain.Calculators.Fazilet.Services
{
    public class FaziletPrayerTimeCalculator : IPrayerTimeCalculator
    {
        private readonly IFaziletDBAccess _faziletDBAccess;
        private readonly IFaziletApiService _faziletApiService;

        public FaziletPrayerTimeCalculator(IFaziletDBAccess faziletDBAccess, IFaziletApiService faziletApiService)
        {
            _faziletDBAccess = faziletDBAccess;
            _faziletApiService = faziletApiService;
        }

        public HashSet<ETimeType> GetUnsupportedCalculationTimeTypes()
        {
            return _unsupportedCalculationTimeTypes;
        }

        private HashSet<ETimeType> _unsupportedCalculationTimeTypes =
            new HashSet<ETimeType>
            {
                ETimeType.FajrGhalas,
                ETimeType.FajrKaraha,
                ETimeType.DuhaEnd,
                ETimeType.AsrMithlayn,
                ETimeType.AsrKaraha,
                ETimeType.MaghribIshtibaq,
            };

        public async Task<ICalculationPrayerTimes> GetPrayerTimesAsync(
            DateTime date,
            ETimeType timeType,
            BaseCalculationConfiguration configuration)
        {
            // because currently there is no location selection
            string countryName = PrayerTimesConfigurationStorage.COUNTRY_NAME;
            string cityName = PrayerTimesConfigurationStorage.CITY_NAME;

            return await getPrayerTimesInternal(date, countryName, cityName);
        }

        private async Task<FaziletPrayerTimes> getPrayerTimesInternal(DateTime date, string countryName, string cityName)
        {
            int countryID = await getCountryID(countryName);
            int cityID = await getCityID(cityName, countryID);
            FaziletPrayerTimes prayerTimes = await getPrayerTimesByDateAndCityID(date, cityID)
                ?? throw new Exception($"Prayer times for the {date:D} could not be found for an unknown reason.");

            prayerTimes.NextFajr = (await getPrayerTimesByDateAndCityID(date.AddDays(1), cityID))?.Fajr;

            return prayerTimes;
        }

        private async Task<FaziletPrayerTimes> getPrayerTimesByDateAndCityID(DateTime date, int cityID)
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

        private async Task<int> getCityID(string cityName, int countryID)
        {
            // We only check if it is empty because a selection of countries missing is not expected.
            if ((await _faziletDBAccess.GetCitiesByCountryID(countryID)).Count == 0)
            {
                // load cities through HTTP request
                Dictionary<string, int> cities = await _faziletApiService.GetCitiesByCountryID(countryID);

                // save cities to db
                await _faziletDBAccess.InsertCities(cities, countryID);
            }
            if (!(await _faziletDBAccess.GetCitiesByCountryID(countryID)).TryGetValue(cityName, out int cityID))
                throw new ArgumentException($"{nameof(cityName)} could not be found!");
            return cityID;
        }

        private async Task<int> getCountryID(string countryName)
        {
            // We only check if it is empty because a selection of countries missing is not expected.
            if ((await _faziletDBAccess.GetCountries()).Count == 0)
            {
                // load countries through HTTP request
                Dictionary<string, int> countries = await _faziletApiService.GetCountries();

                // save countries to db
                await _faziletDBAccess.InsertCountries(countries);
            }
            if (!(await _faziletDBAccess.GetCountries()).TryGetValue(countryName, out int countryID))
                throw new ArgumentException($"{nameof(countryName)} could not be found!");
            return countryID;
        }
    }
}
