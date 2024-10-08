﻿using NodaTime;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Models;

namespace PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers;

public interface IMosquePrayerTimeProvider
{
    Task<IMosqueDailyPrayerTimes> GetPrayerTimesAsync(LocalDate date, string externalID, CancellationToken cancellationToken);
    Task<bool> ValidateData(string externalID, CancellationToken cancellationToken);
}