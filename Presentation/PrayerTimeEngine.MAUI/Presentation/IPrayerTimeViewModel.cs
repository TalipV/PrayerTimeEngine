using NodaTime;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;
using PrayerTimeEngine.Presentation.Views.PrayerTimeGraphic.VOs;

namespace PrayerTimeEngine.Presentation;

public interface IPrayerTimeViewModel
{
    Profile Profile { get; }
    IPrayerTimesDay PrayerTimesSet { get; }

    LocalDate? DisplayedDate { get; }
    bool IsDisplayingCurrentDay { get; }

    Task RefreshData(ZonedDateTime zonedDateTime, bool isCurrentDay, CancellationToken cancellationToken);
    PrayerTimeGraphicTimeVO CreatePrayerTimeGraphicTimeVO(Instant instant);
}