using NodaTime;
using PrayerTimeEngine.Core.Common;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Models;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;

namespace PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Management;

public class MosquePrayerTimeProviderManager(
        IMosquePrayerTimeProviderFactory mosquePrayerTimeProviderFactory,
        IProfileService profileService,
        ISystemInfoService systemInfoService
    ) : IMosquePrayerTimeProviderManager
{
    public async Task<MosquePrayerTimesSet> CalculatePrayerTimesAsync(int profileID, ZonedDateTime date, CancellationToken cancellationToken)
    {
        date = date.LocalDateTime.Date.AtStartOfDayInZone(date.Zone);

        Profile profile = await profileService.GetUntrackedReferenceOfProfile(profileID, cancellationToken).ConfigureAwait(false)
            ?? throw new Exception($"The Profile with the ID '{profileID}' could not be found");

        MosqueProfile mosqueProfile = profile as MosqueProfile
            ?? throw new Exception($"The Profile with the ID '{profileID}' is not a {nameof(MosqueProfile)}");

        IMosquePrayerTimeProvider provider = 
            mosquePrayerTimeProviderFactory.GetDynamicPrayerTimeProviderByDynamicPrayerTimeProvider(mosqueProfile.MosqueProviderType);
        
        IMosquePrayerTimes times = 
            await provider.GetPrayerTimesAsync(
                date.LocalDateTime.Date,
                mosqueProfile.ExternalID, 
                cancellationToken).ConfigureAwait(false);

        var prayerTimesCollection = getPrayerTimesCollection(times);
        prayerTimesCollection.DataCalculationTimestamp = systemInfoService.GetCurrentZonedDateTime();

        return prayerTimesCollection;
    }

    private static MosquePrayerTimesSet getPrayerTimesCollection(IMosquePrayerTimes times)
    {
        var prayerTimesCollection = new MosquePrayerTimesSet();

        prayerTimesCollection.Fajr.Start = times.Fajr;
        prayerTimesCollection.Fajr.End = times.Shuruq;

        prayerTimesCollection.Dhuhr.Start = times.Dhuhr;
        prayerTimesCollection.Dhuhr.End = times.Asr;

        prayerTimesCollection.Asr.Start = times.Asr;
        prayerTimesCollection.Asr.End = times.Maghrib;

        prayerTimesCollection.Maghrib.Start = times.Maghrib;
        prayerTimesCollection.Maghrib.End = times.Isha;

        prayerTimesCollection.Isha.Start = times.Isha;
        //prayerTimesCollection.Isha.End = times.Fajr;

        prayerTimesCollection.Jumuah.Start = times.Jumuah ?? new LocalTime(0, 0);
        prayerTimesCollection.Jumuah2.Start = times.Jumuah2 ?? new LocalTime(0, 0);

        return prayerTimesCollection;
    }

    public async Task<bool> ValidateData(EMosquePrayerTimeProviderType mosquePrayerTimeProviderType, string externalID, CancellationToken cancellationToken)
    {
        IMosquePrayerTimeProvider provider = 
            mosquePrayerTimeProviderFactory.GetDynamicPrayerTimeProviderByDynamicPrayerTimeProvider(mosquePrayerTimeProviderType);

        return await provider.ValidateData(externalID, cancellationToken);
    }
}
