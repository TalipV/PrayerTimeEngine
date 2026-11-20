using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using NSubstitute;
using PrayerTimeEngine.Core.Common;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Data.EntityFramework.Generated_CompiledModels;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Muwaqqit.Models;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Waqti.Models;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;
using PrayerTimeEngine.Core.Tests.Common.TestData;

namespace PrayerTimeEngine.Core.Tests.Integration.Data.EntityFramework;

/// <summary>
/// Runs the real migrations on an SQLite database, like the app does on its start
/// (<c>Database.MigrateAsync</c>), to check that existing profiles get the Waqti location data.
/// </summary>
public sealed class AddWaqtiPrayerTimesMigrationTests : IDisposable
{
    private const string MIGRATION_BEFORE_WAQTI = "20261030185705_AddFavoritePlaces";

    private readonly SqliteConnection _connection;

    public AddWaqtiPrayerTimesMigrationTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
    }

    public void Dispose()
    {
        _connection.Dispose();
    }

    private AppDbContext CreateDbContext()
    {
        DbContextOptions options = new DbContextOptionsBuilder()
            .UseModel(AppDbContextModel.Instance)
            .UseSqlite(_connection)
            .Options;

        return new AppDbContext(options, new AppDbContextMetaData(), Substitute.For<ISystemInfoService>());
    }

    /// <summary>
    /// A profile as it was stored before Waqti existed (no Waqti location data).
    /// </summary>
    private static DynamicProfile CreateProfileWithoutWaqti(int profileID, bool withMuwaqqit = true)
    {
        DynamicProfile profile = TestDataHelper.CreateCompleteTestDynamicProfile(profileID: profileID, profileSequenceNo: profileID);
        profile.LocationConfigs =
            [.. profile.LocationConfigs.Where(x =>
                x.DynamicPrayerTimeProvider != EDynamicPrayerTimeProviderType.Waqti
                && (withMuwaqqit || x.DynamicPrayerTimeProvider != EDynamicPrayerTimeProviderType.Muwaqqit))];
        return profile;
    }

    private async Task MigrateToBeforeWaqtiAndStoreAsync(params DynamicProfile[] profiles)
    {
        using AppDbContext dbContext = CreateDbContext();
        await dbContext.GetService<IMigrator>().MigrateAsync(MIGRATION_BEFORE_WAQTI);

        await dbContext.Profiles.AddRangeAsync(profiles);
        await dbContext.SaveChangesAsync();
    }

    private async Task<List<ProfileLocationConfig>> MigrateToLatestAndGetLocationConfigsAsync()
    {
        using AppDbContext dbContext = CreateDbContext();
        await dbContext.Database.MigrateAsync();

        return await dbContext.ProfileLocations.AsNoTracking().ToListAsync();
    }

    [Fact]
    public async Task Migrate_ProfileWithMuwaqqitLocation_WaqtiLocationWithSameCoordinatesAndTimezoneAdded()
    {
        // ARRANGE
        DynamicProfile profile = CreateProfileWithoutWaqti(profileID: 1);
        var muwaqqitLocationData = (MuwaqqitLocationData)profile.LocationConfigs
            .Single(x => x.DynamicPrayerTimeProvider == EDynamicPrayerTimeProviderType.Muwaqqit).LocationData;
        await MigrateToBeforeWaqtiAndStoreAsync(profile);

        // ACT
        List<ProfileLocationConfig> locationConfigs = await MigrateToLatestAndGetLocationConfigsAsync();

        // ASSERT
        ProfileLocationConfig waqtiLocationConfig =
            locationConfigs.Single(x => x.ProfileID == 1 && x.DynamicPrayerTimeProvider == EDynamicPrayerTimeProviderType.Waqti);

        waqtiLocationConfig.LocationData.Should().BeOfType<WaqtiLocationData>();
        waqtiLocationConfig.LocationData.Source.Should().Be(EDynamicPrayerTimeProviderType.Waqti);
        waqtiLocationConfig.LocationData.Should().Be(new WaqtiLocationData
        {
            Latitude = muwaqqitLocationData.Latitude,
            Longitude = muwaqqitLocationData.Longitude,
            TimezoneName = muwaqqitLocationData.TimezoneName,
        });
    }

    [Fact]
    public async Task Migrate_ProfileWithMuwaqqitLocation_OtherLocationConfigsUntouched()
    {
        // ARRANGE
        DynamicProfile profile = CreateProfileWithoutWaqti(profileID: 1);
        var locationDataBefore = profile.LocationConfigs.ToDictionary(x => x.DynamicPrayerTimeProvider, x => x.LocationData);
        await MigrateToBeforeWaqtiAndStoreAsync(profile);

        // ACT
        List<ProfileLocationConfig> locationConfigs = await MigrateToLatestAndGetLocationConfigsAsync();

        // ASSERT
        locationConfigs.Should().HaveCount(locationDataBefore.Count + 1);
        foreach (ProfileLocationConfig locationConfig in locationConfigs.Where(x => x.DynamicPrayerTimeProvider != EDynamicPrayerTimeProviderType.Waqti))
        {
            locationConfig.LocationData.Should().Be(locationDataBefore[locationConfig.DynamicPrayerTimeProvider]);
        }
    }

    [Fact]
    public async Task Migrate_SeveralProfiles_EachGetsWaqtiLocationOnlyIfItHasMuwaqqitLocation()
    {
        // ARRANGE
        await MigrateToBeforeWaqtiAndStoreAsync(
            CreateProfileWithoutWaqti(profileID: 1),
            CreateProfileWithoutWaqti(profileID: 2, withMuwaqqit: false),
            CreateProfileWithoutWaqti(profileID: 3));

        // ACT
        List<ProfileLocationConfig> locationConfigs = await MigrateToLatestAndGetLocationConfigsAsync();

        // ASSERT
        locationConfigs
            .Where(x => x.DynamicPrayerTimeProvider == EDynamicPrayerTimeProviderType.Waqti)
            .Select(x => x.ProfileID)
            .Should().BeEquivalentTo([1, 3]);
    }

    [Fact]
    public async Task Migrate_NoProfiles_NoErrorAndWaqtiTableCreated()
    {
        // ARRANGE
        await MigrateToBeforeWaqtiAndStoreAsync();

        // ACT
        List<ProfileLocationConfig> locationConfigs = await MigrateToLatestAndGetLocationConfigsAsync();

        // ASSERT
        locationConfigs.Should().BeEmpty();
        using AppDbContext dbContext = CreateDbContext();
        (await dbContext.WaqtiPrayerTimes.CountAsync()).Should().Be(0);
    }
}
