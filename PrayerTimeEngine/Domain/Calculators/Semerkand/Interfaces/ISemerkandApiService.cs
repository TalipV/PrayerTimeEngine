using PrayerTimeEngine.Domain.Calculators.Semerkand.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrayerTimeEngine.Domain.Calculators.Semerkand.Interfaces
{
    public interface ISemerkandApiService
    {
        public Task<Dictionary<string, int>> GetCountries();
        public Task<Dictionary<string, int>> GetCitiesByCountryID(int countryID);
        public Task<List<SemerkandPrayerTimes>> GetTimesByCityID(DateTime date, int cityID);
    }
}
