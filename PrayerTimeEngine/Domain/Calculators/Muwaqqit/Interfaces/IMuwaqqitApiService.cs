using PrayerTimeEngine.Domain.Calculators.Muwaqqit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrayerTimeEngine.Domain.Calculators.Muwaqqit.Interfaces
{
    public interface IMuwaqqitApiService
    {
        Task<MuwaqqitPrayerTimes> GetTimesAsync(DateTime date, decimal longitude, decimal latitude, double fajrDegree, double ishaDegree, double ishtibaqDegree, double asrKarahaDegree, string timezone);
    }
}
