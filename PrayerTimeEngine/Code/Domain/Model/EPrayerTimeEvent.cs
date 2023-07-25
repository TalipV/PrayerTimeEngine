namespace PrayerTimeEngine.Code.Domain.Model
{
    public enum EPrayerTimeEvent
    {
        Start,
        End,

        // ### ADDITIONAL TIMES
        Fajr_Fadilah,
        Fajr_Karaha,
        AsrMithlayn,
        Asr_Karaha,
        IshtibaqAnNujum,

        // ### IMPLICIT TIMES
        DuhaQuarterOfDay,
        IshaMidnight,
        IshaFirstThirdOfNight,
        IshaSecondThirdOfNight,
    }
}
