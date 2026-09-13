using PrayerTimeEngine.Core.Common.Attribute;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes;

namespace PrayerTimeEngine.Core.Common.Enum;

public enum ETimeType
{
    #region Fajr

    [TimeTypeSupportedBy(EDynamicPrayerTimeProviderType.None, EDynamicPrayerTimeProviderType.Muwaqqit, EDynamicPrayerTimeProviderType.Fazilet, EDynamicPrayerTimeProviderType.Semerkand)]
    [TimeTypeForSection(ETimeSection.Fajr)]
    [DegreeTimeType]
    [IsNotHidableTimeType]
    FajrStart = 100,

    [TimeTypeSupportedBy(EDynamicPrayerTimeProviderType.None, EDynamicPrayerTimeProviderType.Muwaqqit, EDynamicPrayerTimeProviderType.Fazilet, EDynamicPrayerTimeProviderType.Semerkand)]
    [TimeTypeForSection(ETimeSection.Fajr)]
    [IsNotHidableTimeType]
    FajrEnd = 110,

    [TimeTypeSupportedBy(EDynamicPrayerTimeProviderType.None, EDynamicPrayerTimeProviderType.Muwaqqit)]
    [TimeTypeForSection(ETimeSection.Fajr)]
    [DegreeTimeType]
    FajrGhalas = 120,

    [TimeTypeSupportedBy(EDynamicPrayerTimeProviderType.None, EDynamicPrayerTimeProviderType.Muwaqqit)]
    [TimeTypeForSection(ETimeSection.Fajr)]
    [DegreeTimeType]
    FajrKaraha = 130,

    #endregion Fajr

    #region Duha

    [TimeTypeSupportedBy(EDynamicPrayerTimeProviderType.None, EDynamicPrayerTimeProviderType.Muwaqqit, EDynamicPrayerTimeProviderType.Fazilet)]
    [TimeTypeForSection(ETimeSection.Duha)]
    [DegreeTimeType]
    [IsNotHidableTimeType]
    DuhaStart = 200,

    [TimeTypeForSection(ETimeSection.Duha)]
    [SimpleTimeType]
    DuhaQuarterOfDay = 210,

    [TimeTypeForSection(ETimeSection.Duha)]
    [SimpleTimeType]
    DuhaHalfOfDay = 220,

    [TimeTypeForSection(ETimeSection.Duha)]
    [ConfigurableSimpleType]
    [IsNotHidableTimeType]
    DuhaEnd = 230,

    #endregion Duha

    #region Dhuhr

    [TimeTypeSupportedBy(EDynamicPrayerTimeProviderType.None, EDynamicPrayerTimeProviderType.Muwaqqit, EDynamicPrayerTimeProviderType.Fazilet, EDynamicPrayerTimeProviderType.Semerkand)]
    [TimeTypeForSection(ETimeSection.Dhuhr)]
    [IsNotHidableTimeType]
    DhuhrStart = 300,

    [TimeTypeSupportedBy(EDynamicPrayerTimeProviderType.None, EDynamicPrayerTimeProviderType.Muwaqqit, EDynamicPrayerTimeProviderType.Fazilet, EDynamicPrayerTimeProviderType.Semerkand)]
    [TimeTypeForSection(ETimeSection.Dhuhr)]
    [IsNotHidableTimeType]
    DhuhrEnd = 310,

    #endregion Dhuhr

    #region Asr

    [TimeTypeSupportedBy(EDynamicPrayerTimeProviderType.None, EDynamicPrayerTimeProviderType.Muwaqqit, EDynamicPrayerTimeProviderType.Fazilet, EDynamicPrayerTimeProviderType.Semerkand)]
    [TimeTypeForSection(ETimeSection.Asr)]
    [IsNotHidableTimeType]
    AsrStart = 400,

    [TimeTypeSupportedBy(EDynamicPrayerTimeProviderType.None, EDynamicPrayerTimeProviderType.Muwaqqit, EDynamicPrayerTimeProviderType.Fazilet, EDynamicPrayerTimeProviderType.Semerkand)]
    [TimeTypeForSection(ETimeSection.Asr)]
    [IsNotHidableTimeType]
    AsrEnd = 410,

    [TimeTypeSupportedBy(EDynamicPrayerTimeProviderType.None, EDynamicPrayerTimeProviderType.Muwaqqit)]
    [TimeTypeForSection(ETimeSection.Asr)]
    AsrMithlayn = 420,

    [TimeTypeSupportedBy(EDynamicPrayerTimeProviderType.None, EDynamicPrayerTimeProviderType.Muwaqqit)]
    [TimeTypeForSection(ETimeSection.Asr)]
    [DegreeTimeType]
    AsrKaraha = 430,

    #endregion Asr

    #region Maghrib

    [TimeTypeSupportedBy(EDynamicPrayerTimeProviderType.None, EDynamicPrayerTimeProviderType.Muwaqqit, EDynamicPrayerTimeProviderType.Fazilet, EDynamicPrayerTimeProviderType.Semerkand)]
    [TimeTypeForSection(ETimeSection.Maghrib)]
    [IsNotHidableTimeType]
    MaghribStart = 500,

    [TimeTypeSupportedBy(EDynamicPrayerTimeProviderType.None, EDynamicPrayerTimeProviderType.Muwaqqit, EDynamicPrayerTimeProviderType.Fazilet, EDynamicPrayerTimeProviderType.Semerkand)]
    [TimeTypeForSection(ETimeSection.Maghrib)]
    [DegreeTimeType]
    [IsNotHidableTimeType]
    MaghribEnd = 510,

    [TimeTypeForSection(ETimeSection.Maghrib)]
    [ConfigurableSimpleType]
    MaghribSufficientTime = 520,

    [TimeTypeSupportedBy(EDynamicPrayerTimeProviderType.None, EDynamicPrayerTimeProviderType.Muwaqqit)]
    [TimeTypeForSection(ETimeSection.Maghrib)]
    [DegreeTimeType]
    MaghribIshtibak = 530,

    #endregion Maghrib

    #region Isha

    [TimeTypeSupportedBy(EDynamicPrayerTimeProviderType.None, EDynamicPrayerTimeProviderType.Muwaqqit, EDynamicPrayerTimeProviderType.Fazilet, EDynamicPrayerTimeProviderType.Semerkand)]
    [TimeTypeForSection(ETimeSection.Isha)]
    [DegreeTimeType]
    [IsNotHidableTimeType]
    IshaStart = 600,

    [TimeTypeSupportedBy(EDynamicPrayerTimeProviderType.None, EDynamicPrayerTimeProviderType.Muwaqqit, EDynamicPrayerTimeProviderType.Fazilet, EDynamicPrayerTimeProviderType.Semerkand)]
    [TimeTypeForSection(ETimeSection.Isha)]
    [DegreeTimeType]
    [IsNotHidableTimeType]
    IshaEnd = 610,

    [TimeTypeForSection(ETimeSection.Isha)]
    [SimpleTimeType]
    IshaFirstThird = 620,

    [TimeTypeForSection(ETimeSection.Isha)]
    [SimpleTimeType]
    IshaMidnight = 630,

    [TimeTypeForSection(ETimeSection.Isha)]
    [SimpleTimeType]
    IshaSecondThird = 640,

    #endregion Isha

    #region General

    [TimeTypeSupportedBy(EDynamicPrayerTimeProviderType.Muwaqqit)]
    [TimeTypeForSection(ETimeSection.General)]
    QiblaTime = 1000,

    #endregion General
}
