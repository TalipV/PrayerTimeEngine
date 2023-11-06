using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.Extensions;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Models;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Models;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Models;
using PrayerTimeEngine.Core.Domain.Configuration.Interfaces;
using PrayerTimeEngine.Core.Domain.Configuration.Models;
using PrayerTimeEngine.Core.Domain.Configuration.Services;
using PrayerTimeEngine.Core.Domain.Model;

namespace PrayerTimeEngine.Core.Tests
{
    public class ProfileServiceTests : BaseTest
    {
        protected override void ConfigureServiceProvider(ServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IProfileDBAccess, ProfileDBAccess>();
            serviceCollection.AddSingleton<IProfileService, ProfileService>();
        }

        [Test]
        public async Task GetTimeConfig_ExistingTimeConfig_ShouldReturnConfig()
        {
            // ARRANGE
            var dbContext = ServiceProvider.GetService<AppDbContext>();
            var profileService = ServiceProvider.GetService<IProfileService>() as ProfileService;

            Profile profile = getTestProfile();
            await dbContext.Profiles.AddAsync(profile);
            ETimeType timeType = ETimeType.FajrStart;

            // ACT
            GenericSettingConfiguration result = profileService.GetTimeConfig(profile, timeType, false);

            // ASSERT
            Assert.IsNotNull(result);
            Assert.That(result.TimeType, Is.EqualTo(ETimeType.FajrStart));
        }

        [Test]
        public async Task GetTimeConfig_NonExistingTimeConfig_CreateFalse_ShouldReturnNull()
        {
            // ARRANGE
            var dbContext = ServiceProvider.GetService<AppDbContext>();
            var profileService = ServiceProvider.GetService<IProfileService>() as ProfileService;
            
            Profile profile = getTestProfile();
            await dbContext.Profiles.AddAsync(profile);
            ETimeType timeType = ETimeType.IshaEnd;

            profile.TimeConfigs.Remove(profile.TimeConfigs.First(x => x.TimeType == timeType));

            // ACT
            GenericSettingConfiguration result = profileService.GetTimeConfig(profile, timeType, false);

            // ASSERT
            Assert.IsNull(result);
        }

        [Test]
        public async Task GetTimeConfig_NonExistingTimeConfig_CreateTrue_ShouldCreateAndReturnConfig()
        {
            // ARRANGE
            var dbContext = ServiceProvider.GetService<AppDbContext>();
            var profileService = ServiceProvider.GetService<IProfileService>() as ProfileService;
            
            Profile profile = getTestProfile();
            await dbContext.Profiles.AddAsync(profile);
            ETimeType timeType = ETimeType.IshaEnd;

            profile.TimeConfigs.Remove(profile.TimeConfigs.First(x => x.TimeType == timeType));

            // ACT
            GenericSettingConfiguration result = profileService.GetTimeConfig(profile, timeType, true);

            // ASSERT
            Assert.IsNotNull(result);
            Assert.That(result.TimeType, Is.EqualTo(ETimeType.IshaEnd));
        }

        [Test]
        public async Task SetTimeConfig_TimeConfigExists_ShouldUpdateConfig()
        {
            // ARRANGE
            var dbContext = ServiceProvider.GetService<AppDbContext>();
            var profileService = ServiceProvider.GetService<IProfileService>() as ProfileService;
            
            Profile profile = getTestProfile();
            await dbContext.Profiles.AddAsync(profile);
            ETimeType timeType = ETimeType.FajrStart;
            var newSettings = 
                new GenericSettingConfiguration 
                { 
                    TimeType = timeType, 
                    Source = ECalculationSource.Semerkand 
                };

            // ACT
            profileService.SetTimeConfig(profile, timeType, newSettings);
            GenericSettingConfiguration updatedConfig = profileService.GetTimeConfig(profile, timeType, createIfNotExists: false);

            // ASSERT
            Assert.IsNotNull(updatedConfig);
            Assert.That(updatedConfig.Source, Is.EqualTo(ECalculationSource.Semerkand));
        }

        [Test]
        public async Task SetTimeConfig_TimeConfigDoesNotExist_ShouldCreateNewConfig()
        {
            // ARRANGE
            var dbContext = ServiceProvider.GetService<AppDbContext>();
            var profileService = ServiceProvider.GetService<IProfileService>() as ProfileService;
            
            Profile profile = getTestProfile();
            await dbContext.Profiles.AddAsync(profile);
            ETimeType timeType = ETimeType.IshaEnd;
            GenericSettingConfiguration newSettings = 
                new GenericSettingConfiguration 
                { 
                    TimeType = timeType, 
                    Source = ECalculationSource.Fazilet 
                };

            profile.TimeConfigs.Remove(profile.TimeConfigs.First(x => x.TimeType == timeType));

            // ACT
            profileService.SetTimeConfig(profile, timeType, newSettings);
            GenericSettingConfiguration newConfig = profileService.GetTimeConfig(profile, timeType, createIfNotExists: false);

            // ASSERT
            Assert.IsNotNull(newConfig);
            Assert.That(newConfig.Source, Is.EqualTo(ECalculationSource.Fazilet));
        }

        [Test]
        public async Task GetLocationConfig_MatchFound_ShouldReturnLocationData()
        {
            // ARRANGE
            var dbContext = ServiceProvider.GetService<AppDbContext>();
            var profileService = ServiceProvider.GetService<IProfileService>() as ProfileService;

            Profile profile = getTestProfile();
            await dbContext.Profiles.AddAsync(profile);
            ECalculationSource source = ECalculationSource.Muwaqqit;

            // ACT
            MuwaqqitLocationData result = profileService.GetLocationConfig(profile, source) as MuwaqqitLocationData;

            // ASSERT
            Assert.IsNotNull(result);
            Assert.That(result.TimezoneName, Is.EqualTo("Europe/Vienna"));
        }

