using NodaTime;
using PrayerTimeEngine.Core.Common.Enum;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;

public class DynamicPrayerTimeCalculationErrorVO
{
    public required EDynamicPrayerTimeProviderType DynamicPrayerTimeProviderType { get; init; }
    public required LocalDate Date { get; init; }
    public required List<ETimeType> TimeTypes { get; init; } = [];
    public required Exception Exception { get; init; }
}
