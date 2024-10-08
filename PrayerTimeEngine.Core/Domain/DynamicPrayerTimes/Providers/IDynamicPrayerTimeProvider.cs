﻿using NodaTime;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Models;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers;

public interface IDynamicPrayerTimeProvider
{
    public Task<List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)>> GetPrayerTimesAsync(ZonedDateTime date, BaseLocationData locationData, List<GenericSettingConfiguration> configurations, CancellationToken cancellationToken);
    public HashSet<ETimeType> GetUnsupportedTimeTypes();
    public Task<BaseLocationData> GetLocationInfo(ProfilePlaceInfo place, CancellationToken cancellationToken);
}