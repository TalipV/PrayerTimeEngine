using PrayerTimeEngine.Code.Common.Enums;
using PrayerTimeEngine.Code.DUMMYFOLDER;
using PrayerTimeEngine.Code.Interfaces;
using PrayerTimeEngine.Code.Services;
using PrayerTimeEngine.Common.Enums;
using PrayerTimeEngine.Domain.Models;

public class PrayerTimeCalculationService : IPrayerTimeCalculationService
{
    IPrayerTimeCalculatorFactory _prayerTimeCalculatorFactory;

    public PrayerTimeCalculationService(IPrayerTimeCalculatorFactory prayerTimeCalculatorFactory)
    {
        _prayerTimeCalculatorFactory = prayerTimeCalculatorFactory;
    }

    public PrayerTimes Execute(PrayerTimesConfiguration configuration)
    {
        DateTime dateTime = DateTime.Now;

        PrayerTimes prayerTimes = new();

        foreach (EPrayerTime prayerTime in Enum.GetValues(typeof(EPrayerTime)))
        {
            foreach (EPrayerTimeEvent timeEvent in Enum.GetValues(typeof(EPrayerTimeEvent)))
            {
                if (!configuration.Configurations.TryGetValue((prayerTime, timeEvent), out ICalculationConfiguration config) 
                    || config == null)
                {
                    prayerTimes.SetSpecificPrayerTimeDateTime(prayerTime, timeEvent, new DateTime(1, 1, 1, 0, 0, 0));
                    continue;
                }

                IPrayerTimeCalculator service = _prayerTimeCalculatorFactory.GetService(config.Source);

                if (service.GetUnsupportedPrayerTimeEvents().Any(x => x.PrayerTime == prayerTime && x.PrayerTimeEvent == timeEvent))
                {
                    throw new ArgumentException($"{service.GetType().Name}[{config.Source}] does not support {prayerTime}-{timeEvent}!");
                }

                DateTime calculatedTime = service.GetPrayerTimes(dateTime, prayerTime, timeEvent, config);
                prayerTimes.SetSpecificPrayerTimeDateTime(prayerTime, timeEvent, calculatedTime);
            }
        }

        return prayerTimes;
    }
}
