using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrayerTimeEngine.Code.Domain.Calculator.Fazilet.Models
{
    public class FaziletPrayerTimes
    {
        public FaziletPrayerTimes(int cityID, DateTime imsak, DateTime fajr, DateTime shuruq, DateTime dhuhr, DateTime asr, DateTime maghrib, DateTime isha, DateTime? date = null)
        {
            Date = date ?? dhuhr.Date;
            CityID = cityID;

            Imsak = imsak;
            Fajr = fajr;
            Shuruq = shuruq;
            Dhuhr = dhuhr;
            Asr = asr;
            Maghrib = maghrib;
            Isha = isha;
        }

        public DateTime Date { get; set; }
        public int CityID { get; set; }

        public DateTime Imsak { get; set; }
        public DateTime Fajr { get; set; }
        public DateTime? NextFajr { get; set; }
        public DateTime Shuruq { get; set; }
        public DateTime Dhuhr { get; set; }
        public DateTime Asr { get; set; }
        public DateTime Maghrib { get; set; }
        public DateTime Isha { get; set; }
    }
}
