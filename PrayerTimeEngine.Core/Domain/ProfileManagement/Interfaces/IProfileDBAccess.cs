using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;

namespace PrayerTimeEngine.Core.Domain.ProfileManagement.Interfaces
{
    public interface IProfileDBAccess
    {
        public Task<List<Profile>> GetProfiles(CancellationToken cancellationToken);
        public Task<Profile> GetUntrackedReferenceOfProfile(int profileID, CancellationToken cancellationToken);
        public Task SaveProfile(Profile profile, CancellationToken cancellationToken);

        public Task UpdateLocationConfig(Profile profile, string locationName, List<(ECalculationSource CalculationSource, BaseLocationData LocationData)> locationDataByCalculationSource, CancellationToken cancellationToken);
        public Task UpdateTimeConfig(Profile profile, ETimeType timeType, GenericSettingConfiguration settings, CancellationToken cancellationToken);
    }
}
