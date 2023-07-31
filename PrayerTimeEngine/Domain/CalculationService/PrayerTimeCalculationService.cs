using Microsoft.Extensions.DependencyInjection;
using PrayerTimeEngine.Common.Enum;
using PrayerTimeEngine.Domain;
using PrayerTimeEngine.Domain.CalculationService.Interfaces;
using PrayerTimeEngine.Domain.Calculators.Fazilet.Services;
using PrayerTimeEngine.Domain.Calculators.Muwaqqit.Services;
using PrayerTimeEngine.Domain.Calculators.Semerkand;
using PrayerTimeEngine.Domain.Calculators.Semerkand.Services;
using PrayerTimeEngine.Domain.ConfigStore.Models;
using PrayerTimeEngine.Domain.Model;

public class PrayerTimeCalculationService : IPrayerTimeCalculationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeTypeAttributeService _timeTypeAttributeService;

    public PrayerTimeCalculationService(
        IServiceProvider serviceProvider, 
        TimeTypeAttributeService timeTypeAttributeService)
    {
        _serviceProvider = serviceProvider;
        _timeTypeAttributeService = timeTypeAttributeService;
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

            IPrayerTimeCalculator timeCalculator = getPrayerTimeCalculatorByCalculationSource(calculationSource);
            throwIfConfigsHaveUnsupportedTimeTypes(calculationSource, configs, timeCalculator);

            ILookup<ICalculationPrayerTimes, ETimeType> calculationPrayerTimes =
                await timeCalculator.GetPrayerTimesAsync(dateTime, configs);

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

    private static void throwIfConfigsHaveUnsupportedTimeTypes(ECalculationSource calculationSource, List<GenericSettingConfiguration> configs, IPrayerTimeCalculator timeCalculator)
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
            .Select(x => profile.GetConfiguration(x))
            .Where(config =>
            {
                if (config == null 
                    || config.Source == ECalculationSource.None 
                    || !config.IsTimeShown)
                {
                    return false;
                }
                return true;
            })
            .ToList();
    }

    private void handleSimpleTypes(Profile profile, PrayerTimesBundle prayerTimeEntity)
    {
        foreach (ETimeType timeType in _timeTypeAttributeService.ConfigurableSimpleTypes)
        {
            switch (timeType)
            {
                case ETimeType.DuhaEnd:
                    if (prayerTimeEntity.Dhuhr?.Start != null
                        && profile.GetConfiguration(ETimeType.DuhaEnd) is GenericSettingConfiguration duhaConfig
                        && duhaConfig.IsTimeShown)
                    {
                        prayerTimeEntity.SetSpecificPrayerTimeDateTime(
                            ETimeType.DuhaEnd,
                            prayerTimeEntity.Dhuhr.Start.Value.AddMinutes(duhaConfig.MinuteAdjustment));
                    }
                    break;
                case ETimeType.MaghribSufficientTime:
                    if (prayerTimeEntity.Maghrib?.Start != null
                        && profile.GetConfiguration(ETimeType.MaghribSufficientTime) is GenericSettingConfiguration maghribSufficientTimeConfig
                        && maghribSufficientTimeConfig.IsTimeShown)
                    {
                        prayerTimeEntity.SetSpecificPrayerTimeDateTime(
                            ETimeType.MaghribSufficientTime,
                            prayerTimeEntity.Maghrib.Start.Value.AddMinutes(maghribSufficientTimeConfig.MinuteAdjustment));
                    }
                    break;
                default:
                    break;
            }
        }
    }

    private IPrayerTimeCalculator getPrayerTimeCalculatorByCalculationSource(ECalculationSource source)
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
