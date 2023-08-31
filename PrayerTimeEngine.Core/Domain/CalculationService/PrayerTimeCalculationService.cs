using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NodaTime;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain;
using PrayerTimeEngine.Core.Domain.CalculationService.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Services;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Services;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Services;
using PrayerTimeEngine.Core.Domain.Configuration.Models;
using PrayerTimeEngine.Core.Domain.Model;
using System.Diagnostics;

public class PrayerTimeCalculationService : IPrayerTimeCalculationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeTypeAttributeService _timeTypeAttributeService;
    private readonly ILogger<PrayerTimeCalculationService> _logger;

    public PrayerTimeCalculationService(
        IServiceProvider serviceProvider, 
        TimeTypeAttributeService timeTypeAttributeService,
        ILogger<PrayerTimeCalculationService> logger)
    {
        _serviceProvider = serviceProvider;
        _timeTypeAttributeService = timeTypeAttributeService;
        _logger = logger;
    }

    public async Task<PrayerTimesBundle> ExecuteAsync(Profile profile, LocalDate date)
    {
        PrayerTimesBundle prayerTimeEntity = new();

        await handleComplexTypes(profile, date, prayerTimeEntity);
        handleSimpleTypes(profile, prayerTimeEntity);

        return prayerTimeEntity;
    }

    private async Task handleComplexTypes(Profile profile, LocalDate date, PrayerTimesBundle prayerTimeEntity)
    {
        List<GenericSettingConfiguration> configurations = getActiveCalculationConfigurations(profile);

        foreach (var calculationSourceConfigs in configurations.GroupBy(x => x.Source))
        {
            ECalculationSource calculationSource = calculationSourceConfigs.Key;
            List<GenericSettingConfiguration> configs = calculationSourceConfigs.ToList();
            var configsByTimeType = configs.ToDictionary(x => x.TimeType);
            
            IPrayerTimeService timeCalculator = GetPrayerTimeCalculatorByCalculationSource(calculationSource);
            throwIfConfigsHaveUnsupportedTimeTypes(calculationSource, configs, timeCalculator);
            BaseLocationData locationData = profile.LocationDataByCalculationSource[calculationSource];

            Stopwatch stopwatch = Stopwatch.StartNew();

            ILookup<ICalculationPrayerTimes, ETimeType> calculationPrayerTimes =
                await timeCalculator.GetPrayerTimesAsync(date, locationData, configs);

            _logger.LogDebug(
                "{CalculationCount} times took {DurationMS} ms for {TimeCalculatorBane}", 
                configs.Count, stopwatch.ElapsedMilliseconds, timeCalculator.GetType().Name);

            foreach (var calculationPrayerTimeKVP in calculationPrayerTimes)
            {
                handleSingleCalculationPrayerTimes(prayerTimeEntity, configsByTimeType, calculationPrayerTimeKVP);
            }
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
        return _timeTypeAttributeService
            .NonSimpleTypes
            .Select(profile.GetConfiguration)
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
            && profile.GetConfiguration(ETimeType.DuhaEnd) is GenericSettingConfiguration duhaConfig
            && duhaConfig.IsTimeShown)
        {
            prayerTimeEntity.SetSpecificPrayerTimeDateTime(
                ETimeType.DuhaEnd,
                prayerTimeEntity.Dhuhr.Start.Value.PlusMinutes(duhaConfig.MinuteAdjustment));
        }

        if (prayerTimeEntity.Maghrib?.Start != null
            && profile.GetConfiguration(ETimeType.MaghribSufficientTime) is GenericSettingConfiguration maghribSufficientTimeConfig
            && maghribSufficientTimeConfig.IsTimeShown)
        {
            prayerTimeEntity.SetSpecificPrayerTimeDateTime(
                ETimeType.MaghribSufficientTime,
                prayerTimeEntity.Maghrib.Start.Value.PlusMinutes(maghribSufficientTimeConfig.MinuteAdjustment));
        }

        if ((prayerTimeEntity.Asr?.End - prayerTimeEntity.Fajr?.Start) is Duration dayDuration
            && profile.GetConfiguration(ETimeType.DuhaQuarterOfDay) is GenericSettingConfiguration duhaQuarterOfDayConfig
            && duhaQuarterOfDayConfig.IsTimeShown)
        {
            Duration quarterOfDayDuration = dayDuration / 4.0;

            prayerTimeEntity.SetSpecificPrayerTimeDateTime(
                ETimeType.DuhaQuarterOfDay,
                prayerTimeEntity.Fajr.Start.Value + quarterOfDayDuration);
        }

        if ((prayerTimeEntity.Isha?.End - prayerTimeEntity.Maghrib?.Start) is Duration nightDuration)
        {
            if (profile.GetConfiguration(ETimeType.IshaFirstThird) is GenericSettingConfiguration firstThirdOfNightConfig
                && firstThirdOfNightConfig.IsTimeShown)
            {
                Duration thirdOfNightDuration = nightDuration / 3.0;

                prayerTimeEntity.SetSpecificPrayerTimeDateTime(
                    ETimeType.IshaFirstThird,
                    prayerTimeEntity.Maghrib.Start.Value + thirdOfNightDuration);
            }
            if (profile.GetConfiguration(ETimeType.IshaMidnight) is GenericSettingConfiguration halfOfNightConfig
                && halfOfNightConfig.IsTimeShown)
            {
                Duration halfOfNightDuration = nightDuration / 2.0;

                prayerTimeEntity.SetSpecificPrayerTimeDateTime(
                    ETimeType.IshaMidnight,
                    prayerTimeEntity.Maghrib.Start.Value + halfOfNightDuration);
            }
            if (profile.GetConfiguration(ETimeType.IshaSecondThird) is GenericSettingConfiguration secondThirdOfNightConfig
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
                return _serviceProvider.GetRequiredService<FaziletPrayerTimeCalculator>();
            case ECalculationSource.Semerkand:
                return _serviceProvider.GetRequiredService<SemerkandPrayerTimeCalculator>();
            case ECalculationSource.Muwaqqit:
                return _serviceProvider.GetRequiredService<MuwaqqitPrayerTimeCalculator>();
            default:
                throw new NotImplementedException($"No calculator service implemented for source: {source}");
        }
    }
}
