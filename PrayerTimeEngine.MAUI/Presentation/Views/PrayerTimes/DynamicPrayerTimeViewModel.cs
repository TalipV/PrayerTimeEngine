using NodaTime;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Management;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.Models.PrayerTimes;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;
using PrayerTimeEngine.Presentation.Views.PrayerTimeGraphic.VOs;
using PropertyChanged;

namespace PrayerTimeEngine.Presentation.Views.PrayerTimes;

[AddINotifyPropertyChangedInterface]
public class DynamicPrayerTimeViewModel(
        IDynamicPrayerTimeProviderManager dynamicPrayerTimeProviderManager,
        IDispatcher dispatcher,
        IProfileService profileService,
        DynamicProfile profile
    ) : BasePrayerTimeViewModel<DynamicProfile, DynamicPrayerTimesDaySet>(profile)
{
    public bool ShowFajrGhalas { get; set; }
    public bool ShowFajrRedness { get; set; }
    public bool ShowMithlayn { get; set; }
    public bool ShowKaraha { get; set; }
    public bool ShowIshtibaq { get; set; }
    public bool ShowMaghribSufficientTime { get; set; }

    public override async Task<DynamicPrayerTimesDaySet> GetPrayerTimesSet(ZonedDateTime zonedDateTime, CancellationToken cancellationToken)
    {
        return (await dynamicPrayerTimeProviderManager.CalculatePrayerTimesAsync(
            ProfileActual.ID,
            zonedDateTime,
            cancellationToken)).DynamicPrayerTimesDaySet;
    }

    public override async Task RefreshData(ZonedDateTime zonedDateTime, CancellationToken cancellationToken)
    {
        await dispatcher.DispatchAsync(showHideSpecificTimes);
        await base.RefreshData(zonedDateTime, cancellationToken);
    }

    private void showHideSpecificTimes()
    {
        ShowFajrGhalas = isCalculationShown(ETimeType.FajrGhalas);
        ShowFajrRedness = isCalculationShown(ETimeType.FajrKaraha);

        ShowMithlayn = isCalculationShown(ETimeType.AsrMithlayn);
        ShowKaraha = isCalculationShown(ETimeType.AsrKaraha);

        ShowMaghribSufficientTime = isCalculationShown(ETimeType.MaghribSufficientTime);
        ShowIshtibaq = isCalculationShown(ETimeType.MaghribIshtibaq);
    }

    private bool isCalculationShown(ETimeType timeData)
    {
        if (ProfileActual is null)
            return false;

        return profileService.GetTimeConfig(ProfileActual, timeData)?.IsTimeShown == true;
    }

    protected override List<PrayerTimeGraphicSubTimeVO> CreatePrayerTimeGraphicSubTimeVO(GenericPrayerTime prayerTime)
    {
        if (prayerTime is FajrPrayerTime fajrPrayerTime)
        {
            if (fajrPrayerTime.Ghalas is null || fajrPrayerTime.Karaha is null)
            {
                return [];
            }

            return [
                new PrayerTimeGraphicSubTimeVO
                {
                    Name = "Ikhtiyar",
                    Start = prayerTime.Start.Value.ToInstant(),
                    End = fajrPrayerTime.Ghalas.Value.ToInstant()
                },
                new PrayerTimeGraphicSubTimeVO
                {
                    Name = "Normal",
                    Start = fajrPrayerTime.Ghalas.Value.ToInstant(),
                    End = fajrPrayerTime.Karaha.Value.ToInstant()
                },
                new PrayerTimeGraphicSubTimeVO
                {
                    Name = "Karaha",
                    Start = fajrPrayerTime.Karaha.Value.ToInstant(),
                    End = prayerTime.End.Value.ToInstant()
                },
            ];
        }
        else if (prayerTime is DuhaPrayerTime duhaPrayerTime)
        {
            if (duhaPrayerTime.QuarterOfDay is null)
            {
                return [];
            }

            return [
                new PrayerTimeGraphicSubTimeVO
                {
                    Name = "Normal",
                    Start = prayerTime.Start.Value.ToInstant(),
                    End = duhaPrayerTime.QuarterOfDay.Value.ToInstant()
                },
                new PrayerTimeGraphicSubTimeVO
                {
                    Name = "Empfohlen",
                    Start = duhaPrayerTime.QuarterOfDay.Value.ToInstant(),
                    End = prayerTime.End.Value.ToInstant()
                },
            ];
        }
        else if (prayerTime is AsrPrayerTime asrPrayerTime)
        {
            if (asrPrayerTime.Mithlayn is null || asrPrayerTime.Karaha is null)
            {
                return [];
            }

            return [
                new PrayerTimeGraphicSubTimeVO
                {
                    Name = "Ikhtiyar",
                    Start = prayerTime.Start.Value.ToInstant(),
                    End = asrPrayerTime.Mithlayn.Value.ToInstant()
                },
                new PrayerTimeGraphicSubTimeVO
                {
                    Name = "Normal",
                    Start = asrPrayerTime.Mithlayn.Value.ToInstant(),
                    End = asrPrayerTime.Karaha.Value.ToInstant()
                },
                new PrayerTimeGraphicSubTimeVO
                {
                    Name = "Karaha",
                    Start = asrPrayerTime.Karaha.Value.ToInstant(),
                    End = prayerTime.End.Value.ToInstant()
                },
            ];
        }
        else if (prayerTime is MaghribPrayerTime maghribPrayerTime)
        {
            if (maghribPrayerTime.SufficientTime is null || maghribPrayerTime.Ishtibaq is null)
            {
                return [];
            }

            return [
                new PrayerTimeGraphicSubTimeVO
                {
                    Name = "Normal",
                    Start = prayerTime.Start.Value.ToInstant(),
                    End = maghribPrayerTime.SufficientTime.Value.ToInstant()
                },
                new PrayerTimeGraphicSubTimeVO
                {
                    Name = "Karaha1",
                    Start = maghribPrayerTime.SufficientTime.Value.ToInstant(),
                    End = maghribPrayerTime.Ishtibaq.Value.ToInstant()
                },
                new PrayerTimeGraphicSubTimeVO
                {
                    Name = "Karaha2",
                    Start = maghribPrayerTime.Ishtibaq.Value.ToInstant(),
                    End = prayerTime.End.Value.ToInstant()
                }
            ];
        }
        else if (prayerTime is IshaPrayerTime ishaPrayerTime)
        {
            if (ishaPrayerTime.FirstThirdOfNight is null
                || ishaPrayerTime.SecondThirdOfNight is null
                || ishaPrayerTime.MiddleOfNight is null)
            {
                return [];
            }

            return [
                new PrayerTimeGraphicSubTimeVO
                {
                    Name = "1/3",
                    Start = prayerTime.Start.Value.ToInstant(),
                    End = ishaPrayerTime.FirstThirdOfNight.Value.ToInstant(),
                    SubTimeType = ESubTimeType.RightHalf
                },
                new PrayerTimeGraphicSubTimeVO
                {
                    Name = "2/3",
                    Start = ishaPrayerTime.FirstThirdOfNight.Value.ToInstant(),
                    End = ishaPrayerTime.SecondThirdOfNight.Value.ToInstant(),
                    SubTimeType = ESubTimeType.RightHalf
                },
                new PrayerTimeGraphicSubTimeVO
                {
                    Name = "3/3",
                    Start = ishaPrayerTime.SecondThirdOfNight.Value.ToInstant(),
                    End = prayerTime.End.Value.ToInstant(),
                    SubTimeType = ESubTimeType.RightHalf
                },

                new PrayerTimeGraphicSubTimeVO
                {
                    Name = "1/2",
                    Start = prayerTime.Start.Value.ToInstant(),
                    End = ishaPrayerTime.MiddleOfNight.Value.ToInstant(),
                    SubTimeType = ESubTimeType.LeftHalf
                },
                new PrayerTimeGraphicSubTimeVO
                {
                    Name = "2/2",
                    Start = ishaPrayerTime.MiddleOfNight.Value.ToInstant(),
                    End = prayerTime.End.Value.ToInstant(),
                    SubTimeType = ESubTimeType.LeftHalf
                },
            ];
        }

        return [];
    }

}
