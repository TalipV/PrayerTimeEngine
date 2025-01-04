using NodaTime;
using PrayerTimeEngine.Core.Common;
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

        IMosqueDailyPrayerTimes times =
            await provider.GetPrayerTimesAsync(
                date.LocalDateTime.Date,
                mosqueProfile.ExternalID,
                cancellationToken).ConfigureAwait(false);

        var prayerTimesCollection = getPrayerTimesCollection(times);
        prayerTimesCollection.DataCalculationTimestamp = systemInfoService.GetCurrentZonedDateTime();

        return prayerTimesCollection;
    }

    private MosquePrayerTimesSet getPrayerTimesCollection(IMosqueDailyPrayerTimes times)
    {
        // TODO fix this. The Profile should know its own time zone!
        var timeZone = systemInfoService.GetSystemTimeZone();

        var prayerTimesCollection = new MosquePrayerTimesSet();

        prayerTimesCollection.Fajr.Start = times.Fajr.On(times.Date).InZoneStrictly(timeZone);
        prayerTimesCollection.Fajr.CongregationStartOffset = (int)Period.Between(times.Fajr, times.FajrCongregation, PeriodUnits.Minutes).Minutes;
        prayerTimesCollection.Fajr.End = times.Shuruq.On(times.Date).InZoneStrictly(timeZone);

        prayerTimesCollection.Dhuhr.Start = times.Dhuhr.On(times.Date).InZoneStrictly(timeZone);
        prayerTimesCollection.Dhuhr.CongregationStartOffset = (int)Period.Between(times.Dhuhr, times.DhuhrCongregation, PeriodUnits.Minutes).Minutes;
        prayerTimesCollection.Dhuhr.End = times.Asr.On(times.Date).InZoneStrictly(timeZone);

        prayerTimesCollection.Asr.Start = times.Asr.On(times.Date).InZoneStrictly(timeZone);
        prayerTimesCollection.Asr.CongregationStartOffset = (int)Period.Between(times.Asr, times.AsrCongregation, PeriodUnits.Minutes).Minutes;
        prayerTimesCollection.Asr.End = times.Maghrib.On(times.Date).InZoneStrictly(timeZone);

        prayerTimesCollection.Maghrib.Start = times.Maghrib.On(times.Date).InZoneStrictly(timeZone);
        prayerTimesCollection.Maghrib.CongregationStartOffset = (int)Period.Between(times.Maghrib, times.MaghribCongregation, PeriodUnits.Minutes).Minutes;
        prayerTimesCollection.Maghrib.End = times.Isha.On(times.Date).InZoneStrictly(timeZone);

        prayerTimesCollection.Isha.Start = times.Isha.On(times.Date).InZoneStrictly(timeZone);
        prayerTimesCollection.Isha.CongregationStartOffset = (int)Period.Between(times.Isha, times.IshaCongregation, PeriodUnits.Minutes).Minutes;

        // TODO fix this. Doesn't make sense.. but let's go with it for now
        prayerTimesCollection.Isha.End = times.Fajr.On(times.Date).InZoneStrictly(timeZone);

        prayerTimesCollection.Jumuah.Start = (times.Jumuah ?? new LocalTime(0, 0)).On(times.Date).InZoneStrictly(timeZone);
        prayerTimesCollection.Jumuah2.Start = (times.Jumuah2 ?? new LocalTime(0, 0)).On(times.Date).InZoneStrictly(timeZone);

        return prayerTimesCollection;
    }

    public async Task<bool> ValidateData(EMosquePrayerTimeProviderType mosquePrayerTimeProviderType, string externalID, CancellationToken cancellationToken)
    {
        IMosquePrayerTimeProvider provider =
            mosquePrayerTimeProviderFactory.GetDynamicPrayerTimeProviderByDynamicPrayerTimeProvider(mosquePrayerTimeProviderType);

        return await provider.ValidateData(externalID, cancellationToken);
    }
}
