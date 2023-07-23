using PrayerTimeEngine.Code.Common.Enums;
using PrayerTimeEngine.Code.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrayerTimeEngine.Code.Domain.Muwaqqit.Models
{
    public class MuwaqqitDegreeCalculationConfiguration : MuwaqqitCalculationConfiguration, IDegreeCalculationConfiguration
    {
        public static readonly Dictionary<(EPrayerTime, EPrayerTimeEvent), double> DegreePrayerTimeEvents =
            new Dictionary<(EPrayerTime, EPrayerTimeEvent), double>
            {
                [(EPrayerTime.Fajr, EPrayerTimeEvent.Start)] = -12.0,
                [(EPrayerTime.Fajr, EPrayerTimeEvent.FajrGhalasEnd)] = -7.0,
                [(EPrayerTime.Fajr, EPrayerTimeEvent.FajrSunriseRedness)] = -3.0,

                [(EPrayerTime.Duha, EPrayerTimeEvent.Start)] = 5.0,
                [(EPrayerTime.Asr, EPrayerTimeEvent.AsrKaraha)] = 5.0,

                [(EPrayerTime.Maghrib, EPrayerTimeEvent.MaghribIshtibaq)] = -10.0,
                [(EPrayerTime.Maghrib, EPrayerTimeEvent.End)] = -12.0,

                [(EPrayerTime.Isha, EPrayerTimeEvent.Start)] = -15.0,
                [(EPrayerTime.Isha, EPrayerTimeEvent.End)] = -15.0,
            };

        public MuwaqqitDegreeCalculationConfiguration(
            int minuteAdjustment, 
            decimal longitude, decimal latitude, 
            string timezone,
            double degree) : base(minuteAdjustment, longitude, latitude, timezone)
        {
            Degree = degree;
        }

        public double Degree { get; set; }
    }
}
