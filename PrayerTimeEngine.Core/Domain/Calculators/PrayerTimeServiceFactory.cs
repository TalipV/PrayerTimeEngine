using Microsoft.Extensions.DependencyInjection;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Services;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Services;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Services;

namespace PrayerTimeEngine.Core.Domain.Calculators
{
    public class PrayerTimeServiceFactory(IServiceProvider serviceProvider) : IPrayerTimeServiceFactory
    {
        public IPrayerTimeService GetPrayerTimeCalculatorByCalculationSource(ECalculationSource source)
        {
            return source switch
            {
                ECalculationSource.Fazilet => serviceProvider.GetRequiredService<FaziletPrayerTimeCalculator>(),
                ECalculationSource.Semerkand => serviceProvider.GetRequiredService<SemerkandPrayerTimeCalculator>(),
                ECalculationSource.Muwaqqit => serviceProvider.GetRequiredService<MuwaqqitPrayerTimeCalculator>(),
                _ => throw new NotImplementedException($"No calculator service implemented for source: {source}"),
            };
        }
    }
}
