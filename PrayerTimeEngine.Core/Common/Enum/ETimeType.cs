using PrayerTimeEngine.Common.Attribute;

namespace PrayerTimeEngine.Core.Common.Enum
{
    public enum ETimeType
    {
        #region Fajr

        [TimeTypeSupportedBy(ECalculationSource.None, ECalculationSource.Muwaqqit, ECalculationSource.Fazilet, ECalculationSource.Semerkand)]
        [TimeTypeForPrayerType(EPrayerType.Fajr)]
        [DegreeTimeType]
        [IsNotHidableTimeType]
        FajrStart,

        [TimeTypeSupportedBy(ECalculationSource.None, ECalculationSource.Muwaqqit, ECalculationSource.Fazilet, ECalculationSource.Semerkand)]
        [TimeTypeForPrayerType(EPrayerType.Fajr)]
        [IsNotHidableTimeType]
        FajrEnd,

        [TimeTypeSupportedBy(ECalculationSource.None, ECalculationSource.Muwaqqit)]
        [TimeTypeForPrayerType(EPrayerType.Fajr)]
        [DegreeTimeType]
        FajrGhalas,

        [TimeTypeSupportedBy(ECalculationSource.None, ECalculationSource.Muwaqqit)]
        [TimeTypeForPrayerType(EPrayerType.Fajr)]
        [DegreeTimeType]
        FajrKaraha,

        #endregion Fajr

        #region Duha

        [TimeTypeSupportedBy(ECalculationSource.None, ECalculationSource.Muwaqqit)]
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

        [TimeTypeSupportedBy(ECalculationSource.None, ECalculationSource.Muwaqqit, ECalculationSource.Fazilet, ECalculationSource.Semerkand)]
        [TimeTypeForPrayerType(EPrayerType.Dhuhr)]
        [IsNotHidableTimeType]
        DhuhrStart,

        [TimeTypeSupportedBy(ECalculationSource.None, ECalculationSource.Muwaqqit, ECalculationSource.Fazilet, ECalculationSource.Semerkand)]
        [TimeTypeForPrayerType(EPrayerType.Dhuhr)]
        [IsNotHidableTimeType]
        DhuhrEnd,

        #endregion Dhuhr

        #region Asr

        [TimeTypeSupportedBy(ECalculationSource.None, ECalculationSource.Muwaqqit, ECalculationSource.Fazilet, ECalculationSource.Semerkand)]
        [TimeTypeForPrayerType(EPrayerType.Asr)]
        [IsNotHidableTimeType]
        AsrStart,

        [TimeTypeSupportedBy(ECalculationSource.None, ECalculationSource.Muwaqqit, ECalculationSource.Fazilet, ECalculationSource.Semerkand)]
        [TimeTypeForPrayerType(EPrayerType.Asr)]
        [IsNotHidableTimeType]
        AsrEnd,

        [TimeTypeSupportedBy(ECalculationSource.None, ECalculationSource.Muwaqqit)]
        [TimeTypeForPrayerType(EPrayerType.Asr)]
        AsrMithlayn,

        [TimeTypeSupportedBy(ECalculationSource.None, ECalculationSource.Muwaqqit)]
        [TimeTypeForPrayerType(EPrayerType.Asr)]
        [DegreeTimeType]
        AsrKaraha,

        #endregion Asr

        #region Maghrib

        [TimeTypeSupportedBy(ECalculationSource.None, ECalculationSource.Muwaqqit, ECalculationSource.Fazilet, ECalculationSource.Semerkand)]
        [TimeTypeForPrayerType(EPrayerType.Maghrib)]
        [IsNotHidableTimeType]
        MaghribStart,

        [TimeTypeSupportedBy(ECalculationSource.None, ECalculationSource.Muwaqqit, ECalculationSource.Fazilet, ECalculationSource.Semerkand)]
        [TimeTypeForPrayerType(EPrayerType.Maghrib)]
        [DegreeTimeType]
        [IsNotHidableTimeType]
        MaghribEnd,

        [TimeTypeForPrayerType(EPrayerType.Maghrib)]
        [ConfigurableSimpleType]
        MaghribSufficientTime,

        [TimeTypeSupportedBy(ECalculationSource.None, ECalculationSource.Muwaqqit)]
        [TimeTypeForPrayerType(EPrayerType.Maghrib)]
        [DegreeTimeType]
        MaghribIshtibaq,

        #endregion Maghrib

        #region Isha

        [TimeTypeSupportedBy(ECalculationSource.None, ECalculationSource.Muwaqqit, ECalculationSource.Fazilet, ECalculationSource.Semerkand)]
        [TimeTypeForPrayerType(EPrayerType.Isha)]
        [DegreeTimeType]
        [IsNotHidableTimeType]
        IshaStart,

        [TimeTypeSupportedBy(ECalculationSource.None, ECalculationSource.Muwaqqit, ECalculationSource.Fazilet, ECalculationSource.Semerkand)]
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
}
