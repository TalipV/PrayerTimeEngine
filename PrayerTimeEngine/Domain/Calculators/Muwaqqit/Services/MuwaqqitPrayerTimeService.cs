using PrayerTimeEngine.Common.Enum;
using PrayerTimeEngine.Domain.CalculationService.Interfaces;
using PrayerTimeEngine.Domain.ConfigStore.Models;
using PrayerTimeEngine.Domain.Calculators.Muwaqqit.Interfaces;
using PrayerTimeEngine.Domain.Calculators.Muwaqqit.Models;
using PrayerTimeEngine.Domain.Calculators.Semerkand;
using PrayerTimeEngine.Domain.Model;
using PrayerTimeEngine.Domain.LocationService.Models;

namespace PrayerTimeEngine.Domain.Calculators.Muwaqqit.Services
{
    public class MuwaqqitPrayerTimeCalculator : IPrayerTimeService
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

        public async Task<ILookup<ICalculationPrayerTimes, ETimeType>> GetPrayerTimesAsync(
            DateTime date,
            BaseLocationData locationData,
            List<GenericSettingConfiguration> configurations)
        {
            // check configuration's calcultion sources?

            if (locationData is not MuwaqqitLocationData muwaqqitLocationData)
            {
                throw new Exception("Muwaqqit specific location information was not provided!");
            }

            // time zone has to be added to location data
            string timezone = PrayerTimesConfigurationStorage.TIMEZONE;

            decimal longitude = muwaqqitLocationData.Longitude;
            decimal latitude = muwaqqitLocationData.Latitude;

            List<ETimeType> toBeCalculatedTimeTypes = configurations.Select(x => x.TimeType).ToList();
            Dictionary<ICalculationPrayerTimes, List<ETimeType>> calculatedTimes = new();

            var toBeConsumedConfigurations = configurations.ToList();

            while (toBeConsumedConfigurations.Count != 0)
            {
                double fajrDegree, ishaDegree, ishtibaqDegree, asrKarahaDegree;
                List<ETimeType> consumedTimeTypes = 
                    ConsumeDegreeValues(
                        toBeConsumedConfigurations,
                        out fajrDegree,
                        out ishaDegree,
                        out ishtibaqDegree,
                        out asrKarahaDegree);

                MuwaqqitPrayerTimes muwaqqitPrayerTimes = await getPrayerTimesInternal(date, longitude, latitude, fajrDegree, ishaDegree, ishtibaqDegree, asrKarahaDegree, timezone);
                calculatedTimes[muwaqqitPrayerTimes] = consumedTimeTypes.ToList();
            }

            return calculatedTimes
                .SelectMany(kv => kv.Value.Select(t => new { kv.Key, Value = t }))
                .ToLookup(k => k.Key, k => k.Value);
        }

