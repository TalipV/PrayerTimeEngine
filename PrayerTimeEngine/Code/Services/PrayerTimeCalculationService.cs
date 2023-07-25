using PrayerTimeEngine.Code.Common;
using PrayerTimeEngine.Code.Domain;
using PrayerTimeEngine.Code.Domain.ConfigStore.Models;
using PrayerTimeEngine.Code.Domain.Model;
using PrayerTimeEngine.Code.Interfaces;
using PrayerTimeEngine.Domain.Models;

public class PrayerTimeCalculationService : IPrayerTimeCalculationService
{
    IPrayerTimeCalculatorFactory _prayerTimeCalculatorFactory;

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

    public PrayerTimeCalculationService(IPrayerTimeCalculatorFactory prayerTimeCalculatorFactory)
    {
        _prayerTimeCalculatorFactory = prayerTimeCalculatorFactory;
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
                    || config.IsTimeShown)
                {
                    prayerTimeEntity.SetSpecificPrayerTimeDateTime(prayerTime, timeEvent, null);
                    continue;
                }

                IPrayerTimeCalculator timeCalculator = _prayerTimeCalculatorFactory.GetService(config.Source);

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
}
