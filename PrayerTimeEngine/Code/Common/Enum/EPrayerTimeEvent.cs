using PrayerTimeEngine.Code.Common.Attribute;

namespace PrayerTimeEngine.Code.Common.Enum
{
    public enum EPrayerTimeEvent
    {
        [SupportedBy(ECalculationSource.None, ECalculationSource.Muwaqqit, ECalculationSource.Fazilet, ECalculationSource.Semerkand)]
        Start,
        [SupportedBy(ECalculationSource.None, ECalculationSource.Muwaqqit, ECalculationSource.Fazilet, ECalculationSource.Semerkand)]
        End,

        // ### ADDITIONAL TIMES
        [SupportedBy(ECalculationSource.None, ECalculationSource.Muwaqqit)]
        Fajr_Fadilah,
        [SupportedBy(ECalculationSource.None, ECalculationSource.Muwaqqit)]
        Fajr_Karaha,
        [SupportedBy(ECalculationSource.None, ECalculationSource.Muwaqqit)]
        AsrMithlayn,
        [SupportedBy(ECalculationSource.None, ECalculationSource.Muwaqqit)]
        Asr_Karaha,
        [SupportedBy(ECalculationSource.None, ECalculationSource.Muwaqqit)]
        IshtibaqAnNujum,

        // ### IMPLICIT TIMES
        [SupportedBy(ECalculationSource.None, ECalculationSource.Muwaqqit, ECalculationSource.Fazilet, ECalculationSource.Semerkand)]
        DuhaQuarterOfDay,
        [SupportedBy(ECalculationSource.None, ECalculationSource.Muwaqqit, ECalculationSource.Fazilet, ECalculationSource.Semerkand)]
        IshaMidnight,
        [SupportedBy(ECalculationSource.None, ECalculationSource.Muwaqqit, ECalculationSource.Fazilet, ECalculationSource.Semerkand)]
        IshaFirstThirdOfNight,
        [SupportedBy(ECalculationSource.None, ECalculationSource.Muwaqqit, ECalculationSource.Fazilet, ECalculationSource.Semerkand)]
        IshaSecondThirdOfNight,
    }
}
