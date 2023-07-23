using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrayerTimeEngine.Code.Domain.Muwaqqit.Models
{
    public class MuwaqqitPrayerTimes
    {
        public MuwaqqitPrayerTimes(
            DateTime date,
            decimal longitude,
            decimal latitude,
            DateTime fajr,
            DateTime nextFajr,
            DateTime shuruq,
            DateTime duha,
            DateTime dhuhr,
            DateTime asrMithl,
            DateTime asrMithlayn,
            DateTime maghrib,
            DateTime isha,
            DateTime ishtibaq,
            DateTime asrKaraha)
        {
            Date = date;
            Longitude = longitude;
            Latitude = latitude;
            Fajr = fajr;
            NextFajr = nextFajr;
            Shuruq = shuruq;
            Duha = duha;
            Dhuhr = dhuhr;
            AsrMithl = asrMithl;
            AsrMithlayn = asrMithlayn;
            Maghrib = maghrib;
            Isha = isha;
            Ishtibaq = ishtibaq;
            AsrKaraha = asrKaraha;
        }

        public DateTime Date { get; set; }
        public decimal Longitude { get; set; }
        public decimal Latitude { get; set; }

        public DateTime Fajr { get; set; }
        public DateTime NextFajr { get; set; }
        public DateTime Shuruq { get; set; }
        public DateTime Duha { get; set; }
        public DateTime Dhuhr { get; set; }
        public DateTime AsrMithl { get; set; }
        public DateTime AsrMithlayn { get; set; }
        public DateTime Maghrib { get; set; }
        public DateTime Isha { get; set; }
        public DateTime Ishtibaq { get; set; }
        public DateTime AsrKaraha { get; set; }
    }
}
