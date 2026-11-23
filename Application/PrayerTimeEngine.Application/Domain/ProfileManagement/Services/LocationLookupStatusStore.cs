using System.Collections.Concurrent;
using NodaTime;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Interfaces;

namespace PrayerTimeEngine.Core.Domain.ProfileManagement.Services;

public class LocationLookupStatusStore : ILocationLookupStatusStore
{
    private readonly ConcurrentDictionary<(int ProfileID, EDynamicPrayerTimeProviderType Provider), LocationLookupStatus> _statusByKey = new();

    public LocationLookupStatus? Get(int profileID, EDynamicPrayerTimeProviderType provider)
    {
        return _statusByKey.TryGetValue((profileID, provider), out LocationLookupStatus? status) ? status : null;
    }

    public void Record(int profileID, EDynamicPrayerTimeProviderType provider, bool isPlaceUnknown, Exception? error, Instant attemptedAt)
    {
        _statusByKey.AddOrUpdate(
            (profileID, provider),
            _ => new LocationLookupStatus(attemptedAt, error is null ? 0 : 1, isPlaceUnknown, error),
            (_, previous) => new LocationLookupStatus(
                attemptedAt,
                error is null ? 0 : previous.ConsecutiveFailureCount + 1,
                isPlaceUnknown,
                error));
    }

    public void Clear(int profileID)
    {
        foreach ((int ProfileID, EDynamicPrayerTimeProviderType Provider) key in _statusByKey.Keys.Where(x => x.ProfileID == profileID))
        {
            _statusByKey.TryRemove(key, out _);
        }
    }
}
