using PrayerTimeEngine.Code.Domain.Muwaqqit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrayerTimeEngine.Code.Domain.Muwaqqit.Interfaces
{
    public interface IMuwaqqitDBAccess
    {
        MuwaqqitPrayerTimes GetTimes(DateTime date, decimal longitude, decimal latitude, decimal fajrDegree, decimal ishaDegree);
        void InsertMuwaqqitPrayerTimes(DateTime date, decimal longitude, decimal latitude, decimal fajrDegree, decimal ishaDegree, MuwaqqitPrayerTimes prayerTimes);
    }
}
