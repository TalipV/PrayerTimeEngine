using PrayerTimeEngine.Code.Common;
using PrayerTimeEngine.Code.Common.Enum;
using PrayerTimeEngine.Code.Domain;
using PrayerTimeEngine.Code.Domain.CalculationService;
using PrayerTimeEngine.Code.Domain.Calculator.Fazilet.Services;
using PrayerTimeEngine.Code.Domain.Calculator.Muwaqqit.Services;
using PrayerTimeEngine.Code.Domain.Calculators;
using PrayerTimeEngine.Code.Domain.Calculators.Semerkand.Services;
using PrayerTimeEngine.Code.Domain.ConfigStore.Models;
using PrayerTimeEngine.Domain.Models;

public class PrayerTimeCalculationService : IPrayerTimeCalculationService
{
    private readonly IServiceProvider _serviceProvider;

    private readonly Dictionary<EPrayerTime, List<EPrayerTimeEvent>> CalculatorPrayerTimeEventsByPrayerTime =
        new Dictionary<EPrayerTime, List<EPrayerTimeEvent>>
        {
            [EPrayerTime.Fajr]      = new List<EPrayerTimeEvent> { EPrayerTimeEvent.Start, EPrayerTimeEvent.End, EPrayerTimeEvent.Fajr_Fadilah, EPrayerTimeEvent.Fajr_Karaha },
            [EPrayerTime.Duha]      = new List<EPrayerTimeEvent> { EPrayerTimeEvent.Start },
            [EPrayerTime.Dhuhr]     = new List<EPrayerTimeEvent> { EPrayerTimeEvent.Start, EPrayerTimeEvent.End },
            [EPrayerTime.Asr]       = new List<EPrayerTimeEvent> { EPrayerTimeEvent.Start, EPrayerTimeEvent.End, EPrayerTimeEvent.AsrMithlayn, EPrayerTimeEvent.Asr_Karaha},
            [EPrayerTime.Maghrib]   = new List<EPrayerTimeEvent> { EPrayerTimeEvent.Start, EPrayerTimeEvent.End, EPrayerTimeEvent.IshtibaqAnNujum },
            [EPrayerTime.Isha]      = new List<EPrayerTimeEvent> { EPrayerTimeEvent.Start, EPrayerTimeEvent.End },
        };

    public PrayerTimeCalculationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<PrayerTimesBundle> ExecuteAsync(Profile profile, DateTime dateTime)
    {
        PrayerTimesBundle prayerTimeEntity = new();

        foreach (EPrayerTime prayerTime in CalculatorPrayerTimeEventsByPrayerTime.Keys)
        {
            foreach (EPrayerTimeEvent timeEvent in CalculatorPrayerTimeEventsByPrayerTime[prayerTime])
            {
                if (!profile.Configurations.TryGetValue((prayerTime, timeEvent), out BaseCalculationConfiguration config)
                    || config == null
                    || config.Source == ECalculationSource.None
                    || !config.IsTimeShown)
                {
                    prayerTimeEntity.SetSpecificPrayerTimeDateTime(prayerTime, timeEvent, null);
                    continue;
                }

                IPrayerTimeCalculator timeCalculator = getPrayerTimeCalculatorByCalculationSource(config.Source);

                if (timeCalculator.GetUnsupportedPrayerTimeEvents().Any(x => x.PrayerTime == prayerTime && x.PrayerTimeEvent == timeEvent))
                {
                    throw new ArgumentException($"{timeCalculator.GetType().Name}[{config.Source}] does not support {prayerTime}-{timeEvent}!");
                }

                DateTime calculatedTime = await timeCalculator.GetPrayerTimesAsync(dateTime, prayerTime, timeEvent, config);
                calculatedTime = calculatedTime.AddMinutes(config.MinuteAdjustment);

                prayerTimeEntity.SetSpecificPrayerTimeDateTime(prayerTime, timeEvent, calculatedTime);
            }
        }

        if (prayerTimeEntity.Dhuhr?.Start != null 
            && profile.Configurations.TryGetValue((EPrayerTime.Duha, EPrayerTimeEvent.End), out BaseCalculationConfiguration duhaConfig)
            && duhaConfig != null)
        {
            prayerTimeEntity.SetSpecificPrayerTimeDateTime(
                EPrayerTime.Duha,
                EPrayerTimeEvent.End,
                prayerTimeEntity.Dhuhr.Start.Value.AddMinutes(duhaConfig.MinuteAdjustment));
        }

        return prayerTimeEntity;
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
