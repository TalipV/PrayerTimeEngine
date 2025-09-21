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
            return createFajrPrayerTimeGraphicSubTimeVO(fajrPrayerTime);
        }
        else if (prayerTime is DuhaPrayerTime duhaPrayerTime)
        {
            return createDuhaPrayerTimeGraphicSubTimeVO(duhaPrayerTime);
        }
        else if (prayerTime is AsrPrayerTime asrPrayerTime)
        {
            return createAsrPrayerTimeGraphicSubTimeVO(asrPrayerTime);
        }
        else if (prayerTime is MaghribPrayerTime maghribPrayerTime)
        {
            return createMaghribPrayerTimeGraphicSubTimeVO(maghribPrayerTime);
        }
        else if (prayerTime is IshaPrayerTime ishaPrayerTime)
        {
            return createIshaPrayerTimeGraphicSubTimeVO(ishaPrayerTime);
        }

        return [];
    }

    private static List<PrayerTimeGraphicSubTimeVO> createFajrPrayerTimeGraphicSubTimeVO(FajrPrayerTime fajrPrayerTime)
    {
        if (fajrPrayerTime.Ghalas is null || fajrPrayerTime.Karaha is null)
        {
            return [];
        }

        return [
            new PrayerTimeGraphicSubTimeVO
                {
                    Name = "Ikhtiyar",
                    Start = fajrPrayerTime.Start.Value.ToInstant(),
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
                    End = fajrPrayerTime.End.Value.ToInstant()
                },
            ];
    }

    private static List<PrayerTimeGraphicSubTimeVO> createDuhaPrayerTimeGraphicSubTimeVO(DuhaPrayerTime duhaPrayerTime)
    {
        if (duhaPrayerTime.QuarterOfDay is null)
        {
            return [];
        }

        return [
            new PrayerTimeGraphicSubTimeVO
                {
                    Name = "Normal",
                    Start = duhaPrayerTime.Start.Value.ToInstant(),
                    End = duhaPrayerTime.QuarterOfDay.Value.ToInstant()
                },
                new PrayerTimeGraphicSubTimeVO
                {
                    Name = "Empfohlen",
                    Start = duhaPrayerTime.QuarterOfDay.Value.ToInstant(),
                    End = duhaPrayerTime.End.Value.ToInstant()
                },
            ];
    }

    private static List<PrayerTimeGraphicSubTimeVO> createAsrPrayerTimeGraphicSubTimeVO(AsrPrayerTime asrPrayerTime)
    {
        if (asrPrayerTime.Mithlayn is null || asrPrayerTime.Karaha is null)
        {
            return [];
        }

        return [
            new PrayerTimeGraphicSubTimeVO
                {
                    Name = "Ikhtiyar",
                    Start = asrPrayerTime.Start.Value.ToInstant(),
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
                    End = asrPrayerTime.End.Value.ToInstant()
                },
            ];
    }

    private static List<PrayerTimeGraphicSubTimeVO> createMaghribPrayerTimeGraphicSubTimeVO(MaghribPrayerTime maghribPrayerTime, IshaPrayerTime ishaPrayerTime = null)
    {
        if (maghribPrayerTime.SufficientTime is null || maghribPrayerTime.Ishtibaq is null)
        {
            return [];
        }

        Instant mainStart = maghribPrayerTime.Start.Value.ToInstant();
        Instant mainEnd = maghribPrayerTime.End.Value.ToInstant();

        List<PrayerTimeGraphicSubTimeVO> result = [];

        addSubTimeIfValid(mainStart, mainEnd, result, "Normal", maghribPrayerTime.Start.Value.ToInstant(), maghribPrayerTime.SufficientTime.Value.ToInstant());
        addSubTimeIfValid(mainStart, mainEnd, result, "Karaha1", maghribPrayerTime.SufficientTime.Value.ToInstant(), maghribPrayerTime.Ishtibaq.Value.ToInstant());
        addSubTimeIfValid(mainStart, mainEnd, result, "Karaha2", maghribPrayerTime.Ishtibaq.Value.ToInstant(), maghribPrayerTime.End.Value.ToInstant());

        if (ishaPrayerTime != null)
        {
            addSubTimeIfValid(mainStart, mainEnd, result, "1/3", maghribPrayerTime.Start.Value.ToInstant(), ishaPrayerTime.FirstThirdOfNight.Value.ToInstant(), ESubTimeType.RightHalf);
            addSubTimeIfValid(mainStart, mainEnd, result, "2/3", ishaPrayerTime.FirstThirdOfNight.Value.ToInstant(), ishaPrayerTime.SecondThirdOfNight.Value.ToInstant(), ESubTimeType.RightHalf);
            addSubTimeIfValid(mainStart, mainEnd, result, "3/3", ishaPrayerTime.SecondThirdOfNight.Value.ToInstant(), maghribPrayerTime.End.Value.ToInstant(), ESubTimeType.RightHalf);
        }

        return result;
    }

    private static List<PrayerTimeGraphicSubTimeVO> createIshaPrayerTimeGraphicSubTimeVO(IshaPrayerTime ishaPrayerTime)
    {
        if (ishaPrayerTime.FirstThirdOfNight is null
            || ishaPrayerTime.SecondThirdOfNight is null
            || ishaPrayerTime.MiddleOfNight is null)
        {
            return [];
        }

        Instant mainStart = ishaPrayerTime.Start.Value.ToInstant();
        Instant mainEnd = ishaPrayerTime.End.Value.ToInstant();

        List<PrayerTimeGraphicSubTimeVO> result = [];

        addSubTimeIfValid(mainStart, mainEnd, result, "1/3", ishaPrayerTime.Start.Value.ToInstant(), ishaPrayerTime.FirstThirdOfNight.Value.ToInstant(), ESubTimeType.RightHalf);
        addSubTimeIfValid(mainStart, mainEnd, result, "2/3", ishaPrayerTime.FirstThirdOfNight.Value.ToInstant(), ishaPrayerTime.SecondThirdOfNight.Value.ToInstant(), ESubTimeType.RightHalf);
        addSubTimeIfValid(mainStart, mainEnd, result, "3/3", ishaPrayerTime.SecondThirdOfNight.Value.ToInstant(), ishaPrayerTime.End.Value.ToInstant(), ESubTimeType.RightHalf);
        
        addSubTimeIfValid(mainStart, mainEnd, result, "1/2", ishaPrayerTime.Start.Value.ToInstant(), ishaPrayerTime.MiddleOfNight.Value.ToInstant(), ESubTimeType.LeftHalf);
        addSubTimeIfValid(mainStart, mainEnd, result, "2/2", ishaPrayerTime.MiddleOfNight.Value.ToInstant(), ishaPrayerTime.End.Value.ToInstant(), ESubTimeType.LeftHalf);

        return result;
    }

    private static void addSubTimeIfValid(
        Instant mainStart, Instant mainEnd, 
        List<PrayerTimeGraphicSubTimeVO> result, 
        string name, 
        Instant subTimeStart, Instant subTimeEnd, 
        ESubTimeType type = ESubTimeType.FullHalf)
    {
        // non existent time
        if (subTimeStart >= subTimeEnd)
        {
            return;
        }

        // sub time is not at all contained in main time
        if (subTimeEnd <= mainStart)
        {
            return;
        }

        // sub time starts before main time but ends within it
        if (subTimeStart < mainStart && subTimeEnd <= mainEnd)
        {
            subTimeStart = mainStart;
        }

        // sub time starts within main time but ends after it
        if (subTimeStart >= mainStart && subTimeStart < mainEnd && subTimeEnd > mainEnd)
        {
            subTimeEnd = mainEnd;
        }

        result.Add(new PrayerTimeGraphicSubTimeVO
        {
            Name = name,
            Start = subTimeStart,
            End = subTimeEnd,
            SubTimeType = type
        });
    }
}
