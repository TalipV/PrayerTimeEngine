using PrayerTimeEngine.Code.Common.Attribute;

namespace PrayerTimeEngine.Code.Common.Enum
{
    public enum ETimeType
    {
        [TimeTypeSupportedBy(ECalculationSource.None, ECalculationSource.Muwaqqit, ECalculationSource.Fazilet, ECalculationSource.Semerkand)]
        [DegreeTimeTypeAttribute]
        [StartEndTimeTypeAttribute]
        FajrStart,
        [TimeTypeSupportedBy(ECalculationSource.None, ECalculationSource.Muwaqqit, ECalculationSource.Fazilet, ECalculationSource.Semerkand)]
        [StartEndTimeTypeAttribute]
        FajrEnd,
        [TimeTypeSupportedBy(ECalculationSource.None, ECalculationSource.Muwaqqit)]
        [DegreeTimeTypeAttribute]
        FajrGhalas,
        [TimeTypeSupportedBy(ECalculationSource.None, ECalculationSource.Muwaqqit)]
        [DegreeTimeTypeAttribute]
        FajrKaraha,

        [TimeTypeSupportedBy(ECalculationSource.None, ECalculationSource.Muwaqqit)]
        [DegreeTimeTypeAttribute]
        [StartEndTimeTypeAttribute]
        DuhaStart,

        [TimeTypeSupportedBy(ECalculationSource.None, ECalculationSource.Muwaqqit, ECalculationSource.Fazilet, ECalculationSource.Semerkand)]
        [StartEndTimeTypeAttribute]
        DhuhrStart,
        [TimeTypeSupportedBy(ECalculationSource.None, ECalculationSource.Muwaqqit, ECalculationSource.Fazilet, ECalculationSource.Semerkand)]
        [StartEndTimeTypeAttribute]
        DhuhrEnd,

        [TimeTypeSupportedBy(ECalculationSource.None, ECalculationSource.Muwaqqit, ECalculationSource.Fazilet, ECalculationSource.Semerkand)]
        [StartEndTimeTypeAttribute]
        AsrStart,
        [TimeTypeSupportedBy(ECalculationSource.None, ECalculationSource.Muwaqqit, ECalculationSource.Fazilet, ECalculationSource.Semerkand)]
        [StartEndTimeTypeAttribute]
        AsrEnd,
        [TimeTypeSupportedBy(ECalculationSource.None, ECalculationSource.Muwaqqit)]
        AsrMithlayn,
        [TimeTypeSupportedBy(ECalculationSource.None, ECalculationSource.Muwaqqit)]
        [DegreeTimeTypeAttribute]
        AsrKaraha,

        [TimeTypeSupportedBy(ECalculationSource.None, ECalculationSource.Muwaqqit, ECalculationSource.Fazilet, ECalculationSource.Semerkand)]
        [StartEndTimeTypeAttribute]
        MaghribStart,
        [TimeTypeSupportedBy(ECalculationSource.None, ECalculationSource.Muwaqqit, ECalculationSource.Fazilet, ECalculationSource.Semerkand)]
        [DegreeTimeTypeAttribute]
        [StartEndTimeTypeAttribute]
        MaghribEnd,
        [TimeTypeSupportedBy(ECalculationSource.None, ECalculationSource.Muwaqqit)]
        [DegreeTimeTypeAttribute]
        MaghribIshtibaq,

        [TimeTypeSupportedBy(ECalculationSource.None, ECalculationSource.Muwaqqit, ECalculationSource.Fazilet, ECalculationSource.Semerkand)]
        [DegreeTimeTypeAttribute]
        [StartEndTimeTypeAttribute]
        IshaStart,
        [TimeTypeSupportedBy(ECalculationSource.None, ECalculationSource.Muwaqqit, ECalculationSource.Fazilet, ECalculationSource.Semerkand)]
        [DegreeTimeTypeAttribute]
        [StartEndTimeTypeAttribute]
        IshaEnd,

        // NO NEED FOR CALCULATIONS BY CALCULATION SOURCES
        [SimpleTimeTypeAttribute]
        DuhaQuarterOfDay,
        [SimpleTimeTypeAttribute]
        IshaFirstThird,
        [SimpleTimeTypeAttribute]
        IshaMidnight,
        [SimpleTimeTypeAttribute]
        IshaSecondThird,

        [SimpleTimeTypeAttribute]
        MaghribSufficientTime,
        [SimpleTimeTypeAttribute]
        [StartEndTimeTypeAttribute]
        DuhaEnd,
    }
}
