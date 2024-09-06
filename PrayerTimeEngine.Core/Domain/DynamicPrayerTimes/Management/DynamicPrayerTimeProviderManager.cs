using Microsoft.Extensions.Logging;
using NodaTime;
using PrayerTimeEngine.Core.Common;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;
using System.Collections.Concurrent;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Management
{
    public class DynamicPrayerTimeProviderManager(
            IDynamicPrayerTimeProviderFactory prayerTimeServiceFactory,
            IProfileService profileService,
            ISystemInfoService systemInfoService,
            ILogger<DynamicPrayerTimeProviderManager> logger,
            IEnumerable<IPrayerTimeCacheCleaner> cacheCleaners
        ) : IDynamicPrayerTimeProviderManager
    {
        private readonly ConcurrentDictionary<(ZonedDateTime, int), (Profile, PrayerTimesCollection)> _cachedDateAndProfileIDToPrayerTimeBundle = new();

        private bool tryGetCachedCalculation(
            Profile profile,
            ZonedDateTime date,
            out PrayerTimesCollection prayerTimeEntity)
        {
            // no cache
            if (!_cachedDateAndProfileIDToPrayerTimeBundle.TryGetValue((date, profile.ID), out (Profile, PrayerTimesCollection) cachedValue))
            {
                prayerTimeEntity = null;
                return false;
            }

            // there is only a cache for a different profile config
            if (!cachedValue.Item1.Equals(profile))
            {
                prayerTimeEntity = null;
                return false;
            }

            prayerTimeEntity = cachedValue.Item2;
            return true;
        }

        public async Task<PrayerTimesCollection> CalculatePrayerTimesAsync(int profileID, ZonedDateTime date, CancellationToken cancellationToken)
        {
            date = date.LocalDateTime.Date.AtStartOfDayInZone(date.Zone);
            Profile profile = await profileService.GetUntrackedReferenceOfProfile(profileID, cancellationToken).ConfigureAwait(false);

            if (tryGetCachedCalculation(profile, date, out PrayerTimesCollection prayerTimeEntity))
            {
                prayerTimeEntity.DataCalculationTimestamp = systemInfoService.GetCurrentZonedDateTime();
                return prayerTimeEntity;
            }

            prayerTimeEntity = new PrayerTimesCollection();

            var complexTypeCalculations =
                await calculateInternal(profile, date, cancellationToken).ConfigureAwait(false);

            var allCalculations =
                complexTypeCalculations
                    .Select(x => (x.TimeType, (ZonedDateTime?)x.ZonedDateTime))
                    // left as (non-list) IEnumerable because it has to run after
                    // the complex calculations landed in prayerTimeEntity
                    .Concat(calculateSimpleTypes(profile, prayerTimeEntity));

            foreach ((ETimeType timeType, ZonedDateTime? zonedDateTime) in allCalculations)
            {
                cancellationToken.ThrowIfCancellationRequested();
                prayerTimeEntity.SetSpecificPrayerTimeDateTime(timeType, zonedDateTime);
            }

            prayerTimeEntity.DataCalculationTimestamp = systemInfoService.GetCurrentZonedDateTime();

            _cachedDateAndProfileIDToPrayerTimeBundle[(date, profile.ID)] = (profile, prayerTimeEntity);

            cleanUpOldCacheData(cancellationToken);

            return prayerTimeEntity;
        }

        private void cleanUpOldCacheData(CancellationToken cancellationToken)
        {
            ZonedDateTime deleteBeforeDate = systemInfoService.GetCurrentZonedDateTime().Minus(Duration.FromDays(3));

            foreach (IPrayerTimeCacheCleaner cacheCleaner in cacheCleaners)
            {
                try
                {
                    cacheCleaner.DeleteCacheDataAsync(deleteBeforeDate, cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error while trying to clean cache with {CacheCleanerFullName}", cacheCleaner.GetType().FullName);
                }
            }
        }

        private async Task<List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)>> calculateInternal(
            Profile profile,
            ZonedDateTime date,
            CancellationToken cancellationToken)
        {
            List<Task<List<(ETimeType, ZonedDateTime)>>> calculatorTasks = [];

            foreach (var timeConfigsByCalcSource in profileService.GetActiveComplexTimeConfigs(profile).GroupBy(x => x.Source))
            {
                EDynamicPrayerTimeProviderType dynamicPrayerTimeProviderType = timeConfigsByCalcSource.Key;
                List<GenericSettingConfiguration> configs = [.. timeConfigsByCalcSource];

                IDynamicPrayerTimeProvider dynamicPrayerTimeProvider = prayerTimeServiceFactory.GetDynamicPrayerTimeProviderByDynamicPrayerTimeProvider(dynamicPrayerTimeProviderType);
                throwIfConfigsHaveUnsupportedTimeTypes(dynamicPrayerTimeProvider, dynamicPrayerTimeProviderType, configs);
                BaseLocationData locationData = profileService.GetLocationConfig(profile, dynamicPrayerTimeProviderType);

                try
                {
                    var calculatorTask =
                        dynamicPrayerTimeProvider.GetPrayerTimesAsync(date, locationData, configs, cancellationToken)
                            .ContinueWith(task =>
                            {
                                if (!task.IsCompletedSuccessfully)
                                    return task.GetAwaiter().GetResult();

                                return task.GetAwaiter().GetResult()
                                    .Select(calculation =>
                                    {
                                        GenericSettingConfiguration config = configs.First(config => config.TimeType == calculation.TimeType);
                                        return (calculation.TimeType, calculation.ZonedDateTime.PlusMinutes(config.MinuteAdjustment));
                                    })
                                    .ToList();
                            });

                    calculatorTasks.Add(calculatorTask);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error for {CalculatorName}", dynamicPrayerTimeProviderType.GetType().Name);
                }
            }

            return (await Task.WhenAll(calculatorTasks).ConfigureAwait(false)).SelectMany(x => x).ToList();
        }

        private IEnumerable<(ETimeType, ZonedDateTime?)> calculateSimpleTypes(Profile profile, PrayerTimesCollection prayerTimeEntity)
        {
            if (prayerTimeEntity.Dhuhr?.Start is not null
                && profileService.GetTimeConfig(profile, ETimeType.DuhaEnd) is GenericSettingConfiguration duhaConfig
                && duhaConfig.IsTimeShown)
            {
                yield return (
                    ETimeType.DuhaEnd,
                    prayerTimeEntity.Dhuhr.Start.Value.PlusMinutes(duhaConfig.MinuteAdjustment));
            }

            if (prayerTimeEntity.Maghrib?.Start is not null
                && profileService.GetTimeConfig(profile, ETimeType.MaghribSufficientTime) is GenericSettingConfiguration maghribSufficientTimeConfig
                && maghribSufficientTimeConfig.IsTimeShown)
            {
                yield return (
                    ETimeType.MaghribSufficientTime,
                    prayerTimeEntity.Maghrib.Start.Value.PlusMinutes(maghribSufficientTimeConfig.MinuteAdjustment));
            }

            if (prayerTimeEntity.Asr?.End - prayerTimeEntity.Fajr?.Start is Duration dayDuration)
            {
                Duration quarterOfDayDuration = dayDuration / 4.0;

                yield return (
                    ETimeType.DuhaQuarterOfDay,
                    prayerTimeEntity.Fajr.Start.Value + quarterOfDayDuration);
            }

            if (prayerTimeEntity.Isha?.End - prayerTimeEntity.Maghrib?.Start is Duration nightDuration)
            {
                Duration thirdOfNightDuration = nightDuration / 3.0;

                yield return (
                    ETimeType.IshaFirstThird,
                    prayerTimeEntity.Maghrib.Start.Value + thirdOfNightDuration);

                Duration halfOfNightDuration = nightDuration / 2.0;

                yield return (
                    ETimeType.IshaMidnight,
                    prayerTimeEntity.Maghrib.Start.Value + halfOfNightDuration);

                Duration twoThirdsOfNightDuration = nightDuration * (2.0 / 3.0);

                yield return (
                    ETimeType.IshaSecondThird,
                    prayerTimeEntity.Maghrib.Start.Value + twoThirdsOfNightDuration);
            }
        }

        private static void throwIfConfigsHaveUnsupportedTimeTypes(
            IDynamicPrayerTimeProvider timeCalculator,
            EDynamicPrayerTimeProviderType dynamicPrayerTimeProviderType,
            List<GenericSettingConfiguration> configs)
        {
            List<ETimeType> unsupportedTimeTypes =
                timeCalculator
                .GetUnsupportedTimeTypes().Intersect(configs.Select(x => x.TimeType))
                .ToList();

            if (unsupportedTimeTypes.Count != 0)
            {
                throw new ArgumentException(
                    $"{timeCalculator.GetType().Name}[{dynamicPrayerTimeProviderType}] does not support the following values of {nameof(ETimeType)}: " +
                    string.Join(", ", unsupportedTimeTypes));
            }
        }
    }
}