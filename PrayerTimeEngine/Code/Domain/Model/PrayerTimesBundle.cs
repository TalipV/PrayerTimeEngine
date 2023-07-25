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


        public void SetSpecificPrayerTimeDateTime(EPrayerTime prayerTime, EPrayerTimeEvent prayerTimeEvent, DateTime? dateTime)
        {
            switch (prayerTime)
            {
                case EPrayerTime.Fajr:
                    if (prayerTimeEvent == EPrayerTimeEvent.Start)
                        Fajr.Start = dateTime;
                    else if (prayerTimeEvent == EPrayerTimeEvent.End)
                        Fajr.End = dateTime;
                    else if (prayerTimeEvent == EPrayerTimeEvent.Fajr_Fadilah)
                        FajrGhalasDateTime = dateTime;
                    else if (prayerTimeEvent == EPrayerTimeEvent.Fajr_Karaha)
                        FajrSunriseRednessDateTime = dateTime;
                    break;
                case EPrayerTime.Duha:
                    if (prayerTimeEvent == EPrayerTimeEvent.Start)
                        Duha.Start = dateTime;
                    else if (prayerTimeEvent == EPrayerTimeEvent.End)
                        Duha.End = dateTime;
                    break;
                case EPrayerTime.Dhuhr:
                    if (prayerTimeEvent == EPrayerTimeEvent.Start)
                        Dhuhr.Start = dateTime;
                    else if (prayerTimeEvent == EPrayerTimeEvent.End)
                        Dhuhr.End = dateTime;
                    break;
                case EPrayerTime.Asr:
                    if (prayerTimeEvent == EPrayerTimeEvent.Start)
                        Asr.Start = dateTime;
                    else if (prayerTimeEvent == EPrayerTimeEvent.End)
                        Asr.End = dateTime;
                    else if (prayerTimeEvent == EPrayerTimeEvent.AsrMithlayn)
                        AsrMithlaynDateTime = dateTime;
                    else if (prayerTimeEvent == EPrayerTimeEvent.Asr_Karaha)
                        AsrKarahaDateTime = dateTime;
                    break;
                case EPrayerTime.Maghrib:
                    if (prayerTimeEvent == EPrayerTimeEvent.Start)
                        Maghrib.Start = dateTime;
                    else if (prayerTimeEvent == EPrayerTimeEvent.End)
                        Maghrib.End = dateTime;
                    else if (prayerTimeEvent == EPrayerTimeEvent.IshtibaqAnNujum)
                        IshtibaqDateTime = dateTime;
                    break;
                case EPrayerTime.Isha:
                    if (prayerTimeEvent == EPrayerTimeEvent.Start)
                        Isha.Start = dateTime;
                    else if (prayerTimeEvent == EPrayerTimeEvent.End)
                        Isha.End = dateTime;
                    break;
                default:
                    throw new NotImplementedException();
            }

        }
    }
}