using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrayerTimeEngine.Code.DUMMYFOLDER
{
    public class MuwaqqitCalculationConfigurationFajrIshaStart : MuwaqqitCalculationConfiguration
    {
        public MuwaqqitCalculationConfigurationFajrIshaStart(decimal longitude, decimal latitude, decimal degree) : base(longitude, latitude)
        {
            Degree = degree;
        }

        public decimal Degree { get; set; }
    }
}
