using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using PrayerTimeEngine.Core.Common.Enum;
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
        IProfileService profileService
    ) : IPrayerTimeCalculationService
{
    public async Task<PrayerTimesBundle> ExecuteAsync(Profile profile, LocalDate date)
    {
        PrayerTimesBundle prayerTimeEntity = new();

        await handleComplexTypes(profile, date, prayerTimeEntity).ConfigureAwait(false);
        handleSimpleTypes(profile, prayerTimeEntity);

        return prayerTimeEntity;
    }

    private async Task handleComplexTypes(Profile profile, LocalDate date, PrayerTimesBundle prayerTimeEntity)
    {
        List<GenericSettingConfiguration> configurations = profileService.GetActiveComplexTimeConfigs(profile);

        List<Task> calculationTasks = [];

        foreach (var calculationSourceConfigs in configurations.GroupBy(x => x.Source))
        {
            ECalculationSource calculationSource = calculationSourceConfigs.Key;
            List<GenericSettingConfiguration> configs = calculationSourceConfigs.ToList();

            Task task = handleComplexTypesOfCalculationSource(profile, date, prayerTimeEntity, calculationSource, configs);
            calculationTasks.Add(task);
        }

        await Task.WhenAll(calculationTasks).ConfigureAwait(false);
    }

    private async Task handleComplexTypesOfCalculationSource(Profile profile, LocalDate date, PrayerTimesBundle prayerTimeEntity, ECalculationSource calculationSource, List<GenericSettingConfiguration> configs)
    {
        IPrayerTimeService calculationSourceCalculator = 
            GetPrayerTimeCalculatorByCalculationSource(calculationSource);
        throwIfConfigsHaveUnsupportedTimeTypes(calculationSource, configs, calculationSourceCalculator);

        var configsByTimeType = configs.ToDictionary(x => x.TimeType);
        BaseLocationData locationData = profileService.GetLocationConfig(profile, calculationSource);

        // CALCULATION
        ILookup<ICalculationPrayerTimes, ETimeType> calculationPrayerTimes =
            await calculationSourceCalculator.GetPrayerTimesAsync(date, locationData, configs).ConfigureAwait(false);

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
        ICalculationPrayerTimes calculationPrayerTimes = calculationPrayerTimeKVP.Key;
        List<ETimeType> associatedTimeTypes = calculationPrayerTimeKVP.ToList();

        foreach (var timeType in associatedTimeTypes)
        {
            GenericSettingConfiguration config = configsByTimeType[timeType];
            ZonedDateTime calculatedZonedDateTime =
                calculationPrayerTimes
                    .GetZonedDateTimeForTimeType(timeType)
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

        if ((prayerTimeEntity.Asr?.End - prayerTimeEntity.Fajr?.Start) is Duration dayDuration)
        {
            Duration quarterOfDayDuration = dayDuration / 4.0;

            prayerTimeEntity.SetSpecificPrayerTimeDateTime(
                ETimeType.DuhaQuarterOfDay,
                prayerTimeEntity.Fajr.Start.Value + quarterOfDayDuration);
        }

        if ((prayerTimeEntity.Isha?.End - prayerTimeEntity.Maghrib?.Start) is Duration nightDuration)
        {
            Duration thirdOfNightDuration = nightDuration / 3.0;

            prayerTimeEntity.SetSpecificPrayerTimeDateTime(
                ETimeType.IshaFirstThird,
                prayerTimeEntity.Maghrib.Start.Value + thirdOfNightDuration);

            Duration halfOfNightDuration = nightDuration / 2.0;

            prayerTimeEntity.SetSpecificPrayerTimeDateTime(
                ETimeType.IshaMidnight,
                prayerTimeEntity.Maghrib.Start.Value + halfOfNightDuration);

            Duration twoThirdsOfNightDuration = nightDuration * (2.0 / 3.0);

            prayerTimeEntity.SetSpecificPrayerTimeDateTime(
                ETimeType.IshaSecondThird,
                prayerTimeEntity.Maghrib.Start.Value + twoThirdsOfNightDuration);
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