        private List<ETimeType> ConsumeDegreeValues(
            List<GenericSettingConfiguration> muwaqqitConfigs,
            out double fajrDegree,
            out double ishaDegree,
            out double ishtibaqDegree,
            out double asrKarahaDegree)
        {
            List<ETimeType> consumedTimeTypes = new();

            double? calculatedFajrDegree = null;
            double? calculatedIshaDegree = null;
            double? calculatedIshtibaqDegree = null;
            double? calculatedAsrKarahaDegree = null;

            foreach (GenericSettingConfiguration muwaqqitConfig in muwaqqitConfigs.ToList())
            {
                ETimeType timeType = muwaqqitConfig.TimeType;

                if (muwaqqitConfig is not MuwaqqitDegreeCalculationConfiguration muwaqqitDegreeConfig)
                {
                    if (_timeTypeAttributeService.DegreeTypes.Contains(timeType))
                    {
                        throw new ArgumentException($"Time {timeType} requires a {nameof(MuwaqqitDegreeCalculationConfiguration)} for its degree information.");
                    }

                    muwaqqitConfigs.Remove(muwaqqitConfig);
                    consumedTimeTypes.Add(timeType);
                    continue;
                }

                double degreeValue = muwaqqitDegreeConfig.Degree;

                switch (timeType)
                {
                    case ETimeType.IshaEnd:
                    case ETimeType.FajrStart:
                    case ETimeType.FajrGhalas:
                    case ETimeType.FajrKaraha:
                        if (calculatedFajrDegree == null)
                        {
                            calculatedFajrDegree = degreeValue;
                            muwaqqitConfigs.Remove(muwaqqitConfig); 
                            consumedTimeTypes.Add(timeType);
                        }
                        else if (calculatedFajrDegree == degreeValue)
                        {
                            muwaqqitConfigs.Remove(muwaqqitConfig); 
                            consumedTimeTypes.Add(timeType);
                        }
                        break;

                    case ETimeType.MaghribEnd:
                    case ETimeType.IshaStart:
                        if (calculatedIshaDegree == null)
                        {
                            calculatedIshaDegree = degreeValue;
                            muwaqqitConfigs.Remove(muwaqqitConfig); 
                            consumedTimeTypes.Add(timeType);
                        }
                        else if (calculatedIshaDegree == degreeValue)
                        {
                            muwaqqitConfigs.Remove(muwaqqitConfig); 
                            consumedTimeTypes.Add(timeType);
                        }
                        break;

                    case ETimeType.MaghribIshtibaq:
                        if (calculatedIshtibaqDegree == null)
                        {
                            calculatedIshtibaqDegree = degreeValue;
                            muwaqqitConfigs.Remove(muwaqqitConfig);
                            consumedTimeTypes.Add(timeType);
                        }
                        else if (calculatedIshtibaqDegree == degreeValue)
                        {
                            muwaqqitConfigs.Remove(muwaqqitConfig);
                            consumedTimeTypes.Add(timeType);
                        }
                        break;

                    case ETimeType.DuhaStart:
                    case ETimeType.AsrKaraha:
                        if (calculatedAsrKarahaDegree == null)
                        {
                            calculatedAsrKarahaDegree = degreeValue;
                            muwaqqitConfigs.Remove(muwaqqitConfig);
                            consumedTimeTypes.Add(timeType);
                        }
                        else if (calculatedAsrKarahaDegree == degreeValue)
                        {
                            muwaqqitConfigs.Remove(muwaqqitConfig);
                            consumedTimeTypes.Add(timeType);
                        }
                        break;
                }
            }

            fajrDegree = calculatedFajrDegree ?? - 12.0;
            ishaDegree = calculatedIshaDegree ?? -12.0;
            ishtibaqDegree = calculatedIshtibaqDegree ?? -12.0;
            asrKarahaDegree = calculatedAsrKarahaDegree ?? -12.0;

            return consumedTimeTypes;
        }

        public HashSet<ETimeType> GetUnsupportedTimeTypes()
        {
            return new HashSet<ETimeType>();
        }

        private SemaphoreSlim semaphoreGetPrayerTimesInternal = new SemaphoreSlim(1, 1);

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
            // check-then-act has to be thread safe
            await semaphoreGetPrayerTimesInternal.WaitAsync();

            try
            {
                MuwaqqitPrayerTimes prayerTimes = await _muwaqqitDBAccess.GetTimesAsync(date, longitude, latitude, fajrDegree, ishaDegree, ishtibaqDegree, asrKarahaDegree);

                if (prayerTimes == null)
                {
                    prayerTimes = await _muwaqqitApiService.GetTimesAsync(date, longitude, latitude, fajrDegree, ishaDegree, ishtibaqDegree, asrKarahaDegree, timezone);
                    await _muwaqqitDBAccess.InsertMuwaqqitPrayerTimesAsync(date, timezone, longitude, latitude, fajrDegree, ishaDegree, ishtibaqDegree, asrKarahaDegree, prayerTimes);
                }

                return prayerTimes;
            }
            finally
            {
                semaphoreGetPrayerTimesInternal.Release();
            }
        }

        public Task<BaseLocationData> GetLocationInfo(LocationIQPlace place)
        {
            if (place == null)
                throw new ArgumentNullException(nameof(place));

            return Task.FromResult<BaseLocationData>(new MuwaqqitLocationData
            {
                Longitude = decimal.Parse(place.lon, System.Globalization.CultureInfo.InvariantCulture),
                Latitude = decimal.Parse(place.lat, System.Globalization.CultureInfo.InvariantCulture)
            });
        }
    }
}
