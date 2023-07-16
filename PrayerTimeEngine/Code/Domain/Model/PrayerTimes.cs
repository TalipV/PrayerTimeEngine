using MvvmHelpers;
using PrayerTimeEngine.Common.Enums;

namespace PrayerTimeEngine.Domain.Models
{
    public class PrayerTimes
    {
        public PrayerTime Fajr { get; private set; } = new PrayerTime(EPrayerTime.Fajr);
        public PrayerTime Duha { get; private set; } = new PrayerTime(EPrayerTime.Duha);
        public PrayerTime Dhuhr { get; private set; } = new PrayerTime(EPrayerTime.Dhuhr);
        public PrayerTime Asr { get; private set; } = new PrayerTime(EPrayerTime.Asr);
        public PrayerTime Maghrib { get; private set; } = new PrayerTime(EPrayerTime.Maghrib);
        public PrayerTime Isha { get; private set; } = new PrayerTime(EPrayerTime.Isha);

        public void SetSpecificPrayerTimeDateTime(EPrayerTime prayerTime, EPrayerTimeEvent prayerTimeEvent, DateTime dateTime)
        {
            switch (prayerTime)
            {
                case EPrayerTime.Fajr:
                    if (prayerTimeEvent == EPrayerTimeEvent.Start)
                        Fajr.Start = dateTime;
                    else
                        Fajr.End = dateTime;
                    break;
                case EPrayerTime.Duha:
                    if (prayerTimeEvent == EPrayerTimeEvent.Start)
                        Duha.Start = dateTime;
                    else
                        Duha.End = dateTime;
                    break;
                case EPrayerTime.Dhuhr:
                    if (prayerTimeEvent == EPrayerTimeEvent.Start)
                        Dhuhr.Start = dateTime;
                    else
                        Dhuhr.End = dateTime;
                    break;
                case EPrayerTime.Asr:
                    if (prayerTimeEvent == EPrayerTimeEvent.Start)
                        Asr.Start = dateTime;
                    else
                        Asr.End = dateTime;
                    break;
                case EPrayerTime.Maghrib:
                    if (prayerTimeEvent == EPrayerTimeEvent.Start)
                        Maghrib.Start = dateTime;
                    else
                        Maghrib.End = dateTime;
                    break;
                case EPrayerTime.Isha:
                    if (prayerTimeEvent == EPrayerTimeEvent.Start)
                        Isha.Start = dateTime;
                    else
                        Isha.End = dateTime;
                    break;
                default:
                    throw new NotImplementedException();
            }

        }
    }
}