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

    /// <summary>The day whose times are shown (or were last attempted to be shown); <c>null</c> before the first load.</summary>
    public LocalDate? DisplayedDate { get; private set; }

    /// <summary>
    /// Whether the shown day is the current day. Only then the running prayer is highlighted and shown in the graphic.
    /// </summary>
    public bool IsDisplayingCurrentDay { get; private set; }

    public abstract Task<TPrayerTimeSetType> GetPrayerTimesSet(ZonedDateTime zonedDateTime, CancellationToken cancellationToken);

    public virtual async Task RefreshData(ZonedDateTime zonedDateTime, bool isCurrentDay, CancellationToken cancellationToken)
    {
        LocalDate date = zonedDateTime.Date;

        try
        {
            TPrayerTimeSetType prayerTimesSet = await GetPrayerTimesSet(zonedDateTime, cancellationToken);

            // set before the times, so that the bindings reacting to the new times already see the new day
            IsDisplayingCurrentDay = isCurrentDay;
            DisplayedDate = date;
            PrayerTimesSet = prayerTimesSet;
        }
        catch (Exception) when (!cancellationToken.IsCancellationRequested && DisplayedDate != date)
        {
            // the times of the previously shown day must not remain visible under the new day
            IsDisplayingCurrentDay = isCurrentDay;
            DisplayedDate = date;
            PrayerTimesSet = null;
            throw;
        }
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