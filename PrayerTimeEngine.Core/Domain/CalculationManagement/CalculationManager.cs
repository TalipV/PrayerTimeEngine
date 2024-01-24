using NodaTime;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.Calculators;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models;

namespace PrayerTimeEngine.Core.Domain.CalculationManagement
{
    public class CalculationManager(
            IPrayerTimeServiceFactory prayerTimeServiceFactory,
            IProfileService profileService
        ) : ICalculationManager
    {
        public async Task<PrayerTimesBundle> CalculatePrayerTimesAsync(Profile profile, ZonedDateTime zoneDate)
        {
            LocalDate date = zoneDate.Date;
            DateTimeZone zone = zoneDate.Zone;

            var prayerTimeEntity = new PrayerTimesBundle();

            await foreach ((ETimeType timeType, ZonedDateTime? zonedDateTime) in calculateComplexTypes(profile, date).ConfigureAwait(false))
            {
                prayerTimeEntity.SetSpecificPrayerTimeDateTime(timeType, zonedDateTime);
            }

            foreach ((ETimeType timeType, ZonedDateTime? zonedDateTime) in calculateSimpleTypes(profile, prayerTimeEntity))
            {
                prayerTimeEntity.SetSpecificPrayerTimeDateTime(timeType, zonedDateTime);
            }
            
            prayerTimeEntity.DataCalculationTimestamp = SystemClock.Instance.GetCurrentInstant().InZone(zone);
            return prayerTimeEntity;
        }

        private async IAsyncEnumerable<(ETimeType, ZonedDateTime?)> calculateComplexTypes(Profile profile, LocalDate date)
        {
            var asyncEnumerables = new List<IAsyncEnumerable<(ETimeType, ZonedDateTime?)>>();

            foreach (var timeConfigsByCalcSource in profileService.GetActiveComplexTimeConfigs(profile).GroupBy(x => x.Source))
            {
                ECalculationSource calculationSource = timeConfigsByCalcSource.Key;
                List<GenericSettingConfiguration> configs = timeConfigsByCalcSource.ToList();

                BaseLocationData locationData = profileService.GetLocationConfig(profile, calculationSource);
                IPrayerTimeService calculationSourceCalculator = prayerTimeServiceFactory.GetPrayerTimeCalculatorByCalculationSource(calculationSource);
                throwIfConfigsHaveUnsupportedTimeTypes(calculationSourceCalculator, calculationSource, configs);

                var asyncEnumerable = calculateComplexTypesForCalcSource(calculationSourceCalculator, date, locationData, configs);
                asyncEnumerables.Add(asyncEnumerable);
            }

            foreach (var result in asyncEnumerables)
            {
                await foreach (var stuff in result)
                {
                    yield return (stuff.Item1, stuff.Item2);
                }
            }
        }

        private async IAsyncEnumerable<(ETimeType, ZonedDateTime?)> calculateComplexTypesForCalcSource(
            IPrayerTimeService calculationSourceCalculator, LocalDate date, 
            BaseLocationData locationData, List<GenericSettingConfiguration> configs)
        {
            var configsByTimeType = configs.ToDictionary(x => x.TimeType);

            // CALCULATION
            foreach (var calculationPrayerTimeKVP in await calculationSourceCalculator.GetPrayerTimesAsync(date, locationData, configs).ConfigureAwait(false))
            {
                ICalculationPrayerTimes calculationPrayerTimes = calculationPrayerTimeKVP.Key;

                foreach (ETimeType timeType in calculationPrayerTimeKVP.ToList())
                {
                    GenericSettingConfiguration config = configsByTimeType[timeType];
                    ZonedDateTime calculatedZonedDateTime =
                        calculationPrayerTimes
                            .GetZonedDateTimeForTimeType(timeType)
                            .PlusMinutes(config.MinuteAdjustment);

                    yield return (timeType, calculatedZonedDateTime);
                }
            }
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
            IPrayerTimeService timeCalculator,
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