using Microsoft.Extensions.Logging;
using NodaTime;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.Calculators;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;

namespace PrayerTimeEngine.Core.Domain.CalculationManagement
{
    public class CalculationManager(
            IPrayerTimeCalculatorFactory prayerTimeServiceFactory,
            IProfileService profileService,
            ILogger<CalculationManager> logger
        ) : ICalculationManager
    {
        private LocalDate? _cachedCalculationDate = null;
        private Profile _cachedProfile = null;
        private PrayerTimesBundle _cachedPrayerTimeBundle = null;

        private bool tryGetCachedCalculation(
            Profile profile, 
            LocalDate date, 
            out PrayerTimesBundle prayerTimeEntity)
        {
            // no cache
            if (_cachedCalculationDate is null 
                || _cachedProfile is null 
                || _cachedPrayerTimeBundle is null)
            {
                prayerTimeEntity = null;
                return false;
            }

            // wrong input params for cache
            if (_cachedCalculationDate != date || !_cachedProfile.Equals(profile))
            {
                prayerTimeEntity = null;
                return false;
            }

            prayerTimeEntity = _cachedPrayerTimeBundle;
            return true;
        }

        public async Task<PrayerTimesBundle> CalculatePrayerTimesAsync(int profileID, ZonedDateTime zoneDate, CancellationToken cancellationToken)
        {
            LocalDate date = zoneDate.Date;
            DateTimeZone zone = zoneDate.Zone;
            Profile profile = await profileService.GetUntrackedReferenceOfProfile(profileID, cancellationToken).ConfigureAwait(false);

            if (tryGetCachedCalculation(profile, date, out PrayerTimesBundle prayerTimeEntity))
            {
                prayerTimeEntity.DataCalculationTimestamp = SystemClock.Instance.GetCurrentInstant().InZone(zone);
                return prayerTimeEntity;
            }

            prayerTimeEntity = new PrayerTimesBundle();

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

            prayerTimeEntity.DataCalculationTimestamp = SystemClock.Instance.GetCurrentInstant().InZone(zone);
            _cachedCalculationDate = date;
            _cachedProfile = profile;
            _cachedPrayerTimeBundle = prayerTimeEntity;
            return prayerTimeEntity;
        }

        private async Task<List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)>> calculateInternal(
            Profile profile,
            LocalDate date,
            CancellationToken cancellationToken)
        {
            List<Task<List<(ETimeType, ZonedDateTime)>>> calculatorTasks = [];

            foreach (var timeConfigsByCalcSource in profileService.GetActiveComplexTimeConfigs(profile).GroupBy(x => x.Source))
            {
                ECalculationSource calculationSource = timeConfigsByCalcSource.Key;
                List<GenericSettingConfiguration> configs = [.. timeConfigsByCalcSource];

                IPrayerTimeCalculator prayerTimeCalculator = prayerTimeServiceFactory.GetPrayerTimeCalculatorByCalculationSource(calculationSource);
                throwIfConfigsHaveUnsupportedTimeTypes(prayerTimeCalculator, calculationSource, configs);
                BaseLocationData locationData = profileService.GetLocationConfig(profile, calculationSource);

                try
                {
                    var calculatorTask = 
                        prayerTimeCalculator.GetPrayerTimesAsync(date, locationData, configs, cancellationToken)
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
                    logger.LogError(ex, "Error for {CalculatorName}", prayerTimeCalculator.GetType().Name);
                }
            }

            return (await Task.WhenAll(calculatorTasks).ConfigureAwait(false)).SelectMany(x => x).ToList();
        }

        private IEnumerable<(ETimeType, ZonedDateTime?)> calculateSimpleTypes(Profile profile, PrayerTimesBundle prayerTimeEntity)
        {
            if (prayerTimeEntity.Dhuhr?.Start != null
                && profileService.GetTimeConfig(profile, ETimeType.DuhaEnd) is GenericSettingConfiguration duhaConfig
                && duhaConfig.IsTimeShown)
            {
                yield return (
                    ETimeType.DuhaEnd,
                    prayerTimeEntity.Dhuhr.Start.Value.PlusMinutes(duhaConfig.MinuteAdjustment));
            }

            if (prayerTimeEntity.Maghrib?.Start != null
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
            IPrayerTimeCalculator timeCalculator,
            ECalculationSource calculationSource,
            List<GenericSettingConfiguration> configs)
        {
            List<ETimeType> unsupportedTimeTypes =
                timeCalculator
                .GetUnsupportedTimeTypes().Intersect(configs.Select(x => x.TimeType))
                .ToList();

            if (unsupportedTimeTypes.Count != 0)
            {
                throw new ArgumentException(
                    $"{timeCalculator.GetType().Name}[{calculationSource}] does not support the following values of {nameof(ETimeType)}: " +
                    string.Join(", ", unsupportedTimeTypes));
            }
        }
    }
}