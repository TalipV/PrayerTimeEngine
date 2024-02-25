﻿using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Models;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Interfaces;
using PrayerTimeEngine.Core.Common.Enum;
using NodaTime;
using AsyncKeyedLock;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Models.Common;

namespace PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Services
{
    public class MuwaqqitPrayerTimeCalculator(
            IMuwaqqitDBAccess muwaqqitDBAccess,
            IMuwaqqitApiService muwaqqitApiService,
            TimeTypeAttributeService timeTypeAttributeService
        ) : IPrayerTimeService
    {
        public async Task<ILookup<ICalculationPrayerTimes, ETimeType>> GetPrayerTimesAsync(
            LocalDate date,
            BaseLocationData locationData,
            List<GenericSettingConfiguration> configurations)
        {
            // check configuration's calcultion sources?

            if (locationData is not MuwaqqitLocationData muwaqqitLocationData)
            {
                throw new Exception("Muwaqqit specific location information was not provided!");
            }

            // time zone has to be added to location data
            string timezone = muwaqqitLocationData.TimezoneName;

            decimal longitude = muwaqqitLocationData.Longitude;
            decimal latitude = muwaqqitLocationData.Latitude;

            List<ETimeType> toBeCalculatedTimeTypes = configurations.Select(x => x.TimeType).ToList();
            Dictionary<ICalculationPrayerTimes, List<ETimeType>> calculatedTimes = new();

            var toBeConsumedConfigurations = configurations.ToList();

            while (toBeConsumedConfigurations.Count != 0)
            {
                double fajrDegree, ishaDegree, ishtibaqDegree, asrKarahaDegree;
                List<ETimeType> consumedTimeTypes =
                    consumeDegreeValues(
                        toBeConsumedConfigurations,
                        out fajrDegree,
                        out ishaDegree,
                        out ishtibaqDegree,
                        out asrKarahaDegree);

                MuwaqqitPrayerTimes muwaqqitPrayerTimes = await getPrayerTimesInternal(date, longitude, latitude, fajrDegree, ishaDegree, ishtibaqDegree, asrKarahaDegree, timezone).ConfigureAwait(false);
                calculatedTimes[muwaqqitPrayerTimes] = consumedTimeTypes;
            }

            return calculatedTimes
                .SelectMany(kv => kv.Value.Select(t => new { kv.Key, Value = t }))
                .ToLookup(k => k.Key, k => k.Value);
        }

        private List<ETimeType> consumeDegreeValues(
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
                    if (timeTypeAttributeService.DegreeTypes.Contains(timeType))
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

            fajrDegree = calculatedFajrDegree ?? -12.0;
            ishaDegree = calculatedIshaDegree ?? -12.0;
            ishtibaqDegree = calculatedIshtibaqDegree ?? -12.0;
            asrKarahaDegree = calculatedAsrKarahaDegree ?? -12.0;

            return consumedTimeTypes;
        }

        public HashSet<ETimeType> GetUnsupportedTimeTypes()
        {
            return new HashSet<ETimeType>();
        }

        private static readonly AsyncKeyedLocker<(LocalDate date, decimal longitude, decimal latitude, double fajrDegree, double ishaDegree, double ishtibaqDegree, double asrKarahaDegree, string timezone)> getPrayerTimesLocker = new(o =>
        {
            o.PoolSize = 20;
            o.PoolInitialFill = 1;
        });

        private async Task<MuwaqqitPrayerTimes> getPrayerTimesInternal(
            LocalDate date,
            decimal longitude,
            decimal latitude,
            double fajrDegree,
            double ishaDegree,
            double ishtibaqDegree,
            double asrKarahaDegree,
            string timezone)
        {
            var lockTuple = (date, longitude, latitude, fajrDegree, ishaDegree, ishtibaqDegree, asrKarahaDegree, timezone);

            using (await getPrayerTimesLocker.LockAsync(lockTuple).ConfigureAwait(false))
            {
                MuwaqqitPrayerTimes prayerTimes = await muwaqqitDBAccess.GetTimesAsync(date, longitude, latitude, fajrDegree, ishaDegree, ishtibaqDegree, asrKarahaDegree).ConfigureAwait(false);

                if (prayerTimes == null)
                {
                    prayerTimes = await muwaqqitApiService.GetTimesAsync(date, longitude, latitude, fajrDegree, ishaDegree, ishtibaqDegree, asrKarahaDegree, timezone).ConfigureAwait(false);
                    await muwaqqitDBAccess.InsertMuwaqqitPrayerTimesAsync(prayerTimes).ConfigureAwait(false);
                }

                return prayerTimes;
            }
        }

        public Task<BaseLocationData> GetLocationInfo(CompletePlaceInfo place)
        {
            if (place == null)
                throw new ArgumentNullException(nameof(place));

            return Task.FromResult<BaseLocationData>(new MuwaqqitLocationData
            {
                Longitude = place.Longitude,
                Latitude = place.Latitude,
                TimezoneName = place.TimezoneInfo.Name
            });
        }
    }
}
