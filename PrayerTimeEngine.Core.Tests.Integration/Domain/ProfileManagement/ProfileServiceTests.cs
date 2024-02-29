using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.Extensions;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Models;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Models;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Models;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Services;
using PrayerTimeEngine.Core.Tests.Common;

namespace PrayerTimeEngine.Core.Tests.Integration.Domain.ProfileManagement
{
    public class ProfileServiceTests : BaseTest
    {
        [Fact]
        public async Task UpdateLocationConfig_SetsData_CorrectDataWithOriginalProfileUntouched()
        {
            // ARRANGE
            ServiceProvider serviceProvider = createServiceProvider(
                serviceCollection =>
                {
                    serviceCollection.AddSingleton(GetHandledDbContext());
                    serviceCollection.AddSingleton<TimeTypeAttributeService>();
                    serviceCollection.AddSingleton<IProfileDBAccess, ProfileDBAccess>();
                    serviceCollection.AddSingleton<IProfileService, ProfileService>();
                });

            var dbContext = serviceProvider.GetService<AppDbContext>();
            var profileService = serviceProvider.GetService<IProfileService>() as ProfileService;

            await dbContext.Profiles.AddAsync(TestData.CreateNewCompleteTestProfile());
            await dbContext.SaveChangesAsync();

            // in the UI the data is loaded without tracking (i.e. intended for read only)
            // changes have to be made on separate entities with tracking to keep the mechanisms clean
            var profile =
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
                [ECalculationSource.Fazilet] = new FaziletLocationData { CountryName = "Almanya", CityName = newLocationName },
                [ECalculationSource.Semerkand] = new SemerkandLocationData { CountryName = "Almanya", CityName = newLocationName, TimezoneName = "Europe/Vienna" },
            };

            // ACT
            await profileService.UpdateLocationConfig(profile, newLocationName, newLocationDataByCalculationSource.Select(x => (x.Key, x.Value)).ToList());

            // ASSERT
            dbContext.ChangeTracker.HasChanges().Should().BeFalse();

            profile.LocationName.Should().Be(newLocationName);
            foreach (var locationDataByCalculationSource in profile.LocationConfigs.ToDictionary(x => x.CalculationSource, x => x.LocationData))
            {
                BaseLocationData newValue = newLocationDataByCalculationSource[locationDataByCalculationSource.Key];
                BaseLocationData currentValue = locationDataByCalculationSource.Value;

                currentValue.Should().Be(newValue);
            }
        }

        [Fact]
        public async Task UpdateLocationConfig_SetsDataWithExceptionOnCommit_LocalAndDbChangesReverted()
        {
            // ARRANGE
            ServiceProvider serviceProvider = createServiceProvider(
                serviceCollection =>
                {
                    serviceCollection.AddSingleton(GetHandledDbContext());
                    serviceCollection.AddSingleton<TimeTypeAttributeService>();
                    serviceCollection.AddSingleton<IProfileDBAccess, ProfileDBAccess>();
                    serviceCollection.AddSingleton<IProfileService, ProfileService>();
                });

            var dbContext = serviceProvider.GetService<AppDbContext>();
            var transactionSub = Substitute.For<IDbContextTransaction>();
            dbContext.Database.Configure().BeginTransactionAsync().Returns(Task.FromResult(transactionSub));
            transactionSub.Configure().CommitAsync(Arg.Any<CancellationToken>())
                          .Throws(new Exception("Test exception during commit"));

            var profileService = serviceProvider.GetService<IProfileService>() as ProfileService;

            await dbContext.Profiles.AddAsync(TestData.CreateNewCompleteTestProfile());
            await dbContext.SaveChangesAsync();
            var profile =
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
            Func<Task> updateLocationConfigFunc =
                async () => await profileService.UpdateLocationConfig(profile, oldLocationName, newLocationDataByCalculationSource.Select(x => (x.Key, x.Value)).ToList());

            // ASSERT
            await updateLocationConfigFunc.Should().ThrowAsync<Exception>();
            dbContext.ChangeTracker.HasChanges().Should().BeFalse();

            // old values should still be in this profile instance
            profile.LocationName.Should().Be(oldLocationName);
            foreach (var locationDataByCalculationSource in profile.LocationConfigs.ToDictionary(x => x.CalculationSource, x => x.LocationData))
            {
                BaseLocationData oldValue = oldLocationDataByCalculationSource[locationDataByCalculationSource.Key];
                BaseLocationData currentValue = locationDataByCalculationSource.Value;

                currentValue.Should().Be(oldValue);
            }
        }

        [Fact]
        public async Task UpdateTimeConfig_SetNewValue_Success()
        {
            // ARRANGE
            ServiceProvider serviceProvider = createServiceProvider(
                serviceCollection =>
                {
                    serviceCollection.AddSingleton(GetHandledDbContext());
                    serviceCollection.AddSingleton<TimeTypeAttributeService>();
                    serviceCollection.AddSingleton<IProfileDBAccess, ProfileDBAccess>();
                    serviceCollection.AddSingleton<IProfileService, ProfileService>();
                });

            var dbContext = serviceProvider.GetService<AppDbContext>();
            var profileService = serviceProvider.GetService<IProfileService>() as ProfileService;

            await dbContext.Profiles.AddAsync(TestData.CreateNewCompleteTestProfile());
            await dbContext.SaveChangesAsync();
            var profile =
                dbContext.Profiles
                    .Include(x => x.LocationConfigs)
                    .Include(x => x.TimeConfigs)
                    .AsNoTracking()
                    .Single();

            // ACT
            var newSemerkandConfig =
                new GenericSettingConfiguration
                {
                    Source = ECalculationSource.Semerkand,
                    TimeType = ETimeType.FajrStart
                };

            await profileService.UpdateTimeConfig(profile, ETimeType.FajrStart, newSemerkandConfig);

            // ASSERT
            dbContext.ChangeTracker.HasChanges().Should().BeFalse();

            var fajrStartConfig = profileService.GetTimeConfig(profile, ETimeType.FajrStart);
            fajrStartConfig.Source.Should().Be(ECalculationSource.Semerkand);
        }
    }
}