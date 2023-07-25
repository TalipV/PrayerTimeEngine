using PrayerTimeEngine.Code.Domain.Fazilet.Services;
using PrayerTimeEngine.Code.Domain.Model;
using PrayerTimeEngine.Code.Domain.Muwaqqit.Services;
using PrayerTimeEngine.Code.Interfaces;

public class PrayerTimeCalculatorFactory : IPrayerTimeCalculatorFactory
{
    private readonly IServiceProvider _serviceProvider;

    public PrayerTimeCalculatorFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IPrayerTimeCalculator GetService(ECalculationSource source)
    {
        switch (source)
        {
            case ECalculationSource.Fazilet:
                return _serviceProvider.GetRequiredService<FaziletPrayerTimeCalculator>();
            case ECalculationSource.Muwaqqit:
                return _serviceProvider.GetRequiredService<MuwaqqitPrayerTimeCalculator>();
            default:
                throw new NotImplementedException($"No calculator service implemented for source: {source}");
        }
    }
}
