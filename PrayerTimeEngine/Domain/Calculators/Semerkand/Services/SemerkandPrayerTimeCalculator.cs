using PrayerTimeEngine.Common.Enum;
using PrayerTimeEngine.Domain.CalculationService.Interfaces;
using PrayerTimeEngine.Domain.Calculators.Semerkand.Interfaces;
using PrayerTimeEngine.Domain.Calculators.Semerkand.Models;
using PrayerTimeEngine.Domain.ConfigStore.Models;

namespace PrayerTimeEngine.Domain.Calculators.Semerkand.Services
{
    public class SemerkandPrayerTimeCalculator : IPrayerTimeCalculator
    {
        private readonly ISemerkandDBAccess _semerkandDBAccess;
        private readonly ISemerkandApiService _semerkandApiService;

        public SemerkandPrayerTimeCalculator(ISemerkandDBAccess semerkandDBAccess, ISemerkandApiService semerkandApiService)
        {
            _semerkandDBAccess = semerkandDBAccess;
            _semerkandApiService = semerkandApiService;
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
            List<GenericSettingConfiguration> configurations)
        {
            // because currently there is no location selection
            string countryName = PrayerTimesConfigurationStorage.COUNTRY_NAME;
            string cityName = PrayerTimesConfigurationStorage.CITY_NAME;

            ICalculationPrayerTimes semerkandPrayerTimes = await getPrayerTimesInternal(date, countryName, cityName);

            // this single calculation entity applies to all the TimeTypes of the configurations
            return configurations
            .Select(x => x.TimeType)
                .ToLookup(x => semerkandPrayerTimes, y => y);
        }

        private async Task<SemerkandPrayerTimes> getPrayerTimesInternal(DateTime date, string countryName, string cityName)
        {
            int countryID = await getCountryID(countryName);
            int cityID = await getCityID(cityName, countryID);
            SemerkandPrayerTimes prayerTimes = await getPrayerTimesByDateAndCityID(date, cityID)
                ?? throw new Exception($"Prayer times for the {date:D} could not be found for an unknown reason.");

            prayerTimes.NextFajr = (await getPrayerTimesByDateAndCityID(date.AddDays(1), cityID))?.Fajr;

            return prayerTimes;
        }

        private async Task<SemerkandPrayerTimes> getPrayerTimesByDateAndCityID(DateTime date, int cityID)
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

        private async Task<int> getCityID(string cityName, int countryID)
        {
            // We only check if it is empty because a selection of countries missing is not expected.
            if ((await _semerkandDBAccess.GetCitiesByCountryID(countryID)).Count == 0)
            {
                // load cities through HTTP request
                Dictionary<string, int> cities = await _semerkandApiService.GetCitiesByCountryID(countryID);

                // save cities to db
                await _semerkandDBAccess.InsertCities(cities, countryID);
            }
            if (!(await _semerkandDBAccess.GetCitiesByCountryID(countryID)).TryGetValue(cityName, out int cityID))
                throw new ArgumentException($"{nameof(cityName)} could not be found!");
            return cityID;
        }

        private async Task<int> getCountryID(string countryName)
        {
            // We only check if it is empty because a selection of countries missing is not expected.
            if ((await _semerkandDBAccess.GetCountries()).Count == 0)
            {
                // load countries through HTTP request
                Dictionary<string, int> countries = await _semerkandApiService.GetCountries();

                // save countries to db
                await _semerkandDBAccess.InsertCountries(countries);
            }
            if (!(await _semerkandDBAccess.GetCountries()).TryGetValue(countryName, out int countryID))
                throw new ArgumentException($"{nameof(countryName)} could not be found!");
            return countryID;
        }
    }
}
