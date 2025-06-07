using PrayerTimeEngine.Core.Common.Attribute;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes;

namespace PrayerTimeEngine.Core.Common.Enum;

public enum ETimeType
{
    #region Fajr

    [TimeTypeSupportedBy(EDynamicPrayerTimeProviderType.None, EDynamicPrayerTimeProviderType.Muwaqqit, EDynamicPrayerTimeProviderType.Fazilet, EDynamicPrayerTimeProviderType.Semerkand)]
    [TimeTypeForPrayerType(EPrayerType.Fajr)]
    [DegreeTimeType]
    [IsNotHidableTimeType]
    FajrStart,

    [TimeTypeSupportedBy(EDynamicPrayerTimeProviderType.None, EDynamicPrayerTimeProviderType.Muwaqqit, EDynamicPrayerTimeProviderType.Fazilet, EDynamicPrayerTimeProviderType.Semerkand)]
    [TimeTypeForPrayerType(EPrayerType.Fajr)]
    [IsNotHidableTimeType]
    FajrEnd,

    [TimeTypeSupportedBy(EDynamicPrayerTimeProviderType.None, EDynamicPrayerTimeProviderType.Muwaqqit)]
    [TimeTypeForPrayerType(EPrayerType.Fajr)]
    [DegreeTimeType]
    FajrGhalas,

    [TimeTypeSupportedBy(EDynamicPrayerTimeProviderType.None, EDynamicPrayerTimeProviderType.Muwaqqit)]
    [TimeTypeForPrayerType(EPrayerType.Fajr)]
    [DegreeTimeType]
    FajrKaraha,

    #endregion Fajr

    #region Duha

    [TimeTypeSupportedBy(EDynamicPrayerTimeProviderType.None, EDynamicPrayerTimeProviderType.Muwaqqit, EDynamicPrayerTimeProviderType.Fazilet)]
    [TimeTypeForPrayerType(EPrayerType.Duha)]
    [DegreeTimeType]
    [IsNotHidableTimeType]
    DuhaStart,

    [TimeTypeForPrayerType(EPrayerType.Duha)]
    [SimpleTimeType]
    DuhaQuarterOfDay,

    [TimeTypeForPrayerType(EPrayerType.Duha)]
    [ConfigurableSimpleType]
    [IsNotHidableTimeType]
    DuhaEnd,

    #endregion Duha

    #region Dhuhr

    [TimeTypeSupportedBy(EDynamicPrayerTimeProviderType.None, EDynamicPrayerTimeProviderType.Muwaqqit, EDynamicPrayerTimeProviderType.Fazilet, EDynamicPrayerTimeProviderType.Semerkand)]
    [TimeTypeForPrayerType(EPrayerType.Dhuhr)]
    [IsNotHidableTimeType]
    DhuhrStart,

    [TimeTypeSupportedBy(EDynamicPrayerTimeProviderType.None, EDynamicPrayerTimeProviderType.Muwaqqit, EDynamicPrayerTimeProviderType.Fazilet, EDynamicPrayerTimeProviderType.Semerkand)]
    [TimeTypeForPrayerType(EPrayerType.Dhuhr)]
    [IsNotHidableTimeType]
    DhuhrEnd,

    #endregion Dhuhr

    #region Asr

    [TimeTypeSupportedBy(EDynamicPrayerTimeProviderType.None, EDynamicPrayerTimeProviderType.Muwaqqit, EDynamicPrayerTimeProviderType.Fazilet, EDynamicPrayerTimeProviderType.Semerkand)]
    [TimeTypeForPrayerType(EPrayerType.Asr)]
    [IsNotHidableTimeType]
    AsrStart,

    [TimeTypeSupportedBy(EDynamicPrayerTimeProviderType.None, EDynamicPrayerTimeProviderType.Muwaqqit, EDynamicPrayerTimeProviderType.Fazilet, EDynamicPrayerTimeProviderType.Semerkand)]
    [TimeTypeForPrayerType(EPrayerType.Asr)]
    [IsNotHidableTimeType]
    AsrEnd,

    [TimeTypeSupportedBy(EDynamicPrayerTimeProviderType.None, EDynamicPrayerTimeProviderType.Muwaqqit)]
    [TimeTypeForPrayerType(EPrayerType.Asr)]
    AsrMithlayn,

    [TimeTypeSupportedBy(EDynamicPrayerTimeProviderType.None, EDynamicPrayerTimeProviderType.Muwaqqit)]
    [TimeTypeForPrayerType(EPrayerType.Asr)]
    [DegreeTimeType]
    AsrKaraha,

    #endregion Asr

    #region Maghrib

    [TimeTypeSupportedBy(EDynamicPrayerTimeProviderType.None, EDynamicPrayerTimeProviderType.Muwaqqit, EDynamicPrayerTimeProviderType.Fazilet, EDynamicPrayerTimeProviderType.Semerkand)]
    [TimeTypeForPrayerType(EPrayerType.Maghrib)]
    [IsNotHidableTimeType]
    MaghribStart,

    [TimeTypeSupportedBy(EDynamicPrayerTimeProviderType.None, EDynamicPrayerTimeProviderType.Muwaqqit, EDynamicPrayerTimeProviderType.Fazilet, EDynamicPrayerTimeProviderType.Semerkand)]
    [TimeTypeForPrayerType(EPrayerType.Maghrib)]
    [DegreeTimeType]
    [IsNotHidableTimeType]
    MaghribEnd,

    [TimeTypeForPrayerType(EPrayerType.Maghrib)]
    [ConfigurableSimpleType]
    MaghribSufficientTime,

    [TimeTypeSupportedBy(EDynamicPrayerTimeProviderType.None, EDynamicPrayerTimeProviderType.Muwaqqit)]
    [TimeTypeForPrayerType(EPrayerType.Maghrib)]
    [DegreeTimeType]
    MaghribIshtibaq,

    #endregion Maghrib

    #region Isha

    [TimeTypeSupportedBy(EDynamicPrayerTimeProviderType.None, EDynamicPrayerTimeProviderType.Muwaqqit, EDynamicPrayerTimeProviderType.Fazilet, EDynamicPrayerTimeProviderType.Semerkand)]
    [TimeTypeForPrayerType(EPrayerType.Isha)]
    [DegreeTimeType]
    [IsNotHidableTimeType]
    IshaStart,

    [TimeTypeSupportedBy(EDynamicPrayerTimeProviderType.None, EDynamicPrayerTimeProviderType.Muwaqqit, EDynamicPrayerTimeProviderType.Fazilet, EDynamicPrayerTimeProviderType.Semerkand)]
    [TimeTypeForPrayerType(EPrayerType.Isha)]
    [DegreeTimeType]
    [IsNotHidableTimeType]
    IshaEnd,

    [TimeTypeForPrayerType(EPrayerType.Isha)]
    [SimpleTimeType]
    IshaFirstThird,

    [TimeTypeForPrayerType(EPrayerType.Isha)]
    [SimpleTimeType]
    IshaMidnight,

    [TimeTypeForPrayerType(EPrayerType.Isha)]
    [SimpleTimeType]
    IshaSecondThird,

    #endregion Isha
}
