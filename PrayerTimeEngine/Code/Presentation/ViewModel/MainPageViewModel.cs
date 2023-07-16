using MvvmHelpers;
using PrayerTimeEngine.Code.DUMMYFOLDER;
using PrayerTimeEngine.Code.Services;
using PrayerTimeEngine.Common.Enums;
using PrayerTimeEngine.Domain.Models;
using System.Windows.Input;

namespace PrayerTimeEngine.Presentation.ViewModels
{
    public class MainPageViewModel : BaseViewModel
    {
        IPrayerTimeCalculationService _prayerTimeCalculationService;

        private PrayerTime _fajrPrayerTime;
        private PrayerTime _duhaPrayerTime;
        private PrayerTime _dhuhrPrayerTime;
        private PrayerTime _asrPrayerTime;
        private PrayerTime _maghribPrayerTime;
        private PrayerTime _ishaPrayerTime;

        public MainPageViewModel(IPrayerTimeCalculationService prayerTimeCalculator)
        {
            _prayerTimeCalculationService = prayerTimeCalculator;

            LoadPrayerTimesButton_ClickCommand = new Command(LoadPrayerTimesButton_Click);

            // In a real application, you would load these from your PrayerTimeRepository

            FajrPrayerTime = new PrayerTime(EPrayerTime.Fajr)
            {
                Start = new DateTime(1, 1, 1, hour: 3, minute: 0, second: 0),
                End = new DateTime(1, 1, 1, hour: 4, minute: 0, second: 0),
            };

            DuhaPrayerTime = new PrayerTime(EPrayerTime.Duha)
            {
                Start = new DateTime(1, 1, 1, hour: 6, minute: 0, second: 0),
                End = new DateTime(1, 1, 1, hour: 11, minute: 0, second: 0),
            };

            DhuhrPrayerTime = new PrayerTime(EPrayerTime.Dhuhr)
            {
                Start = new DateTime(1, 1, 1, hour: 12, minute: 30, second: 0),
                End = new DateTime(1, 1, 1, hour: 16, minute: 0, second: 0),
            };

            AsrPrayerTime = new PrayerTime(EPrayerTime.Asr)
            {
                Start = new DateTime(1, 1, 1, hour: 16, minute: 0, second: 0),
                End = new DateTime(1, 1, 1, hour: 19, minute: 0, second: 0),
            };

            MaghribPrayerTime = new PrayerTime(EPrayerTime.Maghrib)
            {
                Start = new DateTime(1, 1, 1, hour: 19, minute: 0, second: 0),
                End = new DateTime(1, 1, 1, hour: 21, minute: 0, second: 0),
            };

            IshaPrayerTime = new PrayerTime(EPrayerTime.Isha)
            {
                Start = new DateTime(1, 1, 1, hour: 21, minute: 0, second: 0),
                End = new DateTime(1, 1, 2, hour: 3, minute: 0, second: 0),
            };
        }

        public PrayerTime FajrPrayerTime
        {
            get { return _fajrPrayerTime; }
            set { SetProperty(ref _fajrPrayerTime, value); }
        }

        public PrayerTime DuhaPrayerTime
        {
            get { return _duhaPrayerTime; }
            set { SetProperty(ref _duhaPrayerTime, value); }
        }

        public PrayerTime DhuhrPrayerTime
        {
            get { return _dhuhrPrayerTime; }
            set { SetProperty(ref _dhuhrPrayerTime, value); }
        }

        public PrayerTime AsrPrayerTime
        {
            get { return _asrPrayerTime; }
            set { SetProperty(ref _asrPrayerTime, value); }
        }

        public PrayerTime MaghribPrayerTime
        {
            get { return _maghribPrayerTime; }
            set { SetProperty(ref _maghribPrayerTime, value); }
        }

        public PrayerTime IshaPrayerTime
        {
            get { return _ishaPrayerTime; }
            set { SetProperty(ref _ishaPrayerTime, value); }
        }
        public ICommand LoadPrayerTimesButton_ClickCommand { get; }

        private void LoadPrayerTimesButton_Click()
        {
            PrayerTimes prayerTimes = _prayerTimeCalculationService.Execute(PrayerTimesConfiguration.Instance);
            applyPrayerTimesDataToViewModel(prayerTimes);
        }

        private void applyPrayerTimesDataToViewModel(PrayerTimes prayerTimes)
        {
            FajrPrayerTime = prayerTimes.Fajr;
            DuhaPrayerTime = prayerTimes.Duha;
            DhuhrPrayerTime = prayerTimes.Dhuhr;
            AsrPrayerTime = prayerTimes.Asr;
            MaghribPrayerTime = prayerTimes.Maghrib;
            IshaPrayerTime = prayerTimes.Isha;
        }
    }
}
