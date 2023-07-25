using PrayerTimeEngine.Code.Domain.Muwaqqit.Interfaces;
using PrayerTimeEngine.Code.Domain.Muwaqqit.Models;
using PrayerTimeEngine.Code.Interfaces;
using System.Collections.Specialized;
using System.Text.Json;
using System.Web;
using Microsoft.Maui.Controls;
using PrayerTimeEngine.Code.Common.Extension;
using PrayerTimeEngine.Code.Domain.Fazilet.Models;
using PrayerTimeEngine.Code.Domain.ConfigStore;
using PrayerTimeEngine.Code.Domain.Model;
using PrayerTimeEngine.Code.Domain.ConfigStore.Models;

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

        public async Task<DateTime> GetPrayerTimesAsync(
            DateTime date, 
            EPrayerTime prayerTime, 
            EPrayerTimeEvent timeEvent, 
            BaseCalculationConfiguration configuration)
        {
            if (configuration is not MuwaqqitCalculationConfiguration muwaqqitConfig)
                throw new ArgumentException($"{nameof(configuration)} is not an instance of {nameof(MuwaqqitCalculationConfiguration)}");

            // location selection has not been implemented yet
            string timezone = PrayerTimesConfigurationStorage.TIMEZONE;
            decimal longitude = PrayerTimesConfigurationStorage.INNSBRUCK_LONGITUDE;
            decimal latitude = PrayerTimesConfigurationStorage.INNSBRUCK_LATITUDE;

            double fajrDegree, ishaDegree, ishtibaqDegree, asrKarahaDegree;
            getDegreeValue(
                prayerTime, 
                timeEvent, 
                muwaqqitConfig, 
                out fajrDegree, 
                out ishaDegree,
                out ishtibaqDegree,
                out asrKarahaDegree);

            MuwaqqitPrayerTimes prayerTimes = await getPrayerTimesInternal(date, longitude, latitude, fajrDegree, ishaDegree, ishtibaqDegree, asrKarahaDegree, timezone);
            DateTime dateTime = getDateTimeFromMuwaqqitPrayerTimes(prayerTime, timeEvent, prayerTimes);

            return dateTime;
        }

        private static void getDegreeValue(
            EPrayerTime prayerTime, 
            EPrayerTimeEvent timeEvent,
            MuwaqqitCalculationConfiguration muwaqqitConfig, 
            out double fajrDegree, 
            out double ishaDegree, 
            out double ishtibaqDegree,
            out double asrKarahaDegree)
        {
            fajrDegree = -12.0;
            ishaDegree = -12.0;
            ishtibaqDegree = -12.0;
            asrKarahaDegree = -12.0;

            if (muwaqqitConfig is not MuwaqqitDegreeCalculationConfiguration muwaqqitDegreeConfig)
            {
                // should have been degree calculation
                if ((prayerTime == EPrayerTime.Fajr || prayerTime == EPrayerTime.Isha) && timeEvent == EPrayerTimeEvent.Start)
                {
                    throw new ArgumentException(
                        $"When {nameof(muwaqqitConfig)} is an instance of {nameof(MuwaqqitDegreeCalculationConfiguration)} it " +
                        $"has to be a calculation of {nameof(EPrayerTime.Fajr)} or {nameof(EPrayerTime.Isha)}-{EPrayerTimeEvent.Start}");
                }
                return;
            }

            if (prayerTime == EPrayerTime.Fajr && (timeEvent == EPrayerTimeEvent.Start || timeEvent == EPrayerTimeEvent.Fajr_Fadilah || timeEvent == EPrayerTimeEvent.Fajr_Karaha))
                fajrDegree = muwaqqitDegreeConfig.Degree;
            else if (prayerTime == EPrayerTime.Isha && timeEvent == EPrayerTimeEvent.Start)
                ishaDegree = muwaqqitDegreeConfig.Degree;
            else if (prayerTime == EPrayerTime.Isha && timeEvent == EPrayerTimeEvent.End)
                fajrDegree = muwaqqitDegreeConfig.Degree;
            else if (prayerTime == EPrayerTime.Maghrib && timeEvent == EPrayerTimeEvent.End)
                ishaDegree = muwaqqitDegreeConfig.Degree;
            else if (prayerTime == EPrayerTime.Maghrib && timeEvent == EPrayerTimeEvent.IshtibaqAnNujum)
                ishtibaqDegree = muwaqqitDegreeConfig.Degree;
            else if (prayerTime == EPrayerTime.Asr && timeEvent == EPrayerTimeEvent.Asr_Karaha)
                asrKarahaDegree = muwaqqitDegreeConfig.Degree;
            else if (prayerTime == EPrayerTime.Duha && timeEvent == EPrayerTimeEvent.Start)
                asrKarahaDegree = muwaqqitDegreeConfig.Degree;
            else
                throw new ArgumentException(
                    $"When {nameof(muwaqqitConfig)} is an instance of {nameof(MuwaqqitDegreeCalculationConfiguration)} it " +
                    $"has to be a calculation of a time with a degree.");
        }

        public List<(EPrayerTime PrayerTime, EPrayerTimeEvent PrayerTimeEvent)> GetUnsupportedPrayerTimeEvents()
        {
            return new List<(EPrayerTime PrayerTime, EPrayerTimeEvent PrayerTimeEvent)> 
            { 
            };
        }

        private async Task<MuwaqqitPrayerTimes> getPrayerTimesInternal(
            DateTime date, 
            decimal longitude, 
            decimal latitude, 
            double fajrDegree, 
            double ishaDegree, 
            double ishtibaqDegree,
            double asrKarahaDegree,
            string timezone)
        {
            MuwaqqitPrayerTimes prayerTimes = await _muwaqqitDBAccess.GetTimesAsync(date, longitude, latitude, fajrDegree, ishaDegree, ishtibaqDegree, asrKarahaDegree);

            if (prayerTimes == null)
            {
                prayerTimes = await _muwaqqitApiService.GetTimesAsync(date, longitude, latitude, fajrDegree, ishaDegree, ishtibaqDegree, asrKarahaDegree, timezone);
                await _muwaqqitDBAccess.InsertMuwaqqitPrayerTimesAsync(date, timezone, longitude, latitude, fajrDegree, ishaDegree, ishtibaqDegree, asrKarahaDegree, prayerTimes);
            }

            return prayerTimes;
        }

        // TODO: MASSIV HINTERFRAGEN (Generischer und Isha-Ende als Fajr-Beginn??)
        private DateTime getDateTimeFromMuwaqqitPrayerTimes(EPrayerTime prayerTime, EPrayerTimeEvent timeEvent, MuwaqqitPrayerTimes prayerTimes)
        {
            switch (prayerTime)
            {
                case EPrayerTime.Fajr:
                    if (timeEvent == EPrayerTimeEvent.Start || timeEvent == EPrayerTimeEvent.Fajr_Fadilah || timeEvent == EPrayerTimeEvent.Fajr_Karaha)
                    {
                        return prayerTimes.Fajr;
                    }
                    else if (timeEvent == EPrayerTimeEvent.End)
                    {
                        return prayerTimes.Shuruq;
                    }
                    break;
                case EPrayerTime.Duha:
                    if (timeEvent == EPrayerTimeEvent.Start)
                    {
                        return prayerTimes.Duha;
                    }
                    else if (timeEvent == EPrayerTimeEvent.End)
                    {
                        return prayerTimes.Dhuhr;
                    }
                    break;
                case EPrayerTime.Dhuhr:
                    if (timeEvent == EPrayerTimeEvent.Start)
                    {
                        return prayerTimes.Dhuhr;
                    }
                    else if (timeEvent == EPrayerTimeEvent.End)
                    {
                        return prayerTimes.AsrMithl;
                    }
                    break;
                case EPrayerTime.Asr:
                    if (timeEvent == EPrayerTimeEvent.Start)
                    {
                        return prayerTimes.AsrMithl;
                    }
                    else if (timeEvent == EPrayerTimeEvent.End)
                    {
                        return prayerTimes.Maghrib;
                    }
                    else if (timeEvent == EPrayerTimeEvent.AsrMithlayn)
                    {
                        return prayerTimes.AsrMithlayn;
                    }
                    else if (timeEvent == EPrayerTimeEvent.Asr_Karaha)
                    {
                        return prayerTimes.AsrKaraha;
                    }
                    break;
                case EPrayerTime.Maghrib:
                    if (timeEvent == EPrayerTimeEvent.Start)
                    {
                        return prayerTimes.Maghrib;
                    }
                    else if (timeEvent == EPrayerTimeEvent.End)
                    {
                        return prayerTimes.Isha;
                    }
                    else if (timeEvent == EPrayerTimeEvent.IshtibaqAnNujum)
                    {
                        return prayerTimes.Ishtibaq;
                    }
                    break;
                case EPrayerTime.Isha:
                    if (timeEvent == EPrayerTimeEvent.Start)
                    {
                        return prayerTimes.Isha;
                    }
                    else if (timeEvent == EPrayerTimeEvent.End)
                    {
                        return prayerTimes.NextFajr;
                    }
                    break;
                default:
                    break;
            }

            throw new ArgumentException($"Invalid {nameof(prayerTime)} value: {prayerTime}.");
        }
    }
}
