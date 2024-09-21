using NodaTime;

namespace PrayerTimeEngine.Presentation.Views.PrayerTimeGraphic.VOs;

public  class PrayerTimeGraphicTimeVO
{
    public required string Title { get; set; }
    public required ZonedDateTime Start { get; set; }
    public required ZonedDateTime End { get; set; }
    public List<PrayerTimeGraphicSubTimeVO> SubTimeVOs { get; set; } = [];
}
