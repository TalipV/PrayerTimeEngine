using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.Extensions;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Models;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Services;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Muwaqqit.Models;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Models;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Models;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Services;
using PrayerTimeEngine.Core.Tests.Common;
using PrayerTimeEngine.Core.Tests.Common.TestData;

namespace PrayerTimeEngine.Core.Tests.Integration.Domain.ProfileManagement;

public class ProfileServiceTests : BaseTest
{
    private ServiceProvider getServiceProvider()
    {
        return createServiceProvider(
            serviceCollection =>
            {
                serviceCollection.AddSingleton(GetHandledDbContextFactory());
                serviceCollection.AddSingleton<TimeTypeAttributeService>();
                serviceCollection.AddSingleton<IProfileDBAccess, ProfileDBAccess>();
                serviceCollection.AddSingleton<IProfileService, ProfileService>();
                serviceCollection.AddSingleton<IDynamicPrayerTimeProviderFactory, DynamicPrayerTimeProviderFactory>();
                serviceCollection.AddSingleton(Substitute.For<ILogger<ProfileService>>());
            });
    }

    [Fact]
    public async Task UpdateLocationConfig_SetsData_CorrectDataWithOriginalProfileUntouched()
    {
        // ARRANGE
        ServiceProvider serviceProvider = getServiceProvider();

        using var dbContext = serviceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext();
        var profileService = serviceProvider.GetRequiredService<IProfileService>() as ProfileService;

        await dbContext.Profiles.AddAsync(TestDataHelper.CreateCompleteTestDynamicProfile());
        await dbContext.SaveChangesAsync();

        // in the UI the data is loaded without tracking (i.e. intended for read only)
        // changes have to be made on separate entities with tracking to keep the mechanisms clean
        var profile =
            dbContext.DynamicProfiles
                .Include(x => x.LocationConfigs)
                .Include(x => x.TimeConfigs)
                .Include(x => x.PlaceInfo).ThenInclude(x => x.TimezoneInfo)
                .AsNoTracking()
                .Single();

        ProfilePlaceInfo oldPlaceInfo = profile.PlaceInfo;
        var oldLocationDataByDynamicPrayerTimeProvider = profile.LocationConfigs.ToDictionary(x => x.DynamicPrayerTimeProvider, x => x.LocationData);

        var newPlaceInfo = new ProfilePlaceInfo
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

        var newLocationDataByDynamicPrayerTimeProvider = new Dictionary<EDynamicPrayerTimeProviderType, BaseLocationData>
        {
            [EDynamicPrayerTimeProviderType.Muwaqqit] = new MuwaqqitLocationData { Latitude = 50.9413M, Longitude = 6.9583M, TimezoneName = TestDataHelper.EUROPE_BERLIN_TIME_ZONE.Id },
            [EDynamicPrayerTimeProviderType.Fazilet] = new FaziletLocationData { CountryName = "Almanya", CityName = newPlaceInfo.City },
            [EDynamicPrayerTimeProviderType.Semerkand] = new SemerkandLocationData { CountryName = "Almanya", CityName = newPlaceInfo.City, TimezoneName = TestDataHelper.EUROPE_BERLIN_TIME_ZONE.Id },
        };

        // ACT
        await profileService.UpdateLocationConfig(profile, newPlaceInfo, default);

        // ASSERT
        dbContext.ChangeTracker.HasChanges().Should().BeFalse();

        profile.PlaceInfo.Should().NotBeNull();
        profile.PlaceInfo.Should().NotBe(oldPlaceInfo);
        profile.PlaceInfo.Should().Be(newPlaceInfo);

        foreach (var locationDataByDynamicPrayerTimeProvider in profile.LocationConfigs.ToDictionary(x => x.DynamicPrayerTimeProvider, x => x.LocationData))
        {
            BaseLocationData newValue = newLocationDataByDynamicPrayerTimeProvider[locationDataByDynamicPrayerTimeProvider.Key];
            BaseLocationData currentValue = locationDataByDynamicPrayerTimeProvider.Value;

            currentValue.Should().Be(newValue);
        }
    }

