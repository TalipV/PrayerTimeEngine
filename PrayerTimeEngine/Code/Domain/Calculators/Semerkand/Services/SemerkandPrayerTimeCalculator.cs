using PrayerTimeEngine.Code.Common.Enum;
using PrayerTimeEngine.Code.Domain.Calculator.Semerkand.Models;
using PrayerTimeEngine.Code.Domain.Calculators.Semerkand.Interfaces;
using PrayerTimeEngine.Code.Domain.ConfigStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrayerTimeEngine.Code.Domain.Calculators.Semerkand.Services
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

        public async Task<DateTime> GetPrayerTimesAsync(
            DateTime date,
            EPrayerTime prayerTime, EPrayerTimeEvent timeEvent,
            BaseCalculationConfiguration configuration)
        {
            // because currently there is no location selection
            string countryName = PrayerTimesConfigurationStorage.COUNTRY_NAME;
            string cityName = PrayerTimesConfigurationStorage.CITY_NAME;

            SemerkandPrayerTimes prayerTimes = await getPrayerTimesInternal(date, countryName, cityName);
            DateTime dateTime = getDateTimeFromSemerkandPrayerTimes(prayerTime, timeEvent, prayerTimes);

            return dateTime;
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
                List<SemerkandPrayerTimes> prayerTimesLst = await _semerkandApiService.GetTimesByCityID(date,cityID);
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

        // TODO: MASSIV HINTERFRAGEN (Generischer und Isha-Ende als Fajr-Beginn??)
        private DateTime getDateTimeFromSemerkandPrayerTimes(EPrayerTime prayerTime, EPrayerTimeEvent timeEvent, SemerkandPrayerTimes prayerTimes)
        {
            DateTime result;

            switch (prayerTime)
            {
                case EPrayerTime.Fajr:
                    result = timeEvent == EPrayerTimeEvent.Start ? prayerTimes.Fajr : prayerTimes.Tulu;
                    break;
                case EPrayerTime.Duha:
                    result = timeEvent == EPrayerTimeEvent.Start ? prayerTimes.Tulu : prayerTimes.Zuhr;
                    break;
                case EPrayerTime.Dhuhr:
                    result = timeEvent == EPrayerTimeEvent.Start ? prayerTimes.Zuhr : prayerTimes.Asr;
                    break;
                case EPrayerTime.Asr:
                    result = timeEvent == EPrayerTimeEvent.Start ? prayerTimes.Asr : prayerTimes.Maghrib;
                    break;
                case EPrayerTime.Maghrib:
                    result = timeEvent == EPrayerTimeEvent.Start ? prayerTimes.Maghrib : prayerTimes.Isha;
                    break;
                case EPrayerTime.Isha:
                    result = timeEvent == EPrayerTimeEvent.Start ? prayerTimes.Isha : (prayerTimes.NextFajr ?? prayerTimes.Isha);
                    break;
                default:
                    throw new ArgumentException($"Invalid {nameof(prayerTime)} value: {prayerTime}.");
            }

            return result;
        }

        public List<(EPrayerTime PrayerTime, EPrayerTimeEvent PrayerTimeEvent)> GetUnsupportedPrayerTimeEvents()
        {
            return new List<(EPrayerTime PrayerTime, EPrayerTimeEvent PrayerTimeEvent)>
            {
                (EPrayerTime.Fajr, EPrayerTimeEvent.Fajr_Fadilah),
                (EPrayerTime.Fajr, EPrayerTimeEvent.Fajr_Karaha),
                (EPrayerTime.Duha, EPrayerTimeEvent.Start),
                (EPrayerTime.Duha, EPrayerTimeEvent.End),
                (EPrayerTime.Asr, EPrayerTimeEvent.AsrMithlayn),
                (EPrayerTime.Asr, EPrayerTimeEvent.Asr_Karaha),
                (EPrayerTime.Maghrib, EPrayerTimeEvent.IshtibaqAnNujum),
            };
        }
    }
}
