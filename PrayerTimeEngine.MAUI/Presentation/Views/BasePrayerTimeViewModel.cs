using NodaTime;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;
using PrayerTimeEngine.Presentation.Views.PrayerTimeGraphic.VOs;
using PropertyChanged;

namespace PrayerTimeEngine.Presentation.Views;

[AddINotifyPropertyChangedInterface]
public abstract class BasePrayerTimeViewModel<ProfileType, PrayerTimeSetType> (
        ProfileType profile
    ) : IPrayerTimeViewModel
    where ProfileType : Profile
    where PrayerTimeSetType : IPrayerTimesSet
{
    protected ProfileType ProfileActual { get; } = profile;
    public IPrayerTimesSet PrayerTimesSet { get; private set; }
    public Profile Profile => ProfileActual;

    public abstract Task<PrayerTimeSetType> GetPrayerTimesSet(ZonedDateTime zonedDateTime, CancellationToken cancellationToken);

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

        var currentPrayerTime = PrayerTimesSet.AllPrayerTimes
                .FirstOrDefault(x => x.Times.Start?.ToInstant() <= instant && instant <= x.Times.End?.ToInstant());

        if (currentPrayerTime == default)
            currentPrayerTime = PrayerTimesSet.AllPrayerTimes
                    .OrderBy(x => x.Times.Start?.ToInstant())
                    .FirstOrDefault(x => x.Times.Start?.ToInstant() > instant);
        
        if (currentPrayerTime == default || currentPrayerTime.Times?.Start is null || currentPrayerTime.Times?.End is null)
            return null;

        return new PrayerTimeGraphicTimeVO
        {
            Title = currentPrayerTime.PrayerType.ToString(),
            Start = currentPrayerTime.Times.Start.Value,
            End = currentPrayerTime.Times.End.Value,
            SubTimeVOs = CreatePrayerTimeGraphicSubTimeVO(currentPrayerTime.Times)
        };
    }

    protected virtual List<PrayerTimeGraphicSubTimeVO> CreatePrayerTimeGraphicSubTimeVO(GenericPrayerTime prayerTime) => [];
}