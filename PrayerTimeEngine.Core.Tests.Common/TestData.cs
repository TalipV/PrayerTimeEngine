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
        public static Profile CreateNewCompleteTestProfile()
        {
            Profile profile = new Profile
            {
                ID = 1,
                Name = "Standard-Profil",
                LocationName = "Innsbruck",
                SequenceNo = 1,
            };

            profile.LocationConfigs =
                new List<ProfileLocationConfig>
                {
                    new ProfileLocationConfig
                    {
                        CalculationSource = ECalculationSource.Muwaqqit,
                        ProfileID = profile.ID,
                        Profile = profile,
                        LocationData = new MuwaqqitLocationData{ Latitude = 47.2803835M,Longitude = 11.41337M,TimezoneName = "Europe/Vienna" }
                    },
                    new ProfileLocationConfig
                    {
                        CalculationSource = ECalculationSource.Fazilet,
                        ProfileID = profile.ID,
                        Profile = profile,
                        LocationData = new FaziletLocationData{ CountryName = "Avusturya",CityName = "Innsbruck" }
                    },
                    new ProfileLocationConfig
                    {
                        CalculationSource = ECalculationSource.Semerkand,
                        ProfileID = profile.ID,
                        Profile = profile,
                        LocationData = new SemerkandLocationData{ CountryName = "Avusturya",CityName = "Innsbruck",TimezoneName = "Europe/Vienna" }
                    },
                };

            profile.TimeConfigs =
                new List<ProfileTimeConfig>
                {
                    new ProfileTimeConfig
                    {
                        TimeType = ETimeType.FajrStart,
                        ProfileID = profile.ID,
                        Profile = profile,
                        CalculationConfiguration = new GenericSettingConfiguration { Source = ECalculationSource.Fazilet, TimeType = ETimeType.FajrStart }
                    },
                    new ProfileTimeConfig
                    {
                        TimeType = ETimeType.FajrEnd,
                        ProfileID = profile.ID,
                        Profile = profile,
                        CalculationConfiguration = new GenericSettingConfiguration { Source = ECalculationSource.Semerkand, TimeType = ETimeType.FajrEnd }
                    },
                    new ProfileTimeConfig
                    {
                        TimeType = ETimeType.FajrGhalas,
                        ProfileID = profile.ID,
                        Profile = profile,
                        CalculationConfiguration = new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.FajrGhalas, Degree = -8.5 }
                    },
                    new ProfileTimeConfig
                    {
                        TimeType = ETimeType.FajrKaraha,
                        ProfileID = profile.ID,
                        Profile = profile,
                        CalculationConfiguration = new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.FajrKaraha, Degree = -4.0 }
                    },
                    new ProfileTimeConfig
                    {
                        TimeType = ETimeType.DuhaStart,
                        ProfileID = profile.ID,
                        Profile = profile,
                        CalculationConfiguration = new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.DuhaStart, Degree = 5.0 }
                    },
                    new ProfileTimeConfig
                    {
                        TimeType = ETimeType.DuhaEnd,
                        ProfileID = profile.ID,
                        Profile = profile,
                        CalculationConfiguration = new GenericSettingConfiguration { TimeType = ETimeType.DuhaEnd, MinuteAdjustment = -25 }
                    },
                    new ProfileTimeConfig
                    {
                        TimeType = ETimeType.DhuhrStart,
                        ProfileID = profile.ID,
                        Profile = profile,
                        CalculationConfiguration = new GenericSettingConfiguration { Source = ECalculationSource.Fazilet, TimeType = ETimeType.DhuhrStart }
                    },
                    new ProfileTimeConfig
                    {
                        TimeType = ETimeType.DhuhrEnd,
                        ProfileID = profile.ID,
                        Profile = profile,
                        CalculationConfiguration = new GenericSettingConfiguration { Source = ECalculationSource.Muwaqqit, TimeType = ETimeType.DhuhrEnd }
                    },
                    new ProfileTimeConfig
                    {
                        TimeType = ETimeType.AsrStart,
                        ProfileID = profile.ID,
                        Profile = profile,
                        CalculationConfiguration = new GenericSettingConfiguration { Source = ECalculationSource.Fazilet, TimeType = ETimeType.AsrStart }
                    },
                    new ProfileTimeConfig
                    {
                        TimeType = ETimeType.AsrEnd,
                        ProfileID = profile.ID,
                        Profile = profile,
                        CalculationConfiguration = new GenericSettingConfiguration { Source = ECalculationSource.Muwaqqit, TimeType = ETimeType.AsrEnd }
                    },
                    new ProfileTimeConfig
                    {
                        TimeType = ETimeType.AsrMithlayn,
                        ProfileID = profile.ID,
                        Profile = profile,
                        CalculationConfiguration = new GenericSettingConfiguration { Source = ECalculationSource.Muwaqqit, TimeType = ETimeType.AsrMithlayn }
                    },
                    new ProfileTimeConfig
                    {
                        TimeType = ETimeType.AsrKaraha,
                        ProfileID = profile.ID,
                        Profile = profile,
                        CalculationConfiguration = new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.AsrKaraha, Degree = 5.0 }
                    },
                    new ProfileTimeConfig
                    {
                        TimeType = ETimeType.MaghribStart,
                        ProfileID = profile.ID,
                        Profile = profile,
                        CalculationConfiguration = new GenericSettingConfiguration { Source = ECalculationSource.Fazilet, TimeType = ETimeType.MaghribStart }
                    },
                    new ProfileTimeConfig
                    {
                        TimeType = ETimeType.MaghribEnd,
                        ProfileID = profile.ID,
                        Profile = profile,
                        CalculationConfiguration = new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.MaghribEnd, Degree = -15.0 }
                    },
                    new ProfileTimeConfig
                    {
                        TimeType = ETimeType.MaghribSufficientTime,
                        ProfileID = profile.ID,
                        Profile = profile,
                        CalculationConfiguration = new GenericSettingConfiguration { TimeType = ETimeType.MaghribSufficientTime, MinuteAdjustment = 20 }
                    },
                    new ProfileTimeConfig
                    {
                        TimeType = ETimeType.MaghribIshtibaq,
                        ProfileID = profile.ID,
                        Profile = profile,
                        CalculationConfiguration = new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.MaghribIshtibaq, Degree = -10.0 }
                    },
                    new ProfileTimeConfig
                    {
                        TimeType = ETimeType.IshaStart,
                        ProfileID = profile.ID,
                        Profile = profile,
                        CalculationConfiguration = new GenericSettingConfiguration { Source = ECalculationSource.Fazilet, TimeType = ETimeType.IshaStart }
                    },
                    new ProfileTimeConfig
                    {
                        TimeType = ETimeType.IshaEnd,
                        ProfileID = profile.ID,
                        Profile = profile,
                        CalculationConfiguration = new GenericSettingConfiguration { Source = ECalculationSource.Semerkand, TimeType = ETimeType.IshaEnd }
                    }
                };

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
