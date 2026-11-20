using AsyncKeyedLock;
using NodaTime;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Waqti.Interfaces;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Waqti.Models;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Waqti.Models.DTOs;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Waqti.Models.Entities;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Models;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Waqti.Services;

/// <summary>
/// Prayer times from the calculation backend of waqti.app. The backend always answers with a
/// whole month, so one request fills the local cache for every day of that month.
/// </summary>
/// <remarks>
/// Waqti has no own values for Ghalas, Karaha (morning and afternoon) and Ishtibak. Like with
/// Muwaqqit they are derived from other times calculated for their sun angle: Fajr with the angle
/// of Ghalas / Karaha and Isha with the angle of Asr Karaha / Ishtibak. Each derived angle needs an
/// own request, but one request can carry a Fajr and an Isha angle at the same time.
/// </remarks>
public class WaqtiDynamicPrayerTimeProvider(
        IWaqtiRepository waqtiRepository,
        IWaqtiApiService waqtiApiService
    ) : IDynamicPrayerTimeProvider
{
    private readonly record struct RequestKey(string Method, double? FajrAngle, double? IshaaAngle);

    public async Task<List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)>> GetPrayerTimesAsync(
        ZonedDateTime date,
        BaseLocationData locationData,
        List<GenericSettingConfiguration> configurations,
        CancellationToken cancellationToken)
    {
        if (locationData is not WaqtiLocationData waqtiLocationData)
        {
            throw new Exception("Waqti specific location information was not provided!");
        }

        var result = new List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)>();

        foreach ((RequestKey requestKey, List<ETimeType> timeTypes) in GroupByRequest(configurations))
        {
            WaqtiDailyPrayerTimes? prayerTimes =
                await GetPrayerTimesInternal(date.Date, waqtiLocationData, requestKey, cancellationToken).ConfigureAwait(false);

            if (prayerTimes is null)
            {
                continue;
            }

            foreach (ETimeType timeType in timeTypes)
            {
                // missing times are simply not returned
                if (prayerTimes.GetZonedDateTimeForTimeType(timeType) is ZonedDateTime zonedDateTime)
                {
                    result.Add((timeType, zonedDateTime));
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Distributes the time types over as few requests as possible: per method one request for
    /// the regular times and, if needed, requests which each carry one derived Fajr angle and one
    /// derived Isha angle.
    /// </summary>
    private static List<(RequestKey RequestKey, List<ETimeType> TimeTypes)> GroupByRequest(List<GenericSettingConfiguration> configurations)
    {
        var requests = new List<(RequestKey RequestKey, List<ETimeType> TimeTypes)>();

        foreach (IGrouping<string, GenericSettingConfiguration> methodGroup in configurations.GroupBy(GetMethod))
        {
            string method = methodGroup.Key;

            List<ETimeType> regularTimeTypes =
                [.. methodGroup
                    .Where(x => !WaqtiCalculationConfiguration.DEGREE_TIME_TYPES.Contains(x.TimeType))
                    .Select(x => x.TimeType)];

            if (regularTimeTypes.Count != 0)
            {
                requests.Add((new RequestKey(method, null, null), regularTimeTypes));
            }

            // the sun angles of the settings use the Muwaqqit convention (negative: below the
            // horizon), Waqti expects the depression below the horizon (positive: below)
            List<IGrouping<double, ETimeType>> fajrAngles = GetAngleGroups(methodGroup, WaqtiCalculationConfiguration.FAJR_DEGREE_TIME_TYPES);
            List<IGrouping<double, ETimeType>> ishaaAngles = GetAngleGroups(methodGroup, WaqtiCalculationConfiguration.ISHA_DEGREE_TIME_TYPES);

            for (int i = 0; i < Math.Max(fajrAngles.Count, ishaaAngles.Count); i++)
            {
                IGrouping<double, ETimeType>? fajrAngle = i < fajrAngles.Count ? fajrAngles[i] : null;
                IGrouping<double, ETimeType>? ishaaAngle = i < ishaaAngles.Count ? ishaaAngles[i] : null;

                requests.Add((
                    new RequestKey(method, fajrAngle?.Key, ishaaAngle?.Key),
                    [.. fajrAngle ?? Enumerable.Empty<ETimeType>(), .. ishaaAngle ?? Enumerable.Empty<ETimeType>()]));
            }
        }

        return requests;
    }

    private static List<IGrouping<double, ETimeType>> GetAngleGroups(IEnumerable<GenericSettingConfiguration> configurations, IReadOnlySet<ETimeType> timeTypes)
    {
        return
            [.. configurations
                .Where(x => timeTypes.Contains(x.TimeType))
                .GroupBy(x => -GetDegree(x), x => x.TimeType)
                .OrderBy(x => x.Key)];
    }

    private static double GetDegree(GenericSettingConfiguration configuration)
    {
        if (configuration is WaqtiCalculationConfiguration { Degree: double degree })
        {
            return degree;
        }

        throw new ArgumentException($"Time {configuration.TimeType} requires a {nameof(WaqtiCalculationConfiguration)} with a {nameof(WaqtiCalculationConfiguration.Degree)}.");
    }

    private static string GetMethod(GenericSettingConfiguration configuration)
    {
        if (configuration is WaqtiCalculationConfiguration { Method: string method } && !string.IsNullOrWhiteSpace(method))
        {
            return method;
        }

        return WaqtiMethods.DEFAULT_METHOD;
    }

    public HashSet<ETimeType> GetUnsupportedTimeTypes()
    {
        return [];
    }

    public Task<BaseLocationData> GetLocationInfo(ProfilePlaceInfo place, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(place);

        return Task.FromResult<BaseLocationData>(new WaqtiLocationData
        {
            Longitude = place.Longitude,
            Latitude = place.Latitude,
            TimezoneName = place.TimezoneInfo.Name
        });
    }

    private static readonly AsyncKeyedLocker<(int year, int month, decimal longitude, decimal latitude, RequestKey requestKey, string timezone)> s_getPrayerTimesLocker = new(o =>
    {
        o.PoolSize = 20;
        o.PoolInitialFill = 1;
    });

    private async Task<WaqtiDailyPrayerTimes?> GetPrayerTimesInternal(
        LocalDate date,
        WaqtiLocationData locationData,
        RequestKey requestKey,
        CancellationToken cancellationToken)
    {
        // locked per month because one API request delivers the whole month
        (int year, int month, decimal longitude, decimal latitude, RequestKey requestKey, string timezone) lockKey =
            (date.Year, date.Month, locationData.Longitude, locationData.Latitude, requestKey, locationData.TimezoneName);

        using (await s_getPrayerTimesLocker.LockAsync(lockKey, cancellationToken).ConfigureAwait(false))
        {
            WaqtiDailyPrayerTimes? prayerTimes =
                await waqtiRepository.GetPrayerTimesAsync(
                    date,
                    locationData.Longitude,
                    locationData.Latitude,
                    requestKey.Method,
                    requestKey.FajrAngle,
                    requestKey.IshaaAngle,
                    cancellationToken).ConfigureAwait(false);

            if (prayerTimes is not null)
            {
                return prayerTimes;
            }

            List<WaqtiDailyPrayerTimesResponseDTO> response =
                await waqtiApiService.GetMonthPrayerTimesAsync(
                    new WaqtiMonthPrayerTimesRequestDTO
                    {
                        Latitude = locationData.Latitude,
                        Longitude = locationData.Longitude,
                        Elevation = 0,
                        Timezone = locationData.TimezoneName,
                        Year = date.Year,
                        Month = date.Month,
                        Method = requestKey.Method,
                        Overrides = requestKey.FajrAngle is null && requestKey.IshaaAngle is null
                            ? null
                            : new WaqtiOverridesRequestDTO
                            {
                                FajrAngle = requestKey.FajrAngle,
                                IshaaAngle = requestKey.IshaaAngle,
                            },
                    },
                    cancellationToken).ConfigureAwait(false);

            // the coordinates and the method of the request are stored (not the ones of the
            // response), so that the cache lookup matches the next request exactly
            List<WaqtiDailyPrayerTimes> monthPrayerTimes =
                [.. response.Select(x =>
                {
                    WaqtiDailyPrayerTimes dailyPrayerTimes =
                        x.ToWaqtiPrayerTimes(locationData.Longitude, locationData.Latitude, requestKey.FajrAngle, requestKey.IshaaAngle);
                    dailyPrayerTimes.Method = requestKey.Method;
                    return dailyPrayerTimes;
                })];

            await waqtiRepository.InsertPrayerTimesAsync(monthPrayerTimes, cancellationToken).ConfigureAwait(false);

            return monthPrayerTimes.FirstOrDefault(x => x.Date == date);
        }
    }
}
