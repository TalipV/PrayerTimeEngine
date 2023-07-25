using PrayerTimeEngine.Code.Common.Enum;
using PrayerTimeEngine.Code.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrayerTimeEngine.Code.Domain.Muwaqqit.Models
{
    public class MuwaqqitDegreeCalculationConfiguration : MuwaqqitCalculationConfiguration
    {
        public static readonly Dictionary<(EPrayerTime, EPrayerTimeEvent), double> DegreePrayerTimeEvents =
            new Dictionary<(EPrayerTime, EPrayerTimeEvent), double>
            {
                [(EPrayerTime.Fajr, EPrayerTimeEvent.Start)] = -12.0,
                [(EPrayerTime.Fajr, EPrayerTimeEvent.Fajr_Fadilah)] = -7.0,
                [(EPrayerTime.Fajr, EPrayerTimeEvent.Fajr_Karaha)] = -3.0,

                [(EPrayerTime.Duha, EPrayerTimeEvent.Start)] = 5.0,
                [(EPrayerTime.Asr, EPrayerTimeEvent.Asr_Karaha)] = 5.0,

                [(EPrayerTime.Maghrib, EPrayerTimeEvent.IshtibaqAnNujum)] = -10.0,
                [(EPrayerTime.Maghrib, EPrayerTimeEvent.End)] = -12.0,

                [(EPrayerTime.Isha, EPrayerTimeEvent.Start)] = -15.0,
                [(EPrayerTime.Isha, EPrayerTimeEvent.End)] = -15.0,
            };

        public MuwaqqitDegreeCalculationConfiguration(
            int minuteAdjustment, double degree, bool isTimeShown = true) : base(minuteAdjustment, isTimeShown)
        {
            Degree = degree;
        }

        public double Degree { get; }
    }
}
