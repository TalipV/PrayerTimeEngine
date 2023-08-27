using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PrayerTimeEngine.Common.Enum;
using PrayerTimeEngine.Domain;
using PrayerTimeEngine.Domain.CalculationService.Interfaces;
using PrayerTimeEngine.Domain.Calculators.Fazilet.Services;
using PrayerTimeEngine.Domain.Calculators.Muwaqqit.Services;
using PrayerTimeEngine.Domain.Calculators.Semerkand;
using PrayerTimeEngine.Domain.Calculators.Semerkand.Services;
using PrayerTimeEngine.Domain.ConfigStore.Models;
using PrayerTimeEngine.Domain.Model;
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

    public async Task<PrayerTimesBundle> ExecuteAsync(Profile profile, DateTime dateTime)
    {
        PrayerTimesBundle prayerTimeEntity = new();

        await handleComplexTypes(profile, dateTime, prayerTimeEntity);
        handleSimpleTypes(profile, prayerTimeEntity);

        return prayerTimeEntity;
    }

    private async Task handleComplexTypes(Profile profile, DateTime dateTime, PrayerTimesBundle prayerTimeEntity)
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
                await timeCalculator.GetPrayerTimesAsync(dateTime, locationData, configs);

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
            DateTime calculatedTime =
                calculationPrayer.GetDateTimeForTimeType(timeType)
                .AddMinutes(config.MinuteAdjustment);

            prayerTimeEntity.SetSpecificPrayerTimeDateTime(timeType, calculatedTime);
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
                prayerTimeEntity.Dhuhr.Start.Value.AddMinutes(duhaConfig.MinuteAdjustment));
        }

        if (prayerTimeEntity.Maghrib?.Start != null
            && profile.GetConfiguration(ETimeType.MaghribSufficientTime) is GenericSettingConfiguration maghribSufficientTimeConfig
            && maghribSufficientTimeConfig.IsTimeShown)
        {
            prayerTimeEntity.SetSpecificPrayerTimeDateTime(
                ETimeType.MaghribSufficientTime,
                prayerTimeEntity.Maghrib.Start.Value.AddMinutes(maghribSufficientTimeConfig.MinuteAdjustment));
        }

        if ((prayerTimeEntity.Asr?.End - prayerTimeEntity.Fajr?.Start) is TimeSpan dayDuration
            && profile.GetConfiguration(ETimeType.DuhaQuarterOfDay) is GenericSettingConfiguration duhaQuarterOfDayConfig
            && duhaQuarterOfDayConfig.IsTimeShown)
        {
            TimeSpan quarterOfDayDuration = TimeSpan.FromMilliseconds(dayDuration.TotalMilliseconds * (1.0 / 4.0));

            prayerTimeEntity.SetSpecificPrayerTimeDateTime(
                ETimeType.DuhaQuarterOfDay,
                prayerTimeEntity.Fajr.Start.Value.Add(quarterOfDayDuration));
        }

        if ((prayerTimeEntity.Isha?.End - prayerTimeEntity.Maghrib?.Start) is TimeSpan nightDuration)
        {
            if (profile.GetConfiguration(ETimeType.IshaFirstThird) is GenericSettingConfiguration firstThirdOfNightConfig
                && firstThirdOfNightConfig.IsTimeShown)
            {
                TimeSpan thirdOfNightDuration = TimeSpan.FromMilliseconds(nightDuration.TotalMilliseconds * (1.0 / 3.0));

                prayerTimeEntity.SetSpecificPrayerTimeDateTime(
                    ETimeType.IshaFirstThird,
                    prayerTimeEntity.Maghrib.Start.Value.Add(thirdOfNightDuration));
            }
            if (profile.GetConfiguration(ETimeType.IshaMidnight) is GenericSettingConfiguration halfOfNightConfig
                && halfOfNightConfig.IsTimeShown)
            {
                TimeSpan halfOfNightDuration = TimeSpan.FromMilliseconds(nightDuration.TotalMilliseconds * (1.0 / 2.0));

                prayerTimeEntity.SetSpecificPrayerTimeDateTime(
                    ETimeType.IshaMidnight,
                    prayerTimeEntity.Maghrib.Start.Value.Add(halfOfNightDuration));
            }
            if (profile.GetConfiguration(ETimeType.IshaSecondThird) is GenericSettingConfiguration secondThirdOfNightConfig
                && secondThirdOfNightConfig.IsTimeShown)
            {
                TimeSpan twoThirdsOfNightDuration = TimeSpan.FromMilliseconds(nightDuration.TotalMilliseconds * (2.0 / 3.0));

                prayerTimeEntity.SetSpecificPrayerTimeDateTime(
                    ETimeType.IshaSecondThird,
                    prayerTimeEntity.Maghrib.Start.Value.Add(twoThirdsOfNightDuration));
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
