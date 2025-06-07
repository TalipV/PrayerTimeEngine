using AsyncAwaitBestPractices;
using Microsoft.Extensions.Logging;
using NodaTime;
using PrayerTimeEngine.Core.Common;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;
using System.Collections.Concurrent;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Management;

public class DynamicPrayerTimeProviderManager(
        IDynamicPrayerTimeProviderFactory prayerTimeServiceFactory,
        IProfileService profileService,
        ISystemInfoService systemInfoService,
        ILogger<DynamicPrayerTimeProviderManager> logger,
        IEnumerable<IPrayerTimeCacheCleaner> cacheCleaners
    ) : IDynamicPrayerTimeProviderManager
{
    private readonly ConcurrentDictionary<(ZonedDateTime, int), (Profile, DynamicPrayerTimesDaySet)> _cachedDateAndProfileIDToPrayerTimeBundle = new();

    private bool tryGetCachedCalculation(
        Profile profile,
        ZonedDateTime date,
        out DynamicPrayerTimesDaySet prayerTimeEntity)
    {
        // no cache
        if (!_cachedDateAndProfileIDToPrayerTimeBundle.TryGetValue((date, profile.ID), out (Profile, DynamicPrayerTimesDaySet) cachedValue))
        {
            prayerTimeEntity = null;
            return false;
        }

        // there is only a cache for a different profile config
        if (!Equals(cachedValue.Item1, profile))
        {
            prayerTimeEntity = null;
            return false;
        }

        prayerTimeEntity = cachedValue.Item2;
        return true;
    }

    public async Task<DynamicPrayerTimesDaySet> CalculatePrayerTimesAsync(int profileID, ZonedDateTime date, CancellationToken cancellationToken)
    {
        date = date.LocalDateTime.Date.AtStartOfDayInZone(date.Zone);

        Profile profile = await profileService.GetUntrackedReferenceOfProfile(profileID, cancellationToken).ConfigureAwait(false)
            ?? throw new Exception($"The Profile with the ID '{profileID}' could not be found");

        DynamicProfile dynamicProfile = profile as DynamicProfile
            ?? throw new Exception($"The Profile with the ID '{profileID}' is not a {nameof(DynamicProfile)}");

        if (tryGetCachedCalculation(dynamicProfile, date, out DynamicPrayerTimesDaySet prayerTimeEntity))
        {
            prayerTimeEntity.DataCalculationTimestamp = systemInfoService.GetCurrentZonedDateTime();
            return prayerTimeEntity;
        }

        prayerTimeEntity = new DynamicPrayerTimesDaySet
        {
            PreviousDay = new DynamicPrayerTimesDay(),
            CurrentDay = new DynamicPrayerTimesDay(),
            NextDay = new DynamicPrayerTimesDay(),
        };

        await calculateInternal(prayerTimeEntity.CurrentDay, dynamicProfile, date, cancellationToken);
        await calculateInternal(prayerTimeEntity.PreviousDay, dynamicProfile, date.Plus(Duration.FromDays(-1)), cancellationToken);
        await calculateInternal(prayerTimeEntity.NextDay, dynamicProfile, date.Plus(Duration.FromDays(1)), cancellationToken);

        prayerTimeEntity.DataCalculationTimestamp = systemInfoService.GetCurrentZonedDateTime();

        _cachedDateAndProfileIDToPrayerTimeBundle[(date, dynamicProfile.ID)] = (dynamicProfile, prayerTimeEntity);

        // execute without awaiting because we don't want to await and block for this side quest
        cleanUpOldCacheData(cancellationToken).SafeFireAndForget(exception => logger.LogError(exception, "Error during cleanUpOldCacheData"));

        return prayerTimeEntity;
    }

    private async Task calculateInternal(DynamicPrayerTimesDay targetSet, DynamicProfile profile, ZonedDateTime date, CancellationToken ct)
    {
        var complex = await calculateComplexTypes(profile, date, ct).ConfigureAwait(false);

        foreach (var (type, time) in complex)
        {
            ct.ThrowIfCancellationRequested();
            targetSet.SetSpecificPrayerTimeDateTime(type, time);
        }

        foreach (var (type, time) in calculateSimpleTypes(profile, targetSet))
            targetSet.SetSpecificPrayerTimeDateTime(type, time);
    }

    private async Task cleanUpOldCacheData(CancellationToken cancellationToken)
    {
        ZonedDateTime deleteBeforeDate = systemInfoService.GetCurrentZonedDateTime().Minus(Duration.FromDays(3));

        foreach (IPrayerTimeCacheCleaner cacheCleaner in cacheCleaners)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await cacheCleaner.DeleteCacheDataAsync(deleteBeforeDate, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LogError(exception, "Error while trying to clean cache with {CacheCleanerFullName}", cacheCleaner.GetType().FullName);
            }
        }
    }

    private async Task<List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)>> calculateComplexTypes(
        DynamicProfile dynamicProfile,
        ZonedDateTime date,
        CancellationToken cancellationToken)
    {
        List<Task<List<(ETimeType, ZonedDateTime)>>> calculatorTasks = [];

        foreach (var timeConfigsByCalcSource in profileService.GetActiveComplexTimeConfigs(dynamicProfile).GroupBy(x => x.Source))
        {
            EDynamicPrayerTimeProviderType dynamicPrayerTimeProviderType = timeConfigsByCalcSource.Key;
            List<GenericSettingConfiguration> configs = [.. timeConfigsByCalcSource];

            IDynamicPrayerTimeProvider dynamicPrayerTimeProvider = prayerTimeServiceFactory.GetDynamicPrayerTimeProviderByDynamicPrayerTimeProvider(dynamicPrayerTimeProviderType);
            throwIfConfigsHaveUnsupportedTimeTypes(dynamicPrayerTimeProvider, dynamicPrayerTimeProviderType, configs);
            BaseLocationData locationData = profileService.GetLocationConfig(dynamicProfile, dynamicPrayerTimeProviderType);

            // missing location info only means that the associated times are not calculated (i.e. remain at null)
            if (locationData == null)
                continue;

            try
            {
                var calculatorTask =
                    dynamicPrayerTimeProvider.GetPrayerTimesAsync(date, locationData, configs, cancellationToken)
                        .ContinueWith(task =>
                        {
                            // exception case
                            if (!task.IsCompletedSuccessfully)
                            {
                                // getting the result throws exception
                                //return task.GetAwaiter().GetResult();

                                // TODO: delegate error info outside
                                // by just setting null for this calculator's calculation values
                                // without stopping the execution flow with an exception
                                return [];
                            }

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

    private IEnumerable<(ETimeType, ZonedDateTime?)> calculateSimpleTypes(DynamicProfile dynamicProfile, DynamicPrayerTimesDay prayerTimeEntity)
    {
        if (prayerTimeEntity.Dhuhr?.Start is not null
            && profileService.GetTimeConfig(dynamicProfile, ETimeType.DuhaEnd) is GenericSettingConfiguration duhaConfig
            && duhaConfig.IsTimeShown)
        {
            yield return (
                ETimeType.DuhaEnd,
                prayerTimeEntity.Dhuhr.Start.Value.PlusMinutes(duhaConfig.MinuteAdjustment));
        }

        if (prayerTimeEntity.Maghrib?.Start is not null
            && profileService.GetTimeConfig(dynamicProfile, ETimeType.MaghribSufficientTime) is GenericSettingConfiguration maghribSufficientTimeConfig
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

            Duration halfOfDayDuration = dayDuration / 2.0;
            yield return (
                ETimeType.DuhaHalfOfDay,
                prayerTimeEntity.Fajr.Start.Value + halfOfDayDuration);
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