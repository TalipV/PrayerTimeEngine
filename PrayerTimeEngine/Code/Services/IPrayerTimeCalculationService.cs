using PrayerTimeEngine.Code.DUMMYFOLDER;
using PrayerTimeEngine.Domain.Models;

namespace PrayerTimeEngine.Code.Services
{
    public interface IPrayerTimeCalculationService
    {
        public PrayerTimes Execute(PrayerTimesConfiguration configuration);
    }
}
