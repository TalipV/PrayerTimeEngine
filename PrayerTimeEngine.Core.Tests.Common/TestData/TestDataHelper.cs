using NodaTime;
using NodaTime.TimeZones;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Models;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Muwaqqit.Models;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Models;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Models;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;

namespace PrayerTimeEngine.Core.Tests.Common.TestData;

public static class TestDataHelper
{
    private static readonly string BASE_TEST_DATA_FILE_PATH = Path.Combine(Directory.GetCurrentDirectory(), "TestData");

    public static readonly string FAZILET_TEST_DATA_FILE_PATH = Path.Combine(BASE_TEST_DATA_FILE_PATH, "FaziletTestData");
    public static readonly string SEMERKAND_TEST_DATA_FILE_PATH = Path.Combine(BASE_TEST_DATA_FILE_PATH, "SemerkandTestData");
    public static readonly string MUWAQQIT_TEST_DATA_FILE_PATH = Path.Combine(BASE_TEST_DATA_FILE_PATH, "MuwaqqitTestData");
    public static readonly string MAWAQIT_TEST_DATA_FILE_PATH = Path.Combine(BASE_TEST_DATA_FILE_PATH, "MawaqitTestData");
    public static readonly string MYMOSQ_TEST_DATA_FILE_PATH = Path.Combine(BASE_TEST_DATA_FILE_PATH, "MyMosqTestData");

    public static readonly string LOCATIONIQ_TEST_DATA_FILE_PATH = Path.Combine(BASE_TEST_DATA_FILE_PATH, "LocationIQTestData");

    public static readonly DateTimeZone EUROPE_VIENNA_TIME_ZONE = DateTimeZoneProviders.Tzdb["Europe/Vienna"];
    public static readonly DateTimeZone EUROPE_BERLIN_TIME_ZONE = DateTimeZoneProviders.Tzdb["Europe/Berlin"];

