namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;

public class CalculatePrayerTimesResultVO
{
    public DynamicPrayerTimesDaySet DynamicPrayerTimesDaySet { get; set; }
    public List<DynamicPrayerTimeCalculationErrorVO> CalculationErrors { get; set; } = [];
}
