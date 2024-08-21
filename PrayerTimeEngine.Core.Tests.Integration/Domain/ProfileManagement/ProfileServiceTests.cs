﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
using PrayerTimeEngine.Core.Domain.PlaceManagement.Models;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Services;
using PrayerTimeEngine.Core.Tests.Common;
using PrayerTimeEngine.Core.Tests.Common.TestData;

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
                    serviceCollection.AddSingleton(GetHandledDbContextFactory());
                    serviceCollection.AddSingleton<TimeTypeAttributeService>();
                    serviceCollection.AddSingleton<IProfileDBAccess, ProfileDBAccess>();
                    serviceCollection.AddSingleton<IProfileService, ProfileService>();
                });

            using var dbContext = serviceProvider.GetService<IDbContextFactory<AppDbContext>>().CreateDbContext();
            var profileService = serviceProvider.GetService<IProfileService>() as ProfileService;

            await dbContext.Profiles.AddAsync(TestDataHelper.CreateNewCompleteTestProfile());
            await dbContext.SaveChangesAsync();

            // in the UI the data is loaded without tracking (i.e. intended for read only)
            // changes have to be made on separate entities with tracking to keep the mechanisms clean
            var profile =
                dbContext.Profiles
                    .Include(x => x.LocationConfigs)
                    .Include(x => x.TimeConfigs)
                    .Include(x => x.PlaceInfo).ThenInclude(x => x.TimezoneInfo)
                    .AsNoTracking()
                    .Single();

            ProfilePlaceInfo oldPlaceInfo = profile.PlaceInfo;
            var oldLocationDataByCalculationSource = profile.LocationConfigs.ToDictionary(x => x.CalculationSource, x => x.LocationData);

            ProfilePlaceInfo newPlaceInfo = new ProfilePlaceInfo
            {
                ExternalID = "1",
                Longitude = 1M,
                Latitude = 1M,
                InfoLanguageCode = "de",
                Country = "Deutschland",
                City = "Köln",
                CityDistrict = "",
                PostCode = "",
                Street = "",
                TimezoneInfo = new TimezoneInfo { Name = TestDataHelper.EUROPE_BERLIN_TIME_ZONE.Id },
            };

            var newLocationDataByCalculationSource = new Dictionary<ECalculationSource, BaseLocationData>
            {
                [ECalculationSource.Muwaqqit] = new MuwaqqitLocationData { Latitude = 50.9413M, Longitude = 6.9583M, TimezoneName = TestDataHelper.EUROPE_BERLIN_TIME_ZONE.Id },
                [ECalculationSource.Fazilet] = new FaziletLocationData { CountryName = "Almanya", CityName = newPlaceInfo.City },
                [ECalculationSource.Semerkand] = new SemerkandLocationData { CountryName = "Almanya", CityName = newPlaceInfo.City, TimezoneName = TestDataHelper.EUROPE_BERLIN_TIME_ZONE.Id },
            };

            // ACT
            await profileService.UpdateLocationConfig(profile, newPlaceInfo, newLocationDataByCalculationSource.Select(x => (x.Key, x.Value)).ToList(), default);

            // ASSERT
            dbContext.ChangeTracker.HasChanges().Should().BeFalse();

            profile.PlaceInfo.Should().NotBeNull();
            profile.PlaceInfo.Should().NotBe(oldPlaceInfo);
            profile.PlaceInfo.Should().Be(newPlaceInfo);

            foreach (var locationDataByCalculationSource in profile.LocationConfigs.ToDictionary(x => x.CalculationSource, x => x.LocationData))
            {
                BaseLocationData newValue = newLocationDataByCalculationSource[locationDataByCalculationSource.Key];
                BaseLocationData currentValue = locationDataByCalculationSource.Value;

                currentValue.Should().Be(newValue);
            }
        }

        [Fact]
        public async Task UpdateLocationConfig_SetsDataWithExceptionOnCommit_OldDataFullyRemains()
        {
            // ARRANGE
            ServiceProvider serviceProvider = createServiceProvider(
                serviceCollection =>
                {
                    serviceCollection.AddSingleton(GetHandledDbContextFactory());
                    serviceCollection.AddSingleton<TimeTypeAttributeService>();
                    serviceCollection.AddSingleton<IProfileDBAccess, ProfileDBAccess>();
                    serviceCollection.AddSingleton<IProfileService, ProfileService>();
                });

            var dbContextFactory = serviceProvider.GetService<IDbContextFactory<AppDbContext>>();
            using var dbContext = dbContextFactory.CreateDbContext();

            dbContextFactory.Configure().CreateDbContext().Returns(dbContext);
            dbContextFactory.Configure().CreateDbContextAsync().Returns(Task.FromResult(dbContext));

            var profileService = serviceProvider.GetService<IProfileService>() as ProfileService;

            await dbContext.Profiles.AddAsync(TestDataHelper.CreateNewCompleteTestProfile());
            await dbContext.SaveChangesAsync();

            dbContext.SaveChangesAsync().Throws(new Exception("Test exception during commit"));

            var profile =
                dbContext.Profiles
                    .Include(x => x.LocationConfigs)
                    .Include(x => x.TimeConfigs)
                    .Include(x => x.PlaceInfo).ThenInclude(x => x.TimezoneInfo)
                    .AsNoTracking()
                    .Single();

            ProfilePlaceInfo oldPlaceInfo = profile.PlaceInfo;
            var oldLocationDataByCalculationSource = profile.LocationConfigs.ToDictionary(x => x.CalculationSource, x => x.LocationData);
            var newLocationDataByCalculationSource = new Dictionary<ECalculationSource, BaseLocationData>
            {
                [ECalculationSource.Muwaqqit] = new MuwaqqitLocationData { Latitude = 50.9413M, Longitude = 6.9583M, TimezoneName = TestDataHelper.EUROPE_BERLIN_TIME_ZONE.Id },
                [ECalculationSource.Fazilet] = new FaziletLocationData { CountryName = "Almanya", CityName = "Köln" },
                [ECalculationSource.Semerkand] = new SemerkandLocationData { CountryName = "Almanya", CityName = "Köln", TimezoneName = TestDataHelper.EUROPE_BERLIN_TIME_ZONE.Id },
            };

            var newPlaceInfo =
                new ProfilePlaceInfo
                {
                    Latitude = 1M,
                    Longitude = 1M,
                    InfoLanguageCode = "de",
                    Country = "Deutschland",
                    City = "Köln",
                    CityDistrict = "",
                    PostCode = "50587",
                    Street = "",
                    TimezoneInfo = new TimezoneInfo
                    {
                        DisplayName = "CET",
                        Name = "Central European Time",
                        UtcOffsetSeconds = 3600
                    }
                };

            // ACT
            Func<Task> updateLocationConfigFunc =
                async () =>
                {
                    await profileService.UpdateLocationConfig(
                        profile: profile,
                        placeInfo: newPlaceInfo,
                        locationDataByCalculationSource: newLocationDataByCalculationSource.Select(x => (x.Key, x.Value)).ToList(),
                        cancellationToken: default);
                };

            // ASSERT
            await updateLocationConfigFunc.Should().ThrowAsync<Exception>().WithMessage("Test exception during commit");
            
            profile.PlaceInfo.Should().NotBeNull();
            profile.PlaceInfo.Should().NotBe(newPlaceInfo);
            profile.PlaceInfo.Should().Be(oldPlaceInfo);

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
                    serviceCollection.AddSingleton(GetHandledDbContextFactory());
                    serviceCollection.AddSingleton<TimeTypeAttributeService>();
                    serviceCollection.AddSingleton<IProfileDBAccess, ProfileDBAccess>();
                    serviceCollection.AddSingleton<IProfileService, ProfileService>();
                });

            var profileService = serviceProvider.GetService<IProfileService>() as ProfileService;

            await TestArrangeDbContext.Profiles.AddAsync(TestDataHelper.CreateNewCompleteTestProfile());
            await TestArrangeDbContext.SaveChangesAsync();

            var profile =
                TestArrangeDbContext.Profiles
                    .Include(x => x.LocationConfigs)
                    .Include(x => x.TimeConfigs)
                    .AsNoTracking()
                    .Single();

            var newSemerkandConfig =
                new GenericSettingConfiguration
                {
                    Source = ECalculationSource.Semerkand,
                    TimeType = ETimeType.FajrStart
                };

            // ACT
            await profileService.UpdateTimeConfig(profile, ETimeType.FajrStart, newSemerkandConfig, default);

            // ASSERT

            var fajrStartConfig = profileService.GetTimeConfig(profile, ETimeType.FajrStart);
            fajrStartConfig.Source.Should().Be(ECalculationSource.Semerkand);
        }
    }
}