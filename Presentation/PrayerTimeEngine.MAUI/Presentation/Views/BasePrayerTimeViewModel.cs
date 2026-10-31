using NodaTime;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;
using PrayerTimeEngine.Presentation.Views.PrayerTimeGraphic.VOs;
using PropertyChanged;
using PrayerTimeEngine.Presentation.Localization;

namespace PrayerTimeEngine.Presentation.Views;

[AddINotifyPropertyChangedInterface]
public abstract class BasePrayerTimeViewModel<TProfileType, TPrayerTimeSetType>(
        TProfileType profile
    ) : IPrayerTimeViewModel
    where TProfileType : Profile
    where TPrayerTimeSetType : IPrayerTimesDay
{
    protected TProfileType ProfileActual { get; } = profile;
    public IPrayerTimesDay PrayerTimesSet { get; private set; }
    public Profile Profile => ProfileActual;

    public abstract Task<TPrayerTimeSetType> GetPrayerTimesSet(ZonedDateTime zonedDateTime, CancellationToken cancellationToken);

    public virtual async Task RefreshData(ZonedDateTime zonedDateTime, CancellationToken cancellationToken)
    {
        PrayerTimesSet = await GetPrayerTimesSet(zonedDateTime, cancellationToken);
    }

    public PrayerTimeGraphicTimeVO CreatePrayerTimeGraphicTimeVO(Instant instant)
    {
        if (PrayerTimesSet is null)
        {
            return null;
        }

        (ETimeSection Section, GenericPrayerTime Times) currentPrayerTime = PrayerTimesSet.GetCurrentOrNextPrayerTime(instant);

        if (currentPrayerTime == default || currentPrayerTime.Times?.Start is null || currentPrayerTime.Times?.End is null)
        {
            return null;
        }

        return new PrayerTimeGraphicTimeVO
        {
            Title = LocalizedNames.GetSectionName(currentPrayerTime.Section),
            Start = currentPrayerTime.Times.Start.Value,
            End = currentPrayerTime.Times.End.Value,
            SubTimeVOs = CreatePrayerTimeGraphicSubTimeVO(currentPrayerTime.Times)
        };
    }

    protected virtual List<PrayerTimeGraphicSubTimeVO> CreatePrayerTimeGraphicSubTimeVO(GenericPrayerTime prayerTime) => [];
}