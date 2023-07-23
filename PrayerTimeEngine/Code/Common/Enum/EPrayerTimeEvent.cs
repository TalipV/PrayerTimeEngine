namespace PrayerTimeEngine.Code.Common.Enums
{
    public enum EPrayerTimeEvent
    {
        Start,
        End,

        // ### ADDITIONAL TIMES
        FajrGhalasEnd,
        FajrSunriseRedness,
        AsrMithlayn,
        AsrKaraha,
        MaghribIshtibaq,

        // ### IMPLICIT TIMES
        DuhaQuarterOfDay,
        IshaMidnight,          
        IshaFirstThirdOfNight,   
        IshaSecondThirdOfNight,
    }
}
