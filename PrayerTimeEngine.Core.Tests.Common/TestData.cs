using NodaTime;
using NodaTime.TimeZones;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Models;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Models;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Models;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models;

namespace PrayerTimeEngine.Core.Tests.Common
{
    public static class TestData
    {
        public static Profile CreateNewCompleteTestProfile(int profileID = 1)
        {
            var profile = new Profile
            {
                ID = profileID,
                Name = "Standard-Profil",
                LocationName = "Innsbruck",
                SequenceNo = 1,
            };

            profile.LocationConfigs =
                [
                    new()
                    {
                        CalculationSource = ECalculationSource.Muwaqqit,
                        ProfileID = profile.ID,
                        Profile = profile,
                        LocationData = new MuwaqqitLocationData{ Latitude = 47.2803835M,Longitude = 11.41337M,TimezoneName = "Europe/Vienna" }
                    },
                    new()
                    {
                        CalculationSource = ECalculationSource.Fazilet,
                        ProfileID = profile.ID,
                        Profile = profile,
                        LocationData = new FaziletLocationData{ CountryName = "Avusturya",CityName = "Innsbruck" }
                    },
                    new()
                    {
                        CalculationSource = ECalculationSource.Semerkand,
                        ProfileID = profile.ID,
                        Profile = profile,
                        LocationData = new SemerkandLocationData{ CountryName = "Avusturya",CityName = "Innsbruck",TimezoneName = "Europe/Vienna" }
                    },
                ];

            profile.TimeConfigs =
                [
                    new()
                    {
                        TimeType = ETimeType.FajrStart,
                        ProfileID = profile.ID,
                        Profile = profile,
                        CalculationConfiguration = new GenericSettingConfiguration { Source = ECalculationSource.Fazilet, TimeType = ETimeType.FajrStart }
                    },
                    new()
                    {
                        TimeType = ETimeType.FajrEnd,
                        ProfileID = profile.ID,
                        Profile = profile,
                        CalculationConfiguration = new GenericSettingConfiguration { Source = ECalculationSource.Semerkand, TimeType = ETimeType.FajrEnd }
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
                        CalculationConfiguration = new GenericSettingConfiguration { Source = ECalculationSource.Fazilet, TimeType = ETimeType.DhuhrStart }
                    },
                    new()
                    {
                        TimeType = ETimeType.DhuhrEnd,
                        ProfileID = profile.ID,
                        Profile = profile,
                        CalculationConfiguration = new GenericSettingConfiguration { Source = ECalculationSource.Muwaqqit, TimeType = ETimeType.DhuhrEnd }
                    },
                    new()
                    {
                        TimeType = ETimeType.AsrStart,
                        ProfileID = profile.ID,
                        Profile = profile,
                        CalculationConfiguration = new GenericSettingConfiguration { Source = ECalculationSource.Fazilet, TimeType = ETimeType.AsrStart }
                    },
                    new()
                    {
                        TimeType = ETimeType.AsrEnd,
                        ProfileID = profile.ID,
                        Profile = profile,
                        CalculationConfiguration = new GenericSettingConfiguration { Source = ECalculationSource.Muwaqqit, TimeType = ETimeType.AsrEnd }
                    },
                    new()
                    {
                        TimeType = ETimeType.AsrMithlayn,
                        ProfileID = profile.ID,
                        Profile = profile,
                        CalculationConfiguration = new GenericSettingConfiguration { Source = ECalculationSource.Muwaqqit, TimeType = ETimeType.AsrMithlayn }
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
                        CalculationConfiguration = new GenericSettingConfiguration { Source = ECalculationSource.Fazilet, TimeType = ETimeType.MaghribStart }
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
                        CalculationConfiguration = new GenericSettingConfiguration { Source = ECalculationSource.Fazilet, TimeType = ETimeType.IshaStart }
                    },
                    new()
                    {
                        TimeType = ETimeType.IshaEnd,
                        ProfileID = profile.ID,
                        Profile = profile,
                        CalculationConfiguration = new GenericSettingConfiguration { Source = ECalculationSource.Semerkand, TimeType = ETimeType.IshaEnd }
                    }
                ];

            return profile;
        }

        public static PrayerTimesBundle CreateNewTestPrayerTimesBundle()
        {
            var bundle = new PrayerTimesBundle();
            var zone = DateTimeZoneProviders.Tzdb["Europe/Vienna"];

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
}
