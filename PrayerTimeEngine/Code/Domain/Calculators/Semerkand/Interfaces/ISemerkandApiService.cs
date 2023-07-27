using PrayerTimeEngine.Code.Domain.Calculator.Semerkand.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrayerTimeEngine.Code.Domain.Calculators.Semerkand.Interfaces
{
    public interface ISemerkandApiService
    {
        public Task<Dictionary<string, int>> GetCountries();
        public Task<Dictionary<string, int>> GetCitiesByCountryID(int countryID);
        public Task<List<SemerkandPrayerTimes>> GetTimesByCityID(DateTime date, int cityID);
    }
}
