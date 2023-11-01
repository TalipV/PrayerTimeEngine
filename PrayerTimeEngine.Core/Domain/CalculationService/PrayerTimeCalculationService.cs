using MethodTimer;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain;
using PrayerTimeEngine.Core.Domain.CalculationService.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Services;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Services;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Services;
using PrayerTimeEngine.Core.Domain.Configuration.Interfaces;
using PrayerTimeEngine.Core.Domain.Configuration.Models;
using PrayerTimeEngine.Core.Domain.Model;

public class PrayerTimeCalculationService(
        IServiceProvider serviceProvider,
        IProfileService profileService,
        TimeTypeAttributeService timeTypeAttributeService
    ) : IPrayerTimeCalculationService
{
    [Time]
    public async Task<PrayerTimesBundle> ExecuteAsync(Profile profile, LocalDate date)
    {
        PrayerTimesBundle prayerTimeEntity = new();

        await handleComplexTypes(profile, date, prayerTimeEntity).ConfigureAwait(false);
        handleSimpleTypes(profile, prayerTimeEntity);

        return prayerTimeEntity;
    }

    private async Task handleComplexTypes(Profile profile, LocalDate date, PrayerTimesBundle prayerTimeEntity)
    {
        List<GenericSettingConfiguration> configurations = getActiveCalculationConfigurations(profile);

        List<Task> calculationTasks = new List<Task>();

        foreach (var calculationSourceConfigs in configurations.GroupBy(x => x.Source))
        {
            ECalculationSource calculationSource = calculationSourceConfigs.Key;
            List<GenericSettingConfiguration> configs = calculationSourceConfigs.ToList();
            calculationTasks.Add(handleComplexTypesOfSource(profile, date, prayerTimeEntity, calculationSource, configs));
        }

        await Task.WhenAll(calculationTasks).ConfigureAwait(false);
    }

    private async Task handleComplexTypesOfSource(Profile profile, LocalDate date, PrayerTimesBundle prayerTimeEntity, ECalculationSource calculationSource, List<GenericSettingConfiguration> configs)
    {
        var configsByTimeType = configs.ToDictionary(x => x.TimeType);

        IPrayerTimeService timeCalculator = GetPrayerTimeCalculatorByCalculationSource(calculationSource);
        throwIfConfigsHaveUnsupportedTimeTypes(calculationSource, configs, timeCalculator);
        BaseLocationData locationData = profileService.GetLocationConfig(profile, calculationSource);

        ILookup<ICalculationPrayerTimes, ETimeType> calculationPrayerTimes =
            await timeCalculator.GetPrayerTimesAsync(date, locationData, configs).ConfigureAwait(false);

        foreach (var calculationPrayerTimeKVP in calculationPrayerTimes)
        {
            handleSingleCalculationPrayerTimes(prayerTimeEntity, configsByTimeType, calculationPrayerTimeKVP);
        }
    }

    private static void handleSingleCalculationPrayerTimes(
        PrayerTimesBundle prayerTimeEntity, 
        Dictionary<ETimeType, GenericSettingConfiguration> configsByTimeType, 
        IGrouping<ICalculationPrayerTimes, ETimeType> calculationPrayerTimeKVP)
    {
        ICalculationPrayerTimes calculationPrayer = calculationPrayerTimeKVP.Key;
        List<ETimeType> associatedTimeTypes = calculationPrayerTimeKVP.ToList();

        foreach (var timeType in associatedTimeTypes)
        {
            GenericSettingConfiguration config = configsByTimeType[timeType];
            ZonedDateTime calculatedZonedDateTime =
                calculationPrayer.GetZonedDateTimeForTimeType(timeType)
                .PlusMinutes(config.MinuteAdjustment);

            prayerTimeEntity.SetSpecificPrayerTimeDateTime(timeType, calculatedZonedDateTime);
        }
    }

    private static void throwIfConfigsHaveUnsupportedTimeTypes(
        ECalculationSource calculationSource,
        List<GenericSettingConfiguration> configs,
        IPrayerTimeService timeCalculator)
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

    private List<GenericSettingConfiguration> getActiveCalculationConfigurations(Profile profile)
    {
        return timeTypeAttributeService
            .NonSimpleTypes
            .Select(x => profileService.GetTimeConfig(profile, x))
            .Where(config =>
                config is GenericSettingConfiguration
                {
                    Source: not ECalculationSource.None,
                    IsTimeShown: true
                })
            .ToList();
    }

    private void handleSimpleTypes(Profile profile, PrayerTimesBundle prayerTimeEntity)
    {
        if (prayerTimeEntity.Dhuhr?.Start != null
            && profileService.GetTimeConfig(profile, ETimeType.DuhaEnd) is GenericSettingConfiguration duhaConfig
            && duhaConfig.IsTimeShown)
        {
            prayerTimeEntity.SetSpecificPrayerTimeDateTime(
                ETimeType.DuhaEnd,
                prayerTimeEntity.Dhuhr.Start.Value.PlusMinutes(duhaConfig.MinuteAdjustment));
        }

        if (prayerTimeEntity.Maghrib?.Start != null
            && profileService.GetTimeConfig(profile, ETimeType.MaghribSufficientTime) is GenericSettingConfiguration maghribSufficientTimeConfig
            && maghribSufficientTimeConfig.IsTimeShown)
        {
            prayerTimeEntity.SetSpecificPrayerTimeDateTime(
                ETimeType.MaghribSufficientTime,
                prayerTimeEntity.Maghrib.Start.Value.PlusMinutes(maghribSufficientTimeConfig.MinuteAdjustment));
        }

        if ((prayerTimeEntity.Asr?.End - prayerTimeEntity.Fajr?.Start) is Duration dayDuration
            && profileService.GetTimeConfig(profile, ETimeType.DuhaQuarterOfDay) is GenericSettingConfiguration duhaQuarterOfDayConfig
            && duhaQuarterOfDayConfig.IsTimeShown)
        {
            Duration quarterOfDayDuration = dayDuration / 4.0;

            prayerTimeEntity.SetSpecificPrayerTimeDateTime(
                ETimeType.DuhaQuarterOfDay,
                prayerTimeEntity.Fajr.Start.Value + quarterOfDayDuration);
        }

        if ((prayerTimeEntity.Isha?.End - prayerTimeEntity.Maghrib?.Start) is Duration nightDuration)
        {
            if (profileService.GetTimeConfig(profile, ETimeType.IshaFirstThird) is GenericSettingConfiguration firstThirdOfNightConfig
                && firstThirdOfNightConfig.IsTimeShown)
            {
                Duration thirdOfNightDuration = nightDuration / 3.0;

                prayerTimeEntity.SetSpecificPrayerTimeDateTime(
                    ETimeType.IshaFirstThird,
                    prayerTimeEntity.Maghrib.Start.Value + thirdOfNightDuration);
            }
            if (profileService.GetTimeConfig(profile, ETimeType.IshaMidnight) is GenericSettingConfiguration halfOfNightConfig
                && halfOfNightConfig.IsTimeShown)
            {
                Duration halfOfNightDuration = nightDuration / 2.0;

                prayerTimeEntity.SetSpecificPrayerTimeDateTime(
                    ETimeType.IshaMidnight,
                    prayerTimeEntity.Maghrib.Start.Value + halfOfNightDuration);
            }
            if (profileService.GetTimeConfig(profile, ETimeType.IshaSecondThird) is GenericSettingConfiguration secondThirdOfNightConfig
                && secondThirdOfNightConfig.IsTimeShown)
            {
                Duration twoThirdsOfNightDuration = nightDuration * (2.0 / 3.0);

                prayerTimeEntity.SetSpecificPrayerTimeDateTime(
                    ETimeType.IshaSecondThird,
                    prayerTimeEntity.Maghrib.Start.Value + twoThirdsOfNightDuration);
            }
        }
    }

    public IPrayerTimeService GetPrayerTimeCalculatorByCalculationSource(ECalculationSource source)
    {
        switch (source)
        {
            case ECalculationSource.Fazilet:
                return serviceProvider.GetRequiredService<FaziletPrayerTimeCalculator>();
            case ECalculationSource.Semerkand:
                return serviceProvider.GetRequiredService<SemerkandPrayerTimeCalculator>();
            case ECalculationSource.Muwaqqit:
                return serviceProvider.GetRequiredService<MuwaqqitPrayerTimeCalculator>();
            default:
                throw new NotImplementedException($"No calculator service implemented for source: {source}");
        }
    }
}
