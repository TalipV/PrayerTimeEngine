using NodaTime;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;
using PrayerTimeEngine.Presentation.Views.PrayerTimeGraphic.VOs;

namespace PrayerTimeEngine.Presentation;

public interface IPrayerTimeViewModel
{
    Profile Profile { get; }
    IPrayerTimesSet PrayerTimesSet { get; }

    Task RefreshData(ZonedDateTime zonedDateTime, CancellationToken cancellationToken);
    PrayerTimeGraphicTimeVO CreatePrayerTimeGraphicTimeVO(Instant instant);
}