using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using AsyncAwaitBestPractices;
using Microsoft.Extensions.Logging;
using NodaTime;
using PrayerTimeEngine.Core.Common;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;

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
    /// Caches the most recent calculation per profile because consumers like the
    /// persistent notification request the same times again every few seconds.
    ///
    /// Only a single day set is kept per profile (practically the current day), so newer
    /// calculations just replace older ones and the cache stays bounded without any clean up logic.
    ///
    /// A partial result (at least one provider failed) is cached as well, so the times of the
    /// working providers stay available, but only until the failed providers may be retried.
    /// </summary>
    private readonly ConcurrentDictionary<int, CachedDaySetEntry> _cachedDaySetByProfileID = new();

    private sealed record CachedDaySetEntry(
        ZonedDateTime Date,
        long ProfileVersion,
        DynamicPrayerTimesDaySet DaySet,
        List<DynamicPrayerTimeCalculationErrorVO> CalculationErrors,
        Instant? RecalculateAt);

    /// <summary>
    /// Identifies a single provider request: one provider for one profile configuration on one day.
    /// </summary>
    private sealed record ProviderRequestKey(
        EDynamicPrayerTimeProviderType ProviderType,
        int ProfileID,
        long ProfileVersion,
        LocalDate Date);

    private sealed class ProviderFailureState
    {
        public int ConsecutiveFailureCount { get; set; }
        public Instant RetryAt { get; set; }
        public Exception? LastException { get; set; }
    }

    private static readonly Duration s_initialRetryDelay = Duration.FromSeconds(30);
    private static readonly Duration s_maximumRetryDelay = Duration.FromMinutes(15);

    /// <summary>
    /// Backoff per failed provider request. Without it every consumer retries a failed provider
    /// immediately, e.g. the persistent notification on every tick, which hammers an unavailable
    /// API every few seconds for as long as it is down.
    /// </summary>
    private readonly ConcurrentDictionary<ProviderRequestKey, ProviderFailureState> _failureStateByRequest = new();

    private bool TryGetCachedEntry(
        int profileID,
        ZonedDateTime date,
        long profileVersion,
        [NotNullWhen(true)] out CachedDaySetEntry? cacheEntry)
    {
        return _cachedDaySetByProfileID.TryGetValue(profileID, out cacheEntry)
            && cacheEntry.Date == date
            // reusing the cache is only valid as long as the profile configuration stayed the same
            && cacheEntry.ProfileVersion == profileVersion;
    }

    public bool TryGetAlreadyCalculatedPrayerTimes(int profileID, ZonedDateTime date, out DynamicPrayerTimesDaySet daySet)
    {
        // normalize identically to CalculatePrayerTimesAsync so the cache key (start of day) matches
        date = date.LocalDateTime.Date.AtStartOfDayInZone(date.Zone);

        // partial results are returned as well, showing the available times beats showing none
        if (TryGetCachedEntry(profileID, date, profileService.GetProfileVersion(profileID), out CachedDaySetEntry? cacheEntry))
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
        long profileVersion = profileService.GetProfileVersion(profileID);

        // Fast path: version check is in-memory, no DB round-trip needed
        if (TryGetCachedEntry(profileID, date, profileVersion, out CachedDaySetEntry? cacheEntry)
            && (cacheEntry.RecalculateAt is null || systemInfoService.GetCurrentInstant() < cacheEntry.RecalculateAt))
        {
            cacheEntry.DaySet.DataCalculationTimestamp = systemInfoService.GetCurrentZonedDateTime();

            return new CalculatePrayerTimesResultVO
            {
                DynamicPrayerTimesDaySet = cacheEntry.DaySet,
                CalculationErrors = [.. cacheEntry.CalculationErrors]
            };
        }

        Profile profile = await profileService.GetUntrackedReferenceOfProfile(profileID, cancellationToken).ConfigureAwait(false)
            ?? throw new Exception($"The Profile with the ID '{profileID}' could not be found");

        DynamicProfile dynamicProfile = profile as DynamicProfile
            ?? throw new Exception($"The Profile with the ID '{profileID}' is not a {nameof(DynamicProfile)}");

        var prayerTimeEntity = new DynamicPrayerTimesDaySet
        {
            PreviousDay = new DynamicPrayerTimesDay(),
            CurrentDay = new DynamicPrayerTimesDay(),
            NextDay = new DynamicPrayerTimesDay(),
        };

        // the three days are calculated in parallel (and within each day the providers are requested in parallel)
        Task<List<DynamicPrayerTimeCalculationErrorVO>> currentDayTask = CalculateInternal(prayerTimeEntity.CurrentDay, dynamicProfile, profileVersion, date, cancellationToken);
        Task<List<DynamicPrayerTimeCalculationErrorVO>> previousDayTask = CalculateInternal(prayerTimeEntity.PreviousDay, dynamicProfile, profileVersion, date.Plus(Duration.FromDays(-1)), cancellationToken);
        Task<List<DynamicPrayerTimeCalculationErrorVO>> nextDayTask = CalculateInternal(prayerTimeEntity.NextDay, dynamicProfile, profileVersion, date.Plus(Duration.FromDays(1)), cancellationToken);

        await Task.WhenAll(currentDayTask, previousDayTask, nextDayTask).ConfigureAwait(false);

        List<DynamicPrayerTimeCalculationErrorVO> calculationErrors =
            [.. currentDayTask.Result, .. previousDayTask.Result, .. nextDayTask.Result];

        prayerTimeEntity.DataCalculationTimestamp = systemInfoService.GetCurrentZonedDateTime();

        // erroneous results are cached too, but only until the earliest failed provider may be retried
        _cachedDaySetByProfileID[dynamicProfile.ID] = new CachedDaySetEntry(
            date,
            profileVersion,
            prayerTimeEntity,
            calculationErrors,
            RecalculateAt: calculationErrors.Count == 0
                ? null
                : GetEarliestRetryAt(dynamicProfile.ID, profileVersion, calculationErrors));

        // execute without awaiting because we don't want to await and block for this side quest
        CleanUpOldCacheData(cancellationToken).SafeFireAndForget(exception => logger.LogError(exception, "Error during cleanUpOldCacheData"));

        return new CalculatePrayerTimesResultVO
        {
            DynamicPrayerTimesDaySet = prayerTimeEntity,
            CalculationErrors = calculationErrors
        };
    }

    private async Task<List<DynamicPrayerTimeCalculationErrorVO>> CalculateInternal(
        DynamicPrayerTimesDay targetSet,
        DynamicProfile profile,
        long profileVersion,
        ZonedDateTime date,
        CancellationToken ct)
    {
        (List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)>? prayerTimes, List<DynamicPrayerTimeCalculationErrorVO>? calculationErrors) = await CalculateComplexTypes(profile, profileVersion, date, ct).ConfigureAwait(false);

        foreach ((ETimeType type, ZonedDateTime time) in prayerTimes)
        {
            ct.ThrowIfCancellationRequested();
            targetSet.SetSpecificPrayerTimeDateTime(type, time);
        }

        foreach ((ETimeType type, ZonedDateTime? time) in CalculateSimpleTypes(profile, targetSet))
        {
            targetSet.SetSpecificPrayerTimeDateTime(type, time);
        }

        return calculationErrors;
    }

    private async Task CleanUpOldCacheData(CancellationToken cancellationToken)
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

    private async Task<(List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)> PrayerTimes, List<DynamicPrayerTimeCalculationErrorVO> CalculationErrors)> CalculateComplexTypes(
        DynamicProfile dynamicProfile,
        long profileVersion,
        ZonedDateTime date,
        CancellationToken cancellationToken)
    {
        List<Task<ComplexCalculationResult>> calculatorTasks = [];

        foreach (IGrouping<EDynamicPrayerTimeProviderType, GenericSettingConfiguration> timeConfigsByCalcSource in profileService.GetActiveComplexTimeConfigs(dynamicProfile).GroupBy(x => x.Source))
        {
            EDynamicPrayerTimeProviderType dynamicPrayerTimeProviderType = timeConfigsByCalcSource.Key;
            List<GenericSettingConfiguration> configs = [.. timeConfigsByCalcSource];

            BaseLocationData locationData = profileService.GetLocationConfig(dynamicProfile, dynamicPrayerTimeProviderType);

            // missing location info only means that the associated times are not calculated (i.e. remain at null)
            if (locationData == null)
            {
                continue;
            }

            calculatorTasks.Add(CalculateComplexTypesForCalculator(
                locationData,
                new ProviderRequestKey(dynamicPrayerTimeProviderType, dynamicProfile.ID, profileVersion, date.LocalDateTime.Date),
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

    private async Task<ComplexCalculationResult> CalculateComplexTypesForCalculator(
        BaseLocationData locationData,
        ProviderRequestKey requestKey,
        List<GenericSettingConfiguration> configs,
        ZonedDateTime date,
        CancellationToken cancellationToken)
    {
        EDynamicPrayerTimeProviderType dynamicPrayerTimeProviderType = requestKey.ProviderType;

        // the provider failed only recently for this request, so it is not asked again before its retry delay passed
        if (TryGetActiveBackoff(requestKey, out Exception? lastException))
        {
            return new ComplexCalculationResult([], CreateCalculationError(requestKey, configs, lastException));
        }

        try
        {
            IDynamicPrayerTimeProvider dynamicPrayerTimeProvider = prayerTimeServiceFactory.GetDynamicPrayerTimeProviderByDynamicPrayerTimeProvider(dynamicPrayerTimeProviderType);

            // within the try block so that a single invalid provider config doesn't kill the calculations of the other providers
            ThrowIfConfigsHaveUnsupportedTimeTypes(dynamicPrayerTimeProvider, dynamicPrayerTimeProviderType, configs);

            List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)> calculationResults =
                await dynamicPrayerTimeProvider.GetPrayerTimesAsync(date, locationData, configs, cancellationToken).ConfigureAwait(false);

            RegisterSuccess(requestKey);

            return new ComplexCalculationResult(
                PrayerTimes: [.. calculationResults
                    .Select(calculation =>
                    {
                        GenericSettingConfiguration config = configs.First(config => config.TimeType == calculation.TimeType);
                        return (calculation.TimeType, calculation.ZonedDateTime.PlusMinutes(config.MinuteAdjustment));
                    })],
                CalculationError: null);
        }
        catch (Exception exception) when (IsCausedByCancellation(exception, cancellationToken))
        {
            // A caller-side cancellation (e.g. the notification's short render timeout) can surface from
            // the HTTP stack (Refit / the resilience pipeline) as something other than a plain
            // OperationCanceledException. Treat it as a cancellation instead of logging it as a
            // calculation error, so it is handled uniformly by the OperationCanceledException-aware callers.
            throw new OperationCanceledException("Prayer time calculation was canceled.", exception, cancellationToken);
        }
        catch (Exception exception)
        {
            RegisterFailure(requestKey, exception);

            return new ComplexCalculationResult([], CreateCalculationError(requestKey, configs, exception));
        }
    }

    private static DynamicPrayerTimeCalculationErrorVO CreateCalculationError(
        ProviderRequestKey requestKey,
        List<GenericSettingConfiguration> configs,
        Exception exception)
    {
        return new DynamicPrayerTimeCalculationErrorVO
        {
            DynamicPrayerTimeProviderType = requestKey.ProviderType,
            Date = requestKey.Date,
            TimeTypes = [.. configs.Select(x => x.TimeType).Distinct().OrderBy(x => x.ToString())],
            Exception = exception,
        };
    }

    private bool TryGetActiveBackoff(ProviderRequestKey requestKey, [NotNullWhen(true)] out Exception? lastException)
    {
        if (_failureStateByRequest.TryGetValue(requestKey, out ProviderFailureState? failureState))
        {
            lock (failureState)
            {
                if (failureState.LastException is not null && systemInfoService.GetCurrentInstant() < failureState.RetryAt)
                {
                    lastException = failureState.LastException;
                    return true;
                }
            }
        }

        lastException = null;
        return false;
    }

    /// <summary>
    /// Starts the next, longer backoff period for the request. Only the first failure of an outage is
    /// logged as an error (with the stack trace); the following ones are logged once per backoff period
    /// as a short warning, so an outage of several hours doesn't flood the log.
    /// </summary>
    private void RegisterFailure(ProviderRequestKey requestKey, Exception exception)
    {
        Instant now = systemInfoService.GetCurrentInstant();

        // stale entries (e.g. of an outage that ended without a later successful request for that day)
        foreach (ProviderRequestKey staleKey in _failureStateByRequest.Keys.Where(key => key.Date < requestKey.Date.PlusDays(-3)))
        {
            _failureStateByRequest.TryRemove(staleKey, out _);
        }

        ProviderFailureState failureState = _failureStateByRequest.GetOrAdd(requestKey, _ => new ProviderFailureState());
        int consecutiveFailureCount;
        Duration retryDelay;

        lock (failureState)
        {
            failureState.LastException = exception;

            // a parallel request for the same key already failed and started the current backoff period
            if (failureState.ConsecutiveFailureCount > 0 && now < failureState.RetryAt)
            {
                return;
            }

            failureState.ConsecutiveFailureCount++;
            consecutiveFailureCount = failureState.ConsecutiveFailureCount;
            retryDelay = GetRetryDelay(consecutiveFailureCount);
            failureState.RetryAt = now + retryDelay;
        }

        if (consecutiveFailureCount == 1)
        {
            logger.LogError(exception,
                "Error while calculating complex prayer times for {DynamicPrayerTimeProviderType} on {Date}, next attempt in {RetryDelay}",
                requestKey.ProviderType,
                requestKey.Date,
                retryDelay);
        }
        else
        {
            logger.LogWarning(
                "Complex prayer times for {DynamicPrayerTimeProviderType} on {Date} still failing ({ConsecutiveFailureCount} consecutive failures, last error: {ErrorMessage}), next attempt in {RetryDelay}",
                requestKey.ProviderType,
                requestKey.Date,
                consecutiveFailureCount,
                exception.Message,
                retryDelay);
        }
    }

    private void RegisterSuccess(ProviderRequestKey requestKey)
    {
        if (_failureStateByRequest.TryRemove(requestKey, out ProviderFailureState? failureState))
        {
            logger.LogInformation(
                "Complex prayer times for {DynamicPrayerTimeProviderType} on {Date} available again after {ConsecutiveFailureCount} consecutive failures",
                requestKey.ProviderType,
                requestKey.Date,
                failureState.ConsecutiveFailureCount);
        }
    }

    /// <summary>Doubles with every consecutive failure: 30 s, 1 min, 2 min, ... up to 15 min.</summary>
    private static Duration GetRetryDelay(int consecutiveFailureCount)
    {
        Duration retryDelay = s_initialRetryDelay * Math.Pow(2, Math.Min(consecutiveFailureCount - 1, 10));
        return retryDelay < s_maximumRetryDelay ? retryDelay : s_maximumRetryDelay;
    }

    private Instant GetEarliestRetryAt(int profileID, long profileVersion, List<DynamicPrayerTimeCalculationErrorVO> calculationErrors)
    {
        Instant now = systemInfoService.GetCurrentInstant();

        return calculationErrors
            .Select(error =>
            {
                var requestKey = new ProviderRequestKey(error.DynamicPrayerTimeProviderType, profileID, profileVersion, error.Date);

                if (!_failureStateByRequest.TryGetValue(requestKey, out ProviderFailureState? failureState))
                {
                    // no backoff (anymore), e.g. because a parallel calculation succeeded in the meantime
                    return now;
                }

                lock (failureState)
                {
                    return failureState.RetryAt;
                }
            })
            .Min();
    }

    /// <summary>
    /// Whether the exception is (or wraps) a cancellation, or the token has been canceled. Needed because
    /// the HTTP stack does not always surface a canceled request as a plain <see cref="OperationCanceledException"/>.
    /// </summary>
    private static bool IsCausedByCancellation(Exception exception, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return true;
        }

        for (Exception current = exception; current is not null; current = current.InnerException)
        {
            if (current is OperationCanceledException)
            {
                return true;
            }
        }

        return false;
    }

    private IEnumerable<(ETimeType, ZonedDateTime?)> CalculateSimpleTypes(DynamicProfile dynamicProfile, DynamicPrayerTimesDay prayerTimeEntity)
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

    private static void ThrowIfConfigsHaveUnsupportedTimeTypes(
        IDynamicPrayerTimeProvider timeCalculator,
        EDynamicPrayerTimeProviderType dynamicPrayerTimeProviderType,
        List<GenericSettingConfiguration> configs)
    {
        var unsupportedTimeTypes =
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
