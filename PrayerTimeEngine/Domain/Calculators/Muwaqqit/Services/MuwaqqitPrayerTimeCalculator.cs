using PrayerTimeEngine.Common.Enum;
using PrayerTimeEngine.Domain.CalculationService.Interfaces;
using PrayerTimeEngine.Domain.ConfigStore.Models;
using PrayerTimeEngine.Domain.Calculators.Muwaqqit.Interfaces;
using PrayerTimeEngine.Domain.Calculators.Muwaqqit.Models;

namespace PrayerTimeEngine.Domain.Calculators.Muwaqqit.Services
{
    public class MuwaqqitPrayerTimeCalculator : IPrayerTimeCalculator
    {
        private readonly IMuwaqqitDBAccess _muwaqqitDBAccess;
        private readonly IMuwaqqitApiService _muwaqqitApiService;
        private readonly TimeTypeAttributeService _timeTypeAttributeService;

        public MuwaqqitPrayerTimeCalculator(
            IMuwaqqitDBAccess muwaqqitDBAccess,
            IMuwaqqitApiService muwaqqitApiService,
            TimeTypeAttributeService timeTypeAttributeService)
        {
            _muwaqqitDBAccess = muwaqqitDBAccess;
            _muwaqqitApiService = muwaqqitApiService;
            _timeTypeAttributeService = timeTypeAttributeService;
        }

        public async Task<ICalculationPrayerTimes> GetPrayerTimesAsync(
            DateTime date,
            ETimeType timeType,
            BaseCalculationConfiguration configuration)
        {
            // location selection has not been implemented yet
            string timezone = PrayerTimesConfigurationStorage.TIMEZONE;
            decimal longitude = PrayerTimesConfigurationStorage.INNSBRUCK_LONGITUDE;
            decimal latitude = PrayerTimesConfigurationStorage.INNSBRUCK_LATITUDE;

            double fajrDegree, ishaDegree, ishtibaqDegree, asrKarahaDegree;
            getDegreeValue(
                timeType,
                configuration,
                out fajrDegree,
                out ishaDegree,
                out ishtibaqDegree,
                out asrKarahaDegree);

            return await getPrayerTimesInternal(date, longitude, latitude, fajrDegree, ishaDegree, ishtibaqDegree, asrKarahaDegree, timezone);
        }

        private void getDegreeValue(
            ETimeType timeType,
            BaseCalculationConfiguration muwaqqitConfig,
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
                if (_timeTypeAttributeService.DegreeTypes.Contains(timeType))
                {
                    throw new ArgumentException($"Time {timeType} requires a {nameof(MuwaqqitDegreeCalculationConfiguration)} for its degree information.");
                }

                return;
            }

            if (timeType == ETimeType.FajrStart || timeType == ETimeType.FajrGhalas || timeType == ETimeType.FajrKaraha)
                fajrDegree = muwaqqitDegreeConfig.Degree;
            else if (timeType == ETimeType.IshaStart)
                ishaDegree = muwaqqitDegreeConfig.Degree;
            else if (timeType == ETimeType.IshaEnd)
                fajrDegree = muwaqqitDegreeConfig.Degree;
            else if (timeType == ETimeType.MaghribEnd)
                ishaDegree = muwaqqitDegreeConfig.Degree;
            else if (timeType == ETimeType.MaghribIshtibaq)
                ishtibaqDegree = muwaqqitDegreeConfig.Degree;
            else if (timeType == ETimeType.AsrKaraha)
                asrKarahaDegree = muwaqqitDegreeConfig.Degree;
            else if (timeType == ETimeType.DuhaStart)
                asrKarahaDegree = muwaqqitDegreeConfig.Degree;
            else
                throw new ArgumentException(
                    $"When {nameof(muwaqqitConfig)} is an instance of {nameof(MuwaqqitDegreeCalculationConfiguration)} it " +
                    $"has to be a calculation of a time with a degree.");
        }

        public HashSet<ETimeType> GetUnsupportedCalculationTimeTypes()
        {
            return new HashSet<ETimeType>();
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
    }
}
