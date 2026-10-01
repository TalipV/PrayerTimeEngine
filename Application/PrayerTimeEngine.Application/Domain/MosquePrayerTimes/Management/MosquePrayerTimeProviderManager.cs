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
    public async Task<MosquePrayerTimesDay> CalculatePrayerTimesAsync(int profileID, ZonedDateTime date, CancellationToken cancellationToken)
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

        MosquePrayerTimesDay prayerTimesCollection = GetPrayerTimesCollection(times);
        prayerTimesCollection.DataCalculationTimestamp = systemInfoService.GetCurrentZonedDateTime();

        return prayerTimesCollection;
    }

    private MosquePrayerTimesDay GetPrayerTimesCollection(IMosqueDailyPrayerTimes times)
    {
        // TODO fix this. The Profile should know its own time zone!
        DateTimeZone timeZone = systemInfoService.GetSystemTimeZone();

        var prayerTimesCollection = new MosquePrayerTimesDay();

        // missing values just lead to missing times
        ZonedDateTime? GetZonedDateTimeOrNull(LocalTime? time)
        {
            return time?.On(times.Date).InZoneStrictly(timeZone);
        }

        static int GetCongregationStartOffset(LocalTime? start, LocalTime? congregationStart)
        {
            if (start is null || congregationStart is null)
            {
                return 0;
            }

            return (int)Period.Between(start.Value, congregationStart.Value, PeriodUnits.Minutes).Minutes;
        }

        prayerTimesCollection.Fajr.Start = GetZonedDateTimeOrNull(times.Fajr);
        prayerTimesCollection.Fajr.CongregationStartOffset = GetCongregationStartOffset(times.Fajr, times.FajrCongregation);
        prayerTimesCollection.Fajr.End = GetZonedDateTimeOrNull(times.Shuruq);

        prayerTimesCollection.Dhuhr.Start = GetZonedDateTimeOrNull(times.Dhuhr);
        prayerTimesCollection.Dhuhr.CongregationStartOffset = GetCongregationStartOffset(times.Dhuhr, times.DhuhrCongregation);
        prayerTimesCollection.Dhuhr.End = GetZonedDateTimeOrNull(times.Asr);

        prayerTimesCollection.Asr.Start = GetZonedDateTimeOrNull(times.Asr);
        prayerTimesCollection.Asr.CongregationStartOffset = GetCongregationStartOffset(times.Asr, times.AsrCongregation);
        prayerTimesCollection.Asr.End = GetZonedDateTimeOrNull(times.Maghrib);

        prayerTimesCollection.Maghrib.Start = GetZonedDateTimeOrNull(times.Maghrib);
        prayerTimesCollection.Maghrib.CongregationStartOffset = GetCongregationStartOffset(times.Maghrib, times.MaghribCongregation);
        prayerTimesCollection.Maghrib.End = GetZonedDateTimeOrNull(times.Isha);

        prayerTimesCollection.Isha.Start = GetZonedDateTimeOrNull(times.Isha);
        prayerTimesCollection.Isha.CongregationStartOffset = GetCongregationStartOffset(times.Isha, times.IshaCongregation);

        // TODO fix this. Doesn't make sense.. but let's go with it for now
        prayerTimesCollection.Isha.End = GetZonedDateTimeOrNull(times.Fajr);

        prayerTimesCollection.Jumuah.Start = GetZonedDateTimeOrNull(times.Jumuah);
        prayerTimesCollection.Jumuah2.Start = GetZonedDateTimeOrNull(times.Jumuah2);

        return prayerTimesCollection;
    }

    public async Task<bool> ValidateData(EMosquePrayerTimeProviderType mosquePrayerTimeProviderType, string externalID, CancellationToken cancellationToken)
    {
        IMosquePrayerTimeProvider provider =
            mosquePrayerTimeProviderFactory.GetDynamicPrayerTimeProviderByDynamicPrayerTimeProvider(mosquePrayerTimeProviderType);

        return await provider.ValidateData(externalID, cancellationToken);
    }
}
