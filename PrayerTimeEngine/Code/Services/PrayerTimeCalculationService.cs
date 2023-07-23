using PrayerTimeEngine.Code.Common;
using PrayerTimeEngine.Code.Common.Enums;
using PrayerTimeEngine.Code.Domain;
using PrayerTimeEngine.Code.Domain.ConfigStore.Models;
using PrayerTimeEngine.Code.Interfaces;
using PrayerTimeEngine.Domain.Models;

public class PrayerTimeCalculationService : IPrayerTimeCalculationService
{
    IPrayerTimeCalculatorFactory _prayerTimeCalculatorFactory;

    private readonly Dictionary<EPrayerTime, List<EPrayerTimeEvent>> CalculatorPrayerTimeEventsByPrayerTime =
        new Dictionary<EPrayerTime, List<EPrayerTimeEvent>>
        {
            [EPrayerTime.Fajr]      = new List<EPrayerTimeEvent> { EPrayerTimeEvent.Start, EPrayerTimeEvent.End, EPrayerTimeEvent.FajrGhalasEnd, EPrayerTimeEvent.FajrSunriseRedness },
            [EPrayerTime.Duha]      = new List<EPrayerTimeEvent> { EPrayerTimeEvent.Start, EPrayerTimeEvent.End },
            [EPrayerTime.Dhuhr]     = new List<EPrayerTimeEvent> { EPrayerTimeEvent.Start, EPrayerTimeEvent.End },
            [EPrayerTime.Asr]       = new List<EPrayerTimeEvent> { EPrayerTimeEvent.Start, EPrayerTimeEvent.End, EPrayerTimeEvent.AsrMithlayn, EPrayerTimeEvent.AsrKaraha},
            [EPrayerTime.Maghrib]   = new List<EPrayerTimeEvent> { EPrayerTimeEvent.Start, EPrayerTimeEvent.End, EPrayerTimeEvent.MaghribIshtibaq },
            [EPrayerTime.Isha]      = new List<EPrayerTimeEvent> { EPrayerTimeEvent.Start, EPrayerTimeEvent.End },
        };

    public PrayerTimeCalculationService(IPrayerTimeCalculatorFactory prayerTimeCalculatorFactory)
    {
        _prayerTimeCalculatorFactory = prayerTimeCalculatorFactory;
    }

    public async Task<PrayerTimesBundle> ExecuteAsync(Profile profile, DateTime dateTime)
    {
        PrayerTimesBundle prayerTimeEntity = new();

        List<(EPrayerTime, EPrayerTimeEvent, GeneralMinuteAdjustmentConfguration)> generalConfigForAfter =
            new List<(EPrayerTime, EPrayerTimeEvent, GeneralMinuteAdjustmentConfguration)>();

        foreach (EPrayerTime prayerTime in CalculatorPrayerTimeEventsByPrayerTime.Keys)
        {
            foreach (EPrayerTimeEvent timeEvent in CalculatorPrayerTimeEventsByPrayerTime[prayerTime])
            {
                if (!profile.Configurations.TryGetValue((prayerTime, timeEvent), out BaseCalculationConfiguration config)
                    || config == null)
                {
                    prayerTimeEntity.SetSpecificPrayerTimeDateTime(prayerTime, timeEvent, null);
                    continue;
                }

                if (config is GeneralMinuteAdjustmentConfguration generalConfig)
                {
                    generalConfigForAfter.Add((prayerTime, timeEvent, generalConfig));
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

        foreach (var item in generalConfigForAfter)
        {
            EPrayerTime prayerTime = item.Item1;
            EPrayerTimeEvent timeEvent = item.Item2;
            GeneralMinuteAdjustmentConfguration generalConfig = item.Item3;

            if (prayerTime == EPrayerTime.Duha && timeEvent == EPrayerTimeEvent.End)
            {
                if (prayerTimeEntity.Dhuhr?.Start == null)
                {
                    continue;
                }

                prayerTimeEntity.SetSpecificPrayerTimeDateTime(
                    prayerTime, 
                    timeEvent, 
                    prayerTimeEntity.Dhuhr.Start.Value.AddMinutes(generalConfig.MinuteAdjustment));
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        return prayerTimeEntity;
    }
}
