using NodaTime;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Models;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Providers;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;

namespace PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Management;

public class MosquePrayerTimeProviderManager(
        IMosquePrayerTimeProviderFactory mosquePrayerTimeProviderFactory,
        IProfileService profileService
    ) : IMosquePrayerTimeProviderManager
{
    public async Task<PrayerTimesCollection> CalculatePrayerTimesAsync(int profileID, ZonedDateTime date, CancellationToken cancellationToken)
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

        return getPrayerTimesCollection(times);
    }

    private static PrayerTimesCollection getPrayerTimesCollection(IMosquePrayerTimes times)
    {
        var prayerTimesCollection = new PrayerTimesCollection();

        prayerTimesCollection.Fajr.Start = (times.Date + times.Fajr).InZoneStrictly(DateTimeZone.Utc);
        prayerTimesCollection.Fajr.End = (times.Date + times.Shuruq).InZoneStrictly(DateTimeZone.Utc);

        prayerTimesCollection.Dhuhr.Start = (times.Date + times.Dhuhr).InZoneStrictly(DateTimeZone.Utc);
        prayerTimesCollection.Dhuhr.End = (times.Date + times.Asr).InZoneStrictly(DateTimeZone.Utc);

        prayerTimesCollection.Asr.Start = (times.Date + times.Asr).InZoneStrictly(DateTimeZone.Utc);
        prayerTimesCollection.Asr.End = (times.Date + times.Maghrib).InZoneStrictly(DateTimeZone.Utc);

        prayerTimesCollection.Maghrib.Start = (times.Date + times.Maghrib).InZoneStrictly(DateTimeZone.Utc);
        prayerTimesCollection.Maghrib.End = (times.Date + times.Isha).InZoneStrictly(DateTimeZone.Utc);

        prayerTimesCollection.Isha.Start = (times.Date + times.Isha).InZoneStrictly(DateTimeZone.Utc);
        //prayerTimesCollection.Isha.End = (times.Date + times.Fajr).InZoneStrictly(DateTimeZone.Utc);

        return prayerTimesCollection;
    }

    public async Task<bool> ValidateData(EMosquePrayerTimeProviderType mosquePrayerTimeProviderType, string externalID, CancellationToken cancellationToken)
    {
        IMosquePrayerTimeProvider provider = 
            mosquePrayerTimeProviderFactory.GetDynamicPrayerTimeProviderByDynamicPrayerTimeProvider(mosquePrayerTimeProviderType);

        return await provider.ValidateData(externalID, cancellationToken);
    }
}
