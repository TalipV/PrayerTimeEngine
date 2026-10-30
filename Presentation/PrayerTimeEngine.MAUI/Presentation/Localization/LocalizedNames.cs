using PrayerTimeEngine.Core.Common.Enum;

namespace PrayerTimeEngine.Presentation.Localization;

/// <summary>
/// Translated display names for enum values of the domain, which must not be shown with their
/// code names (e.g. <see cref="ETimeSection.General"/> or <see cref="ETimeType.IshaFirstThird"/>).
/// </summary>
public static class LocalizedNames
{
    public static string GetSectionName(ETimeSection section)
    {
        return section switch
        {
            ETimeSection.Fajr => AppResources.Prayer_Fajr,
            ETimeSection.Duha => AppResources.Prayer_Duha,
            ETimeSection.Dhuhr => AppResources.Prayer_Dhuhr,
            ETimeSection.Asr => AppResources.Prayer_Asr,
            ETimeSection.Maghrib => AppResources.Prayer_Maghrib,
            ETimeSection.Isha => AppResources.Prayer_Isha,
            ETimeSection.General => AppResources.Prayer_General,
            _ => section.ToString()
        };
    }

    /// <summary>
    /// The name of a time within its section, e.g. "Start" for <see cref="ETimeType.FajrStart"/>.
    /// Used where the section is already visible, like the tabs of the settings page.
    /// </summary>
    public static string GetTimeTypeName(ETimeType timeType)
    {
        return timeType switch
        {
            ETimeType.FajrStart or ETimeType.DuhaStart or ETimeType.DhuhrStart
                or ETimeType.AsrStart or ETimeType.MaghribStart or ETimeType.IshaStart => AppResources.TimeType_Start,
            ETimeType.FajrEnd or ETimeType.DuhaEnd or ETimeType.DhuhrEnd
                or ETimeType.AsrEnd or ETimeType.MaghribEnd or ETimeType.IshaEnd => AppResources.TimeType_End,
            ETimeType.FajrGhalas => AppResources.SubTime_Ghalas,
            ETimeType.FajrKaraha or ETimeType.AsrKaraha => AppResources.SubTime_Karaha,
            ETimeType.DuhaQuarterOfDay => AppResources.SubTime_Quarter,
            ETimeType.DuhaHalfOfDay => AppResources.SubTime_Half,
            ETimeType.AsrMithlayn => AppResources.SubTime_Mithlayn,
            ETimeType.MaghribSufficientTime => AppResources.SubTime_Sufficient,
            ETimeType.MaghribIshtibak => AppResources.SubTime_Ishtibak,
            ETimeType.IshaFirstThird => AppResources.TimeType_FirstThird,
            ETimeType.IshaMidnight => AppResources.TimeType_Midnight,
            ETimeType.IshaSecondThird => AppResources.TimeType_SecondThird,
            ETimeType.QiblaTime => AppResources.Prayer_Qibla,
            _ => timeType.ToString()
        };
    }
}
