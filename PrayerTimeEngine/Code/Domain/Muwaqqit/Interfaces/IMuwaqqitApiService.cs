using PrayerTimeEngine.Code.Domain.Muwaqqit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrayerTimeEngine.Code.Domain.Muwaqqit.Interfaces
{
    public interface IMuwaqqitApiService
    {
        MuwaqqitPrayerTimes GetTimes(DateTime date, decimal longitude, decimal latitude, decimal fajrDegree, decimal ishaDegree, string timezone);
    }
}
