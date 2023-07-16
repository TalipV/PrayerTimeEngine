namespace PrayerTimeEngine.Code.Domain.Muwaqqit.Models
{
    public class MuwaqqitResponse
    {
        public string fajr { get; set; }
        public string sunrise { get; set; }
        public string zohr { get; set; }
        public string asr_shafi { get; set; }
        public string asr_hanafi { get; set; }
        public string sunset { get; set; }
        public string esha { get; set; }
        
        /// <summary>
        /// Longitude
        /// </summary>
        public decimal ln { get; set; }

        /// <summary>
        /// Latitude
        /// </summary>
        public decimal lt { get; set; }

        /// <summary>
        /// Isha-degree
        /// </summary>
        public decimal ea { get; set; }

        /// <summary>
        /// Fajr degree
        /// </summary>
        public decimal fa { get; set; }

        /// <summary>
        /// Timezone as string, like "Europe/Vienna"
        /// </summary>
        public string tz { get; set; }

        /// <summary>
        /// Date of the data
        /// </summary>
        public string d { get; set; }
    }

}