        [Test]
        public async Task GetLocationConfig_NoMatchFound_ShouldReturnNull()
        {
            // ARRANGE
            var dbContext = ServiceProvider.GetService<AppDbContext>();
            var profileService = ServiceProvider.GetService<IProfileService>() as ProfileService;
            
            Profile profile = getTestProfile();
            await dbContext.Profiles.AddAsync(profile);
            ECalculationSource source = ECalculationSource.Muwaqqit;

            profile.LocationConfigs.Remove(profile.LocationConfigs.First(x => x.CalculationSource == source));

            // ACT
            BaseLocationData result = profileService.GetLocationConfig(profile, source);

            // ASSERT
            Assert.IsNull(result);
        }

        [Test]
        public async Task UpdateLocationConfig_SetsData_CorrectDataWithOriginalProfileUntouched()
        {
            // ARRANGE
            var dbContext = ServiceProvider.GetService<AppDbContext>();
            var profileService = ServiceProvider.GetService<IProfileService>() as ProfileService;

            await dbContext.Profiles.AddAsync(getTestProfile());
            await dbContext.SaveChangesAsync();

            // in the UI the data is loaded without tracking (i.e. intended for read only)
            // changes have to be made on separate entities with tracking to keep the mechanisms clean
            Profile profile = 
                dbContext.Profiles
                    .Include(x => x.LocationConfigs)
                    .Include(x => x.TimeConfigs)
                    .AsNoTracking()
                    .Single();

            string oldLocationName = profile.LocationName;
            var oldLocationDataByCalculationSource = profile.LocationConfigs.ToDictionary(x => x.CalculationSource, x => x.LocationData);

            string newLocationName = "Köln";
            var newLocationDataByCalculationSource = new Dictionary<ECalculationSource, BaseLocationData>
            {
                [ECalculationSource.Muwaqqit] = new MuwaqqitLocationData { Latitude = 50.9413M, Longitude = 6.9583M, TimezoneName = "Europe/Vienna" },
                [ECalculationSource.Fazilet] = new FaziletLocationData  { CountryName = "Almanya", CityName = newLocationName },
                [ECalculationSource.Semerkand] = new SemerkandLocationData { CountryName = "Almanya", CityName = newLocationName, TimezoneName = "Europe/Vienna" },
            };

            // ACT
            await profileService.UpdateLocationConfig(profile, newLocationName, newLocationDataByCalculationSource.Select(x => (x.Key, x.Value)).ToList());

            // ASSERT
            Assert.IsFalse(dbContext.ChangeTracker.HasChanges());

            Assert.That(profile.LocationName, Is.EqualTo(newLocationName));
            foreach (var locationDataByCalculationSource in profile.LocationConfigs.ToDictionary(x => x.CalculationSource, x => x.LocationData))
            {
                BaseLocationData newValue = newLocationDataByCalculationSource[locationDataByCalculationSource.Key];
                BaseLocationData currentValue = locationDataByCalculationSource.Value;

                Assert.That(currentValue, Is.EqualTo(newValue));
            }
        }

        [Test]
        public async Task UpdateLocationConfig_SetsDataWithExceptionOnCommit_LocalAndDbChangesReverted()
        {
            // ARRANGE
            var dbContext = ServiceProvider.GetService<AppDbContext>();
            var transactionSub = Substitute.For<IDbContextTransaction>();
            dbContext.Database.Configure().BeginTransactionAsync().Returns(Task.FromResult(transactionSub));
            transactionSub.Configure().CommitAsync(Arg.Any<CancellationToken>())
                          .Throws(new Exception("Test exception during commit"));

            var profileService = ServiceProvider.GetService<IProfileService>() as ProfileService;

            await dbContext.Profiles.AddAsync(getTestProfile());
            await dbContext.SaveChangesAsync();
            Profile profile =
                dbContext.Profiles
                    .Include(x => x.LocationConfigs)
                    .Include(x => x.TimeConfigs)
                    .AsNoTracking()
                    .Single();

            string oldLocationName = profile.LocationName; 
            var oldLocationDataByCalculationSource = profile.LocationConfigs.ToDictionary(x => x.CalculationSource, x => x.LocationData);
            var newLocationDataByCalculationSource = new Dictionary<ECalculationSource, BaseLocationData>
            {
                [ECalculationSource.Muwaqqit] = new MuwaqqitLocationData { Latitude = 50.9413M, Longitude = 6.9583M, TimezoneName = "Europe/Vienna" },
                [ECalculationSource.Fazilet] = new FaziletLocationData { CountryName = "Almanya", CityName = "Köln" },
                [ECalculationSource.Semerkand] = new SemerkandLocationData { CountryName = "Almanya", CityName = "Köln", TimezoneName = "Europe/Vienna" },
            };

            // ACT
            Assert.ThrowsAsync<Exception>(async () => await profileService.UpdateLocationConfig(profile, oldLocationName, newLocationDataByCalculationSource.Select(x => (x.Key, x.Value)).ToList()));

            // ASSERT
            Assert.IsFalse(dbContext.ChangeTracker.HasChanges());

            // old values should still be in this profile instance
            Assert.That(profile.LocationName, Is.EqualTo(oldLocationName));
            foreach (var locationDataByCalculationSource in profile.LocationConfigs.ToDictionary(x => x.CalculationSource, x => x.LocationData))
            {
                BaseLocationData oldValue = oldLocationDataByCalculationSource[locationDataByCalculationSource.Key];
                BaseLocationData currentValue = locationDataByCalculationSource.Value;

                Assert.That(currentValue, Is.EqualTo(oldValue));
            }
        }


        private static Profile getTestProfile()
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
    }
}