using PrayerTimeEngine.Code.Domain.ConfigStore.Models;
using PrayerTimeEngine.Code.Common.Enum;
using PrayerTimeEngine.Code.Domain.Calculator.Fazilet.Interfaces;
using PrayerTimeEngine.Code.Domain.Calculator.Fazilet.Models;
using PrayerTimeEngine.Code.Domain.Calculators;

namespace PrayerTimeEngine.Code.Domain.Calculator.Fazilet.Services
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
            return new HashSet<ETimeType>
            {
                ETimeType.FajrGhalas,
                ETimeType.FajrKaraha,
                ETimeType.DuhaEnd,
                ETimeType.AsrMithlayn,
                ETimeType.AsrKaraha,
                ETimeType.MaghribIshtibaq,
            };
        }

        public async Task<DateTime> GetPrayerTimesAsync(
            DateTime date,
            ETimeType timeType,
            BaseCalculationConfiguration configuration)
        {
            // because currently there is no location selection
            string countryName = PrayerTimesConfigurationStorage.COUNTRY_NAME;
            string cityName = PrayerTimesConfigurationStorage.CITY_NAME;

            FaziletPrayerTimes prayerTimes = await getPrayerTimesInternal(date, countryName, cityName);
            DateTime dateTime = getDateTimeFromFaziletPrayerTimes(timeType, prayerTimes);

            return dateTime;
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

        // TODO: MASSIV HINTERFRAGEN (Generischer und Isha-Ende als Fajr-Beginn??)
        private DateTime getDateTimeFromFaziletPrayerTimes(ETimeType timeType, FaziletPrayerTimes prayerTimes)
        {
            DateTime result;

            switch (timeType)
            {
                case ETimeType.FajrStart:
                    result = prayerTimes.Fajr;
                    break;
                case ETimeType.FajrEnd:
                    result = prayerTimes.Shuruq;
                    break;
                case ETimeType.DuhaStart:
                    result = prayerTimes.Shuruq;
                    break;
                case ETimeType.DhuhrStart:
                    result = prayerTimes.Dhuhr;
                    break;
                case ETimeType.DhuhrEnd:
                    result = prayerTimes.Asr;
                    break;
                case ETimeType.AsrStart:
                    result = prayerTimes.Asr;
                    break;
                case ETimeType.AsrEnd:
                    result = prayerTimes.Maghrib;
                    break;
                case ETimeType.MaghribStart:
                    result = prayerTimes.Maghrib;
                    break;
                case ETimeType.MaghribEnd:
                    result = prayerTimes.Isha;
                    break;
                case ETimeType.IshaStart:
                    result = prayerTimes.Isha;
                    break;
                case ETimeType.IshaEnd:
                    result = prayerTimes.NextFajr ?? prayerTimes.Isha;
                    break;
                default:
                    throw new ArgumentException($"Invalid {nameof(timeType)} value: {timeType}.");
            }

            return result;
        }
    }
}
