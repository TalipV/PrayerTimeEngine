using Microsoft.Extensions.DependencyInjection;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Services;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Services;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Services;

namespace PrayerTimeEngine.Core.Domain.Calculators
{
    public class PrayerTimeCalculatorFactory(IServiceProvider serviceProvider) : IPrayerTimeCalculatorFactory
    {
        public IPrayerTimeCalculator GetPrayerTimeCalculatorByCalculationSource(ECalculationSource source)
        {
            return source switch
            {
                ECalculationSource.Fazilet => serviceProvider.GetRequiredService<FaziletPrayerTimeCalculator>(),
                ECalculationSource.Semerkand => serviceProvider.GetRequiredService<SemerkandPrayerTimeCalculator>(),
                ECalculationSource.Muwaqqit => serviceProvider.GetRequiredService<MuwaqqitPrayerTimeCalculator>(),
                ECalculationSource.None => throw new ArgumentException(message: $"'{nameof(ECalculationSource.None)}' is not a valid calculation source", paramName: nameof(source)),
                _ => throw new NotImplementedException($"No calculator service implemented for source: {source}"),
            };
        }
    }
}
