using System.Collections.Concurrent;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Interfaces;

namespace PrayerTimeEngine.Core.Domain.ProfileManagement.Services;

public class ProfileVersionStore : IProfileVersionStore
{
    private readonly ConcurrentDictionary<int, long> _profileVersions = new();

    public long GetVersion(int profileID)
    {
        return _profileVersions.GetValueOrDefault(profileID, 0L);
    }

    public void BumpVersion(int profileID)
    {
        _profileVersions.AddOrUpdate(profileID, 1L, (_, v) => v + 1L);
    }
}
