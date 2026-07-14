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
    private sealed record ComplexCalculationResult(
        List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)> PrayerTimes,
        DynamicPrayerTimeCalculationErrorVO CalculationError);

    /// <summary>
    /// Caches the most recent successful calculation per profile because consumers like the
    /// persistent notification request the same times again every few seconds.
    ///
    /// Only a single day set is kept per profile (practically the current day), so newer
    /// calculations just replace older ones and the cache stays bounded without any clean up logic.
    /// </summary>
    private readonly ConcurrentDictionary<int, CachedDaySetEntry> _cachedDaySetByProfileID = new();

    private sealed record CachedDaySetEntry(
        ZonedDateTime Date,
        long ProfileVersion,
        DynamicPrayerTimesDaySet DaySet);

    private bool tryGetCachedDaySet(
        int profileID,
        ZonedDateTime date,
        out DynamicPrayerTimesDaySet daySet)
    {
        if (_cachedDaySetByProfileID.TryGetValue(profileID, out CachedDaySetEntry cacheEntry)
            && cacheEntry.Date == date
            // reusing the cache is only valid as long as the profile configuration stayed the same
            && cacheEntry.ProfileVersion == profileService.GetProfileVersion(profileID))
        {
            daySet = cacheEntry.DaySet;
            return true;
        }

        daySet = null;
        return false;
    }

    public async Task<CalculatePrayerTimesResultVO> CalculatePrayerTimesAsync(int profileID, ZonedDateTime date, CancellationToken cancellationToken)
    {
        date = date.LocalDateTime.Date.AtStartOfDayInZone(date.Zone);

        // Fast path: version check is in-memory, no DB round-trip needed
        if (tryGetCachedDaySet(profileID, date, out DynamicPrayerTimesDaySet prayerTimeEntity))
        {
            prayerTimeEntity.DataCalculationTimestamp = systemInfoService.GetCurrentZonedDateTime();

            return new CalculatePrayerTimesResultVO
            {
                DynamicPrayerTimesDaySet = prayerTimeEntity
            };
        }

        Profile profile = await profileService.GetUntrackedReferenceOfProfile(profileID, cancellationToken).ConfigureAwait(false)
            ?? throw new Exception($"The Profile with the ID '{profileID}' could not be found");

        DynamicProfile dynamicProfile = profile as DynamicProfile
            ?? throw new Exception($"The Profile with the ID '{profileID}' is not a {nameof(DynamicProfile)}");

        prayerTimeEntity = new DynamicPrayerTimesDaySet
        {
            PreviousDay = new DynamicPrayerTimesDay(),
            CurrentDay = new DynamicPrayerTimesDay(),
            NextDay = new DynamicPrayerTimesDay(),
        };

        // the three days are calculated in parallel (and within each day the providers are requested in parallel)
        Task<List<DynamicPrayerTimeCalculationErrorVO>> currentDayTask = calculateInternal(prayerTimeEntity.CurrentDay, dynamicProfile, date, cancellationToken);
        Task<List<DynamicPrayerTimeCalculationErrorVO>> previousDayTask = calculateInternal(prayerTimeEntity.PreviousDay, dynamicProfile, date.Plus(Duration.FromDays(-1)), cancellationToken);
        Task<List<DynamicPrayerTimeCalculationErrorVO>> nextDayTask = calculateInternal(prayerTimeEntity.NextDay, dynamicProfile, date.Plus(Duration.FromDays(1)), cancellationToken);

        await Task.WhenAll(currentDayTask, previousDayTask, nextDayTask).ConfigureAwait(false);

        List<DynamicPrayerTimeCalculationErrorVO> calculationErrors =
            [.. currentDayTask.Result, .. previousDayTask.Result, .. nextDayTask.Result];

        prayerTimeEntity.DataCalculationTimestamp = systemInfoService.GetCurrentZonedDateTime();

        // erroneous results are not cached so that the next request retries the calculation
        if (calculationErrors.Count == 0)
        {
            _cachedDaySetByProfileID[dynamicProfile.ID] = new CachedDaySetEntry(
                date,
                profileService.GetProfileVersion(dynamicProfile.ID),
                prayerTimeEntity);
        }

        // execute without awaiting because we don't want to await and block for this side quest
        cleanUpOldCacheData(cancellationToken).SafeFireAndForget(exception => logger.LogError(exception, "Error during cleanUpOldCacheData"));

        return new CalculatePrayerTimesResultVO
        {
            DynamicPrayerTimesDaySet = prayerTimeEntity,
            CalculationErrors = calculationErrors
        };
    }

    private async Task<List<DynamicPrayerTimeCalculationErrorVO>> calculateInternal(
        DynamicPrayerTimesDay targetSet,
        DynamicProfile profile,
        ZonedDateTime date,
        CancellationToken ct)
    {
        var (PrayerTimes, CalculationErrors) = await calculateComplexTypes(profile, date, ct).ConfigureAwait(false);

        foreach (var (type, time) in PrayerTimes)
        {
            ct.ThrowIfCancellationRequested();
            targetSet.SetSpecificPrayerTimeDateTime(type, time);
        }

        foreach (var (type, time) in calculateSimpleTypes(profile, targetSet))
            targetSet.SetSpecificPrayerTimeDateTime(type, time);

        return CalculationErrors;
    }

    private async Task cleanUpOldCacheData(CancellationToken cancellationToken)
    {
        try
        {
            ZonedDateTime deleteBeforeDate = systemInfoService.GetCurrentZonedDateTime().Minus(Duration.FromDays(3));

            foreach (IPrayerTimeCacheCleaner cacheCleaner in cacheCleaners)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                try
                {
                    await cacheCleaner.DeleteCacheDataAsync(deleteBeforeDate.Date, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    logger.LogError(exception,
                        "Error while trying to clean cache with {CacheCleanerFullName}",
                        cacheCleaner.GetType().FullName);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // nothing to do
        }
    }

    private async Task<(List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)> PrayerTimes, List<DynamicPrayerTimeCalculationErrorVO> CalculationErrors)> calculateComplexTypes(
        DynamicProfile dynamicProfile,
        ZonedDateTime date,
        CancellationToken cancellationToken)
    {
        List<Task<ComplexCalculationResult>> calculatorTasks = [];

        foreach (var timeConfigsByCalcSource in profileService.GetActiveComplexTimeConfigs(dynamicProfile).GroupBy(x => x.Source))
        {
            EDynamicPrayerTimeProviderType dynamicPrayerTimeProviderType = timeConfigsByCalcSource.Key;
            List<GenericSettingConfiguration> configs = [.. timeConfigsByCalcSource];

            BaseLocationData locationData = profileService.GetLocationConfig(dynamicProfile, dynamicPrayerTimeProviderType);

            // missing location info only means that the associated times are not calculated (i.e. remain at null)
            if (locationData == null)
                continue;

            calculatorTasks.Add(calculateComplexTypesForCalculator(
                locationData,
                dynamicPrayerTimeProviderType,
                configs,
                date,
                cancellationToken));
        }

        ComplexCalculationResult[] calculatorResults = await Task.WhenAll(calculatorTasks).ConfigureAwait(false);

        return (
            calculatorResults.SelectMany(x => x.PrayerTimes).ToList(),
            calculatorResults
                .Where(x => x.CalculationError is not null)
                .Select(x => x.CalculationError)
                .ToList());
    }

    private async Task<ComplexCalculationResult> calculateComplexTypesForCalculator(
        BaseLocationData locationData,
        EDynamicPrayerTimeProviderType dynamicPrayerTimeProviderType,
        List<GenericSettingConfiguration> configs,
        ZonedDateTime date,
        CancellationToken cancellationToken)
    {
        try
        {
            IDynamicPrayerTimeProvider dynamicPrayerTimeProvider = prayerTimeServiceFactory.GetDynamicPrayerTimeProviderByDynamicPrayerTimeProvider(dynamicPrayerTimeProviderType);

            // within the try block so that a single invalid provider config doesn't kill the calculations of the other providers
            throwIfConfigsHaveUnsupportedTimeTypes(dynamicPrayerTimeProvider, dynamicPrayerTimeProviderType, configs);

            List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)> calculationResults =
                await dynamicPrayerTimeProvider.GetPrayerTimesAsync(date, locationData, configs, cancellationToken).ConfigureAwait(false);

            return new ComplexCalculationResult(
                PrayerTimes: calculationResults
                    .Select(calculation =>
                    {
                        GenericSettingConfiguration config = configs.First(config => config.TimeType == calculation.TimeType);
                        return (calculation.TimeType, calculation.ZonedDateTime.PlusMinutes(config.MinuteAdjustment));
                    })
                    .ToList(),
                CalculationError: null);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception,
                "Error while calculating complex prayer times for {DynamicPrayerTimeProviderType} on {Date}",
                dynamicPrayerTimeProviderType,
                date.LocalDateTime.Date);

            return new ComplexCalculationResult(
                [],
                new DynamicPrayerTimeCalculationErrorVO
                {
                    DynamicPrayerTimeProviderType = dynamicPrayerTimeProviderType,
                    Date = date.LocalDateTime.Date,
                    TimeTypes = configs.Select(x => x.TimeType).Distinct().OrderBy(x => x.ToString()).ToList(),
                    Exception = exception,
                });
        }
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
