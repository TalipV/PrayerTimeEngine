using PrayerTimeEngine.Code.Common.Enum;
using PrayerTimeEngine.Code.Domain;
using PrayerTimeEngine.Code.Domain.CalculationService.Interfaces;
using PrayerTimeEngine.Code.Domain.Calculator.Fazilet.Services;
using PrayerTimeEngine.Code.Domain.Calculator.Muwaqqit.Services;
using PrayerTimeEngine.Code.Domain.Calculators;
using PrayerTimeEngine.Code.Domain.Calculators.Semerkand.Services;
using PrayerTimeEngine.Code.Domain.ConfigStore.Models;
using PrayerTimeEngine.Domain.Models;

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
        foreach (ETimeType timeType in _timeTypeAttributeService.NonSimpleTypes)
        {
            if (profile.GetConfiguration(timeType) is not BaseCalculationConfiguration config
                || config.Source == ECalculationSource.None
                || !config.IsTimeShown)
            {
                prayerTimeEntity.SetSpecificPrayerTimeDateTime(timeType, null);
                continue;
            }

            IPrayerTimeCalculator timeCalculator = getPrayerTimeCalculatorByCalculationSource(config.Source);

            if (timeCalculator.GetUnsupportedCalculationTimeTypes().Contains(timeType))
            {
                throw new ArgumentException($"{timeCalculator.GetType().Name}[{config.Source}] does not support {timeType}!");
            }

            ICalculationPrayerTimes calculationPrayerTimes = await timeCalculator.GetPrayerTimesAsync(dateTime, timeType, config);
            DateTime calculatedTime = 
                calculationPrayerTimes.GetDateTimeForTimeType(timeType)
                .AddMinutes(config.MinuteAdjustment);

            prayerTimeEntity.SetSpecificPrayerTimeDateTime(timeType, calculatedTime);
        }
    }

    private void handleSimpleTypes(Profile profile, PrayerTimesBundle prayerTimeEntity)
    {
        foreach (ETimeType timeType in _timeTypeAttributeService.SimpleTypes)
        {
            switch (timeType)
            {
                case ETimeType.DuhaEnd:
                    if (prayerTimeEntity.Dhuhr?.Start != null
                        && profile.Configurations.TryGetValue(ETimeType.DuhaEnd, out BaseCalculationConfiguration duhaConfig)
                        && duhaConfig != null)
                    {
                        prayerTimeEntity.SetSpecificPrayerTimeDateTime(
                            ETimeType.DuhaEnd,
                            prayerTimeEntity.Dhuhr.Start.Value.AddMinutes(duhaConfig.MinuteAdjustment));
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
