using MvvmHelpers;
using PrayerTimeEngine.Common.Enums;

namespace PrayerTimeEngine.Domain.Models
{
    public class PrayerTime : ObservableObject
    {
        public EPrayerTime PrayerTimeType { get; private set; }

        private DateTime _start;
        private DateTime _end;

        public PrayerTime(EPrayerTime prayerTimeType)
        {
            PrayerTimeType = prayerTimeType;
        }

        public DateTime Start
        {
            get { return _start; }
            set
            {
                if (SetProperty(ref _start, value))
                {
                    OnPropertyChanged(nameof(Duration));
                }
            }
        }

        public DateTime End
        {
            get { return _end; }
            set
            {
                if (SetProperty(ref _end, value))
                {
                    OnPropertyChanged(nameof(Duration));
                }
            }
        }

        public string Duration => $"{Start:HH:mm tt} - {End:HH:mm tt}";
    }
}