    [Fact]
    public async Task UpdateLocationConfig_SetsDataWithExceptionOnCommit_OldDataFullyRemains()
    {
        // ARRANGE
        ServiceProvider serviceProvider = getServiceProvider();

        var dbContextFactory = serviceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
        using var dbContext = dbContextFactory.CreateDbContext();

        dbContextFactory.Configure().CreateDbContext().Returns(dbContext);
        dbContextFactory.Configure().CreateDbContextAsync().Returns(Task.FromResult(dbContext));

        var profileService = serviceProvider.GetRequiredService<IProfileService>() as ProfileService;

        await dbContext.Profiles.AddAsync(TestDataHelper.CreateCompleteTestDynamicProfile());
        await dbContext.SaveChangesAsync();

        dbContext.SaveChangesAsync().Throws(new Exception("Test exception during commit"));

        var profile =
            dbContext.DynamicProfiles
                .Include(x => x.LocationConfigs)
                .Include(x => x.TimeConfigs)
                .Include(x => x.PlaceInfo).ThenInclude(x => x.TimezoneInfo)
                .AsNoTracking()
                .Single();

        ProfilePlaceInfo oldPlaceInfo = profile.PlaceInfo;
        var oldLocationDataByDynamicPrayerTimeProvider = profile.LocationConfigs.ToDictionary(x => x.DynamicPrayerTimeProvider, x => x.LocationData);
        var newLocationDataByDynamicPrayerTimeProvider = new Dictionary<EDynamicPrayerTimeProviderType, BaseLocationData>
        {
            [EDynamicPrayerTimeProviderType.Muwaqqit] = new MuwaqqitLocationData { Latitude = 50.9413M, Longitude = 6.9583M, TimezoneName = TestDataHelper.EUROPE_BERLIN_TIME_ZONE.Id },
            [EDynamicPrayerTimeProviderType.Fazilet] = new FaziletLocationData { CountryName = "Almanya", CityName = "Köln" },
            [EDynamicPrayerTimeProviderType.Semerkand] = new SemerkandLocationData { CountryName = "Almanya", CityName = "Köln", TimezoneName = TestDataHelper.EUROPE_BERLIN_TIME_ZONE.Id },
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
                    cancellationToken: default);
            };

        // ASSERT
        await updateLocationConfigFunc.Should().ThrowAsync<Exception>().WithMessage("Test exception during commit");

        profile.PlaceInfo.Should().NotBeNull();
        profile.PlaceInfo.Should().NotBe(newPlaceInfo);
        profile.PlaceInfo.Should().Be(oldPlaceInfo);

        foreach (var locationDataByDynamicPrayerTimeProvider in profile.LocationConfigs.ToDictionary(x => x.DynamicPrayerTimeProvider, x => x.LocationData))
        {
            BaseLocationData oldValue = oldLocationDataByDynamicPrayerTimeProvider[locationDataByDynamicPrayerTimeProvider.Key];
            BaseLocationData currentValue = locationDataByDynamicPrayerTimeProvider.Value;

            currentValue.Should().Be(oldValue);
        }
    }

    [Fact]
    public async Task UpdateTimeConfig_SetNewValue_Success()
    {
        // ARRANGE
        ServiceProvider serviceProvider = getServiceProvider();

        var profileService = serviceProvider.GetRequiredService<IProfileService>() as ProfileService;

        await TestArrangeDbContext.Profiles.AddAsync(TestDataHelper.CreateCompleteTestDynamicProfile());
        await TestArrangeDbContext.SaveChangesAsync();

        var profile =
            TestArrangeDbContext.DynamicProfiles
                .Include(x => x.LocationConfigs)
                .Include(x => x.TimeConfigs)
                .AsNoTracking()
                .Single();

        var newSemerkandSettingConfig =
            new GenericSettingConfiguration
            {
                Source = EDynamicPrayerTimeProviderType.Semerkand,
                TimeType = ETimeType.FajrStart
            };

        // ACT
        await profileService.UpdateTimeConfig(profile, ETimeType.FajrStart, newSemerkandSettingConfig, default);

        // ASSERT

        var fajrStartConfig = profileService.GetTimeConfig(profile, ETimeType.FajrStart);
        fajrStartConfig.Source.Should().Be(EDynamicPrayerTimeProviderType.Semerkand);
    }
}