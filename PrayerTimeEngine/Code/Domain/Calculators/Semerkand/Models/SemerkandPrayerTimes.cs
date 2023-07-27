using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrayerTimeEngine.Code.Domain.Calculator.Semerkand.Models
{
    public class SemerkandPrayerTimes
    {
        public int CityID { get; set; } = -1;

        [JsonProperty("DayOfYear")]
        public int DayOfYear { get; set; }

        public DateTime Date { get; set; }

        [JsonProperty("Fajr")]
        public DateTime Fajr { get; set; }
        public DateTime? NextFajr { get; set; }

        [JsonProperty("Tulu")]
        public DateTime Tulu { get; set; }

        [JsonProperty("Zuhr")]
        public DateTime Zuhr { get; set; }

        [JsonProperty("Asr")]
        public DateTime Asr { get; set; }

        [JsonProperty("Maghrib")]
        public DateTime Maghrib { get; set; }

        [JsonProperty("Isha")]
        public DateTime Isha { get; set; }
    }
}
