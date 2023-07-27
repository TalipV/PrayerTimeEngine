using PrayerTimeEngine.Code.Common.Enum;

namespace PrayerTimeEngine.Domain.Models
{
    public class PrayerTimesBundle
    {
        public PrayerTime Fajr { get; private set; } = new PrayerTime();
        public PrayerTime Duha { get; private set; } = new PrayerTime();
        public PrayerTime Dhuhr { get; private set; } = new PrayerTime();
        public PrayerTime Asr { get; private set; } = new PrayerTime();
        public PrayerTime Maghrib { get; private set; } = new PrayerTime();
        public PrayerTime Isha { get; private set; } = new PrayerTime();

        public TimeSpan? DayDuratiion
        {
            get
            {
                return Asr?.End - Fajr?.Start;
            }
        }

        public TimeSpan? NightDuratiion
        {
            get
            {
                return Isha?.End - Maghrib?.Start;
            }
        }

        public DateTime? OneThirdDateTime
        {
            get
            {
                if (NightDuratiion == null)
                {
                    return null;
                }

                return Maghrib.Start.Value.Add(TimeSpan.FromMilliseconds(NightDuratiion.Value.TotalMilliseconds * (1.0 / 3.0)));
            }
        }

        public DateTime? TwoThirdsDateTime
        {
            get
            {
                if (NightDuratiion == null)
                {
                    return null;
                }

                return Maghrib.Start.Value.Add(TimeSpan.FromMilliseconds(NightDuratiion.Value.TotalMilliseconds * (2.0 / 3.0)));
            }
        }

        public DateTime? MidnightDateTime
        {
            get
            {
                if (NightDuratiion == null)
                {
                    return null;
                }

                return Maghrib.Start.Value.Add(TimeSpan.FromMilliseconds(NightDuratiion.Value.TotalMilliseconds * (1.0 / 2.0)));
            }
        }

        public DateTime? DuhaQuarterDateTime 
        {
            get
            {
                if (DayDuratiion == null)
                {
                    return null;
                }

                return Fajr.Start.Value.Add(TimeSpan.FromMilliseconds(DayDuratiion.Value.TotalMilliseconds * (1.0 / 4.0)));
            }
        }
        public DateTime? AsrMithlaynDateTime { get; set; }
        public DateTime? IshtibaqDateTime { get; set; }

        public string OneThird => OneThirdDateTime?.ToString("HH:mm:ss") ?? "xx:xx:xx";
        public string Midnight => MidnightDateTime?.ToString("HH:mm:ss") ?? "xx:xx:xx";
        public string TwoThirds => TwoThirdsDateTime?.ToString("HH:mm:ss") ?? "xx:xx:xx";


        public DateTime? FajrGhalasDateTime { get; set; }
        public DateTime? FajrSunriseRednessDateTime { get; set; }
        public DateTime? DuhaHalfDateTime { get; set; }
        public DateTime? AsrKarahaDateTime { get; set; }

        public string FajrGhalas => FajrGhalasDateTime?.ToString("HH:mm:ss") ?? "xx:xx:xx";
        public string FajrSunriseRedness => FajrSunriseRednessDateTime?.ToString("HH:mm:ss") ?? "xx:xx:xx";
        public string DuhaHalf => DuhaHalfDateTime?.ToString("HH:mm:ss") ?? "xx:xx:xx";
        public string AsrKaraha => AsrKarahaDateTime?.ToString("HH:mm:ss") ?? "xx:xx:xx";

        public string DuhaQuarter => DuhaQuarterDateTime?.ToString("HH:mm:ss") ?? "xx:xx:xx";
        public string AsrMithlayn => AsrMithlaynDateTime?.ToString("HH:mm:ss") ?? "xx:xx:xx";
        public string Ishtibaq => IshtibaqDateTime?.ToString("HH:mm:ss") ?? "xx:xx:xx";


        public void SetSpecificPrayerTimeDateTime(ETimeType timeType, DateTime? dateTime)
        {
            switch (timeType)
            {
                case ETimeType.FajrStart:
                    Fajr.Start = dateTime;
                    break;
                case ETimeType.FajrEnd:
                    Fajr.End = dateTime;
                    break;
                case ETimeType.FajrGhalas:
                    FajrGhalasDateTime = dateTime;
                    break;
                case ETimeType.FajrKaraha:
                    FajrSunriseRednessDateTime = dateTime;
                    break;

                case ETimeType.DuhaStart:
                    Duha.Start = dateTime;
                    break;
                case ETimeType.DuhaEnd:
                    Duha.End = dateTime;
                    break;

                case ETimeType.DhuhrStart:
                    Dhuhr.Start = dateTime;
                    break;
                case ETimeType.DhuhrEnd:
                    Dhuhr.End = dateTime;
                    break;

                case ETimeType.AsrStart:
                    Asr.Start = dateTime;
                    break;
                case ETimeType.AsrEnd:
                    Asr.End = dateTime;
                    break;
                case ETimeType.AsrMithlayn:
                    AsrMithlaynDateTime = dateTime;
                    break;
                case ETimeType.AsrKaraha:
                    AsrKarahaDateTime = dateTime;
                    break;

                case ETimeType.MaghribStart:
                    Maghrib.Start = dateTime;
                    break;
                case ETimeType.MaghribEnd:
                    Maghrib.End = dateTime;
                    break;
                case ETimeType.MaghribIshtibaq:
                    IshtibaqDateTime = dateTime;
                    break;

                case ETimeType.IshaStart:
                    Isha.Start = dateTime;
                    break;
                case ETimeType.IshaEnd:
                    Isha.End = dateTime;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}