    public static DynamicProfile CreateNewCompleteTestProfile(int profileID = 1)
    {
        var profile = new DynamicProfile
        {
            ID = profileID,
            Name = "Standard-Profil",
            PlaceInfo = new ProfilePlaceInfo
            {
                Latitude = 47.2803835M,
                Longitude = 11.41337M,
                InfoLanguageCode = "de",
                Country = "Österreich",
                City = "Innsbruck",
                CityDistrict = "",
                PostCode = "6020",
                Street = "",
                TimezoneInfo = new TimezoneInfo
                {
                    DisplayName = "CET",
                    Name = "Central European Time",
                    UtcOffsetSeconds = 3600
                }
            },
            SequenceNo = 1,
        };

        profile.LocationConfigs =
            [
                new()
                {
                    DynamicPrayerTimeProvider = EDynamicPrayerTimeProviderType.Muwaqqit,
                    ProfileID = profile.ID,
                    Profile = profile,
                    LocationData = new MuwaqqitLocationData{ Latitude = 47.2803835M,Longitude = 11.41337M, TimezoneName = EUROPE_VIENNA_TIME_ZONE.Id }
                },
                new()
                {
                    DynamicPrayerTimeProvider = EDynamicPrayerTimeProviderType.Fazilet,
                    ProfileID = profile.ID,
                    Profile = profile,
                    LocationData = new FaziletLocationData{ CountryName = "Avusturya",CityName = "Innsbruck" }
                },
                new()
                {
                    DynamicPrayerTimeProvider = EDynamicPrayerTimeProviderType.Semerkand,
                    ProfileID = profile.ID,
                    Profile = profile,
                    LocationData = new SemerkandLocationData{ CountryName = "Avusturya", CityName = "Innsbruck", TimezoneName = EUROPE_VIENNA_TIME_ZONE.Id }
                },
            ];

        profile.TimeConfigs =
            [
                new()
                {
                    TimeType = ETimeType.FajrStart,
                    ProfileID = profile.ID,
                    Profile = profile,
                    CalculationConfiguration = new GenericSettingConfiguration { Source = EDynamicPrayerTimeProviderType.Fazilet, TimeType = ETimeType.FajrStart }
                },
                new()
                {
                    TimeType = ETimeType.FajrEnd,
                    ProfileID = profile.ID,
                    Profile = profile,
                    CalculationConfiguration = new GenericSettingConfiguration { Source = EDynamicPrayerTimeProviderType.Semerkand, TimeType = ETimeType.FajrEnd }
                },
                new()
                {
                    TimeType = ETimeType.FajrGhalas,
                    ProfileID = profile.ID,
                    Profile = profile,
                    CalculationConfiguration = new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.FajrGhalas, Degree = -8.5 }
                },
                new()
                {
                    TimeType = ETimeType.FajrKaraha,
                    ProfileID = profile.ID,
                    Profile = profile,
                    CalculationConfiguration = new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.FajrKaraha, Degree = -4.0 }
                },
                new()
                {
                    TimeType = ETimeType.DuhaStart,
                    ProfileID = profile.ID,
                    Profile = profile,
                    CalculationConfiguration = new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.DuhaStart, Degree = 5.0 }
                },
                new()
                {
                    TimeType = ETimeType.DuhaEnd,
                    ProfileID = profile.ID,
                    Profile = profile,
                    CalculationConfiguration = new GenericSettingConfiguration { TimeType = ETimeType.DuhaEnd, MinuteAdjustment = -25 }
                },
                new()
                {
                    TimeType = ETimeType.DhuhrStart,
                    ProfileID = profile.ID,
                    Profile = profile,
                    CalculationConfiguration = new GenericSettingConfiguration { Source = EDynamicPrayerTimeProviderType.Fazilet, TimeType = ETimeType.DhuhrStart }
                },
                new()
                {
                    TimeType = ETimeType.DhuhrEnd,
                    ProfileID = profile.ID,
                    Profile = profile,
                    CalculationConfiguration = new GenericSettingConfiguration { Source = EDynamicPrayerTimeProviderType.Muwaqqit, TimeType = ETimeType.DhuhrEnd }
                },
                new()
                {
                    TimeType = ETimeType.AsrStart,
                    ProfileID = profile.ID,
                    Profile = profile,
                    CalculationConfiguration = new GenericSettingConfiguration { Source = EDynamicPrayerTimeProviderType.Fazilet, TimeType = ETimeType.AsrStart }
                },
                new()
                {
                    TimeType = ETimeType.AsrEnd,
                    ProfileID = profile.ID,
                    Profile = profile,
                    CalculationConfiguration = new GenericSettingConfiguration { Source = EDynamicPrayerTimeProviderType.Muwaqqit, TimeType = ETimeType.AsrEnd }
                },
                new()
                {
                    TimeType = ETimeType.AsrMithlayn,
                    ProfileID = profile.ID,
                    Profile = profile,
                    CalculationConfiguration = new GenericSettingConfiguration { Source = EDynamicPrayerTimeProviderType.Muwaqqit, TimeType = ETimeType.AsrMithlayn }
                },
                new()
                {
                    TimeType = ETimeType.AsrKaraha,
                    ProfileID = profile.ID,
                    Profile = profile,
                    CalculationConfiguration = new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.AsrKaraha, Degree = 5.0 }
                },
                new()
                {
                    TimeType = ETimeType.MaghribStart,
                    ProfileID = profile.ID,
                    Profile = profile,
                    CalculationConfiguration = new GenericSettingConfiguration { Source = EDynamicPrayerTimeProviderType.Fazilet, TimeType = ETimeType.MaghribStart }
                },
                new()
                {
                    TimeType = ETimeType.MaghribEnd,
                    ProfileID = profile.ID,
                    Profile = profile,
                    CalculationConfiguration = new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.MaghribEnd, Degree = -15.0 }
                },
                new()
                {
                    TimeType = ETimeType.MaghribSufficientTime,
                    ProfileID = profile.ID,
                    Profile = profile,
                    CalculationConfiguration = new GenericSettingConfiguration { TimeType = ETimeType.MaghribSufficientTime, MinuteAdjustment = 20 }
                },
                new()
                {
                    TimeType = ETimeType.MaghribIshtibaq,
                    ProfileID = profile.ID,
                    Profile = profile,
                    CalculationConfiguration = new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.MaghribIshtibaq, Degree = -10.0 }
                },
                new()
                {
                    TimeType = ETimeType.IshaStart,
                    ProfileID = profile.ID,
                    Profile = profile,
                    CalculationConfiguration = new GenericSettingConfiguration { Source = EDynamicPrayerTimeProviderType.Fazilet, TimeType = ETimeType.IshaStart }
                },
                new()
                {
                    TimeType = ETimeType.IshaEnd,
                    ProfileID = profile.ID,
                    Profile = profile,
                    CalculationConfiguration = new GenericSettingConfiguration { Source = EDynamicPrayerTimeProviderType.Semerkand, TimeType = ETimeType.IshaEnd }
                }
            ];

        return profile;
    }

    public static DynamicPrayerTimesSet CreateNewTestPrayerTimesBundle()
    {
        var bundle = new DynamicPrayerTimesSet();
        var zone = EUROPE_VIENNA_TIME_ZONE;

        bundle.Fajr.Start = new LocalDateTime(2023, 1, 1, 2, 0, 0).InZone(zone, Resolvers.StrictResolver);
        bundle.Fajr.End = new LocalDateTime(2023, 1, 1, 4, 0, 0).InZone(zone, Resolvers.StrictResolver);

        bundle.Duha.Start = new LocalDateTime(2023, 1, 1, 5, 0, 0).InZone(zone, Resolvers.StrictResolver);
        bundle.Duha.End = new LocalDateTime(2023, 1, 1, 6, 0, 0).InZone(zone, Resolvers.StrictResolver);

        bundle.Dhuhr.Start = new LocalDateTime(2023, 1, 1, 12, 0, 0).InZone(zone, Resolvers.StrictResolver);
        bundle.Dhuhr.End = new LocalDateTime(2023, 1, 1, 13, 0, 0).InZone(zone, Resolvers.StrictResolver);

        bundle.Asr.Start = new LocalDateTime(2023, 1, 1, 16, 0, 0).InZone(zone, Resolvers.StrictResolver);
        bundle.Asr.End = new LocalDateTime(2023, 1, 1, 17, 30, 0).InZone(zone, Resolvers.StrictResolver);

        bundle.Maghrib.Start = new LocalDateTime(2023, 1, 1, 19, 0, 0).InZone(zone, Resolvers.StrictResolver);
        bundle.Maghrib.End = new LocalDateTime(2023, 1, 1, 19, 30, 0).InZone(zone, Resolvers.StrictResolver);

        bundle.Isha.Start = new LocalDateTime(2023, 1, 1, 23, 0, 0).InZone(zone, Resolvers.StrictResolver);
        bundle.Isha.End = new LocalDateTime(2023, 1, 2, 1, 0, 0).InZone(zone, Resolvers.StrictResolver);

        return bundle;
    }
}
