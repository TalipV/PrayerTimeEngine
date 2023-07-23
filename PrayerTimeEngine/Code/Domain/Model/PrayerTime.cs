using PrayerTimeEngine.Code.Common.Enums;

namespace PrayerTimeEngine.Domain.Models
{
    public class PrayerTime
    {
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }

        public string DurationDisplayText
        {
            get
            {
                string startTime = Start?.ToString("HH:mm:ss") ?? "xx:xx:xx";
                string endTime = End?.ToString("HH:mm:ss") ?? "xx:xx:xx";

                return $"{startTime} - {endTime}"; ;
            }
        }
    }
}
