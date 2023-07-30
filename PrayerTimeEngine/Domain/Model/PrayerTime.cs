namespace PrayerTimeEngine.Domain.Model
{
    public class PrayerTime
    {
        public string Name { get; set; }

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
