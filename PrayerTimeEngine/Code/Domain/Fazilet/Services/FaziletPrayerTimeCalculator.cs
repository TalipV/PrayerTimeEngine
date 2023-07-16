using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PrayerTimeEngine.Code.Domain.Fazilet.Interfaces;
using PrayerTimeEngine.Code.Domain.Fazilet.Models;
using PrayerTimeEngine.Code.DUMMYFOLDER;
using PrayerTimeEngine.Code.Interfaces;
using PrayerTimeEngine.Common.Enums;

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
                (EPrayerTime.Duha, EPrayerTimeEvent.Start),
                (EPrayerTime.Duha, EPrayerTimeEvent.End),
            };
        }

        public DateTime GetPrayerTimes(DateTime date, EPrayerTime prayerTime, EPrayerTimeEvent timeEvent, ICalculationConfiguration configuration)
        {
            if (configuration is not FaziletCalculationConfiguration faziletConfig)
                throw new ArgumentException($"{nameof(configuration)} is not an instance of {nameof(FaziletCalculationConfiguration)}");

            string countryName = faziletConfig.CountryName;
            string cityName = faziletConfig.CityName;

            FaziletPrayerTimes prayerTimes = getPrayerTimesInternal(date, countryName, cityName);
            return getDateTimeFromFaziletPrayerTimes(prayerTime, timeEvent, prayerTimes);
        }

        private FaziletPrayerTimes getPrayerTimesInternal(DateTime date, string countryName, string cityName)
        {
            int countryID = getCountryID(countryName);
            int cityID = getCityID(cityName, countryID);
            FaziletPrayerTimes prayerTimes = getPrayerTimesByDateAndCityID(date, cityID)
                ?? throw new Exception($"Prayer times for the {date:D} could not be found for an unknown reason.");

            return prayerTimes;
        }

        private FaziletPrayerTimes getPrayerTimesByDateAndCityID(DateTime date, int cityID)
        {
            FaziletPrayerTimes prayerTimes = _faziletDBAccess.GetTimesByDateAndCityID(date, cityID);

            if (prayerTimes == null)
            {
                List<FaziletPrayerTimes> prayerTimesLst = _faziletApiService.GetTimesByCityID(cityID);
                prayerTimesLst.ForEach(x => _faziletDBAccess.InsertFaziletPrayerTimes(x.Date.Date, cityID, x));
                prayerTimes = prayerTimesLst.FirstOrDefault(x => x.Date == date.Date);
            }

            return prayerTimes;
        }

        private int getCityID(string cityName, int countryID)
        {
            // We only check if it is empty because a selection of countries missing is not expected.
            if (_faziletDBAccess.GetCitiesByCountryID(countryID).Count == 0)
            {
                // load cities through HTTP request
                Dictionary<string, int> cities = _faziletApiService.GetCitiesByCountryID(countryID);

                // save cities to db
                _faziletDBAccess.InsertCities(cities, countryID);
            }
            if (!_faziletDBAccess.GetCitiesByCountryID(countryID).TryGetValue(cityName, out int cityID))
                throw new ArgumentException($"{nameof(cityName)} could not be found!");
            return cityID;
        }

        private int getCountryID(string countryName)
        {
            // We only check if it is empty because a selection of countries missing is not expected.
            if (_faziletDBAccess.GetCountries().Count == 0)
            {
                // load countries through HTTP request
                Dictionary<string, int> countries = _faziletApiService.GetCountries();

                // save countries to db
                _faziletDBAccess.InsertCountries(countries);
            }
            if (!_faziletDBAccess.GetCountries().TryGetValue(countryName, out int countryID))
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
