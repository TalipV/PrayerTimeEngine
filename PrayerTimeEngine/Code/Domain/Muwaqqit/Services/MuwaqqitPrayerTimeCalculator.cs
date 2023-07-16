using PrayerTimeEngine.Code.Domain.Muwaqqit.Interfaces;
using PrayerTimeEngine.Code.Domain.Muwaqqit.Models;
using PrayerTimeEngine.Code.DUMMYFOLDER;
using PrayerTimeEngine.Code.Interfaces;
using PrayerTimeEngine.Common.Enums;
using System.Collections.Specialized;
using System.Text.Json;
using System.Web;

namespace PrayerTimeEngine.Code.Domain.Muwaqqit.Services
{
    public class MuwaqqitPrayerTimeCalculator : IPrayerTimeCalculator
    {
        private readonly IMuwaqqitDBAccess _muwaqqitDBAccess;
        private readonly IMuwaqqitApiService _muwaqqitApiService;

        public MuwaqqitPrayerTimeCalculator(IMuwaqqitDBAccess muwaqqitDBAccess, IMuwaqqitApiService muwaqqitApiService)
        {
            _muwaqqitDBAccess = muwaqqitDBAccess;
            _muwaqqitApiService = muwaqqitApiService;
        }

        public DateTime GetPrayerTimes(DateTime date, EPrayerTime prayerTime, EPrayerTimeEvent timeEvent, ICalculationConfiguration configuration)
        {
            string timezone = "Europe/Vienna";

            if (configuration is not MuwaqqitCalculationConfiguration muwaqqitConfig)
                throw new ArgumentException($"{nameof(configuration)} is not an instance of {nameof(MuwaqqitCalculationConfiguration)}");

            decimal longitude = muwaqqitConfig.Longitude;
            decimal latitude = muwaqqitConfig.Latitude;

            decimal fajrDegree = -12.0M;
            decimal ishaDegree = -12.0M;

            if (muwaqqitConfig is MuwaqqitCalculationConfigurationFajrIshaStart muwaqqitDegreeConfig)
            {
                if (prayerTime == EPrayerTime.Fajr)
                    fajrDegree = muwaqqitDegreeConfig.Degree;
                else if (prayerTime == EPrayerTime.Isha)
                    ishaDegree = muwaqqitDegreeConfig.Degree;
                else
                    throw new ArgumentException(
                        $"When {nameof(configuration)} is an instance of {nameof(MuwaqqitCalculationConfigurationFajrIshaStart)} it " +
                        $"has to be a calculation of {nameof(EPrayerTime.Fajr)} or {nameof(EPrayerTime.Isha)}-{EPrayerTimeEvent.Start}");
            }
            else if ((prayerTime == EPrayerTime.Fajr || prayerTime == EPrayerTime.Isha) && timeEvent == EPrayerTimeEvent.Start)
            {
                throw new ArgumentException(
                    $"When {nameof(configuration)} is an instance of {nameof(MuwaqqitCalculationConfigurationFajrIshaStart)} it " +
                    $"has to be a calculation of {nameof(EPrayerTime.Fajr)} or {nameof(EPrayerTime.Isha)}-{EPrayerTimeEvent.Start}");
            }

            MuwaqqitPrayerTimes prayerTimes = getPrayerTimesInternal(date, longitude, latitude, fajrDegree, ishaDegree, timezone);
            return getDateTimeFromMuwaqqitPrayerTimes(prayerTime, timeEvent, prayerTimes);
        }

        public List<(EPrayerTime PrayerTime, EPrayerTimeEvent PrayerTimeEvent)> GetUnsupportedPrayerTimeEvents()
        {
            return new List<(EPrayerTime PrayerTime, EPrayerTimeEvent PrayerTimeEvent)> 
            { 
                (EPrayerTime.Duha, EPrayerTimeEvent.Start),
                (EPrayerTime.Duha, EPrayerTimeEvent.End),
            };
        }

        private MuwaqqitPrayerTimes getPrayerTimesInternal(DateTime date, decimal longitude, decimal latitude, decimal fajrDegree, decimal ishaDegree, string timezone)
        {
            //TODO timezone berücksichtigen

            MuwaqqitPrayerTimes prayerTimes = _muwaqqitDBAccess.GetTimes(date, longitude, latitude, fajrDegree, ishaDegree);

            if (prayerTimes == null)
            {
                prayerTimes = _muwaqqitApiService.GetTimes(date, longitude, latitude, fajrDegree, ishaDegree, timezone);
                _muwaqqitDBAccess.InsertMuwaqqitPrayerTimes(date, longitude, latitude, fajrDegree, ishaDegree, prayerTimes);
            }

            return prayerTimes;
        }

        // TODO: MASSIV HINTERFRAGEN (Generischer und Isha-Ende als Fajr-Beginn??)
        private DateTime getDateTimeFromMuwaqqitPrayerTimes(EPrayerTime prayerTime, EPrayerTimeEvent timeEvent, MuwaqqitPrayerTimes prayerTimes)
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
                    result = timeEvent == EPrayerTimeEvent.Start ? prayerTimes.Dhuhr : prayerTimes.AsrMithl;
                    break;
                case EPrayerTime.Asr:
                    result = timeEvent == EPrayerTimeEvent.Start ? prayerTimes.AsrMithl : prayerTimes.Maghrib;
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
