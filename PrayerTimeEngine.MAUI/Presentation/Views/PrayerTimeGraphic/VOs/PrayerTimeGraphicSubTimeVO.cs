using NodaTime;

namespace PrayerTimeEngine.Presentation.Views.PrayerTimeGraphic.VOs;

public class PrayerTimeGraphicSubTimeVO
{
    public required string Name { get; set; }
    public required Instant Start { get; set; }
    public required Instant End { get; set; }
    public ESubTimeType SubTimeType { get; set; } = ESubTimeType.FullHalf;
}
