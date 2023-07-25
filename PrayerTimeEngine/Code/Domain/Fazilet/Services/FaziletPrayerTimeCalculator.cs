using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PrayerTimeEngine.Code.Domain.Fazilet.Interfaces;
using PrayerTimeEngine.Code.Domain.Fazilet.Models;
using PrayerTimeEngine.Code.Interfaces;
using PrayerTimeEngine.Code.Common.Extension;
using PrayerTimeEngine.Code.Domain.ConfigStore;
using PrayerTimeEngine.Code.Domain.ConfigStore.Models;
using PrayerTimeEngine.Code.Common.Enum;

namespace PrayerTimeEngine.Code.Domain.Fazilet.Services
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

        public async Task<DateTime> GetPrayerTimesAsync(
            DateTime date, 
            EPrayerTime prayerTime, EPrayerTimeEvent timeEvent, 
            BaseCalculationConfiguration configuration)
        {
            // because currently there is no location selection
            string countryName = PrayerTimesConfigurationStorage.COUNTRY_NAME;
            string cityName = PrayerTimesConfigurationStorage.CITY_NAME;

            FaziletPrayerTimes prayerTimes = await getPrayerTimesInternal(date, countryName, cityName);
            DateTime dateTime = getDateTimeFromFaziletPrayerTimes(prayerTime, timeEvent, prayerTimes);

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
        private DateTime getDateTimeFromFaziletPrayerTimes(EPrayerTime prayerTime, EPrayerTimeEvent timeEvent, FaziletPrayerTimes prayerTimes)
        {
            DateTime result;

            switch (prayerTime)
            {
                case EPrayerTime.Fajr:
                    result = timeEvent == EPrayerTimeEvent.Start ? prayerTimes.Fajr : prayerTimes.Shuruq;
                    break;
                case EPrayerTime.Duha:
                    result = timeEvent == EPrayerTimeEvent.Start ? prayerTimes.Shuruq : prayerTimes.Dhuhr;
                    break;
                case EPrayerTime.Dhuhr:
                    result = timeEvent == EPrayerTimeEvent.Start ? prayerTimes.Dhuhr : prayerTimes.Asr;
                    break;
                case EPrayerTime.Asr:
                    result = timeEvent == EPrayerTimeEvent.Start ? prayerTimes.Asr : prayerTimes.Maghrib;
                    break;
                case EPrayerTime.Maghrib:
                    result = timeEvent == EPrayerTimeEvent.Start ? prayerTimes.Maghrib : prayerTimes.Isha;
                    break;
                case EPrayerTime.Isha:
                    result = timeEvent == EPrayerTimeEvent.Start ? prayerTimes.Isha : prayerTimes.Isha.AddDays(1);
                    break;
                default:
                    throw new ArgumentException($"Invalid {nameof(prayerTime)} value: {prayerTime}.");
            }

            return result;
        }
    }
}
