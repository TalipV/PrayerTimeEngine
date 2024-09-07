using PrayerTimeEngine.Core.Domain.ProfileManagement.Services;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Tests.Common;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Tests.Common.TestData;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Models;
using Microsoft.EntityFrameworkCore;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Models;

namespace PrayerTimeEngine.Core.Tests.Unit.Domain.ProfileManagement;

public class ProfileDBAccessTests : BaseTest
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    private readonly ProfileDBAccess _profileDBAccess;

    public ProfileDBAccessTests()
    {
        _dbContextFactory = GetHandledDbContextFactory();
        _profileDBAccess = new ProfileDBAccess(_dbContextFactory);
    }

    #region GetProfiles

    [Fact]
    [Trait("Method", "GetProfiles")]
    public async Task GetProfiles_SavedThreeDifferentProfiles_RetrievedNormally()
    {
        // ARRANGE
        var profile1 = TestDataHelper.CreateNewCompleteTestProfile(profileID: 1);
        var profile2 = TestDataHelper.CreateNewCompleteTestProfile(profileID: 2);
        var profile3 = TestDataHelper.CreateNewCompleteTestProfile(profileID: 3);
        await TestArrangeDbContext.Profiles.AddAsync(profile1);
        await TestArrangeDbContext.Profiles.AddAsync(profile2);
        await TestArrangeDbContext.Profiles.AddAsync(profile3);
        await TestArrangeDbContext.SaveChangesAsync();

        // ACT
        var profiles = await _profileDBAccess.GetProfiles(default);

        // ASSERT
        profiles.Should().NotBeNull().And.HaveCount(3);
        profiles.FirstOrDefault(x => x.ID == 1).Should().BeEquivalentTo(profile1);
        profiles.FirstOrDefault(x => x.ID == 2).Should().BeEquivalentTo(profile2);
        profiles.FirstOrDefault(x => x.ID == 3).Should().BeEquivalentTo(profile3);
    }

    #endregion GetProfiles

    #region GetUntrackedReferenceOfProfile

    [Fact]
    [Trait("Method", "GetUntrackedReferenceOfProfile")]
    public async Task GetUntrackedReferenceOfProfile_ExistingProfile_ProfileRetrieved()
    {
        // ARRANGE
        var profile = TestDataHelper.CreateNewCompleteTestProfile();
        await TestArrangeDbContext.Profiles.AddAsync(profile);
        await TestArrangeDbContext.SaveChangesAsync();

        // ACT
        var untracktedProfile = await _profileDBAccess.GetUntrackedReferenceOfProfile(profile.ID, default);

        // ASSERT
        profile.Should().BeEquivalentTo(untracktedProfile);
        ReferenceEquals(profile, untracktedProfile).Should().BeFalse(because: "they should be equal but not exactly the same");
        TestAssertDbContext.Entry(untracktedProfile).State.Should().Be(Microsoft.EntityFrameworkCore.EntityState.Detached);
    }        
    
    [Fact]
    [Trait("Method", "GetUntrackedReferenceOfProfile")]
    public async Task GetUntrackedReferenceOfProfile_NonExistingProfile_NothingReturned()
    {
        // ARRANGE
        // ACT
        var untracktedProfile = await _profileDBAccess.GetUntrackedReferenceOfProfile(500, default);

        // ASSERT
        untracktedProfile.Should().BeNull();
    }

    #endregion GetUntrackedReferenceOfProfile

    #region SaveProfile

    [Fact]
    [Trait("Method", "SaveProfile")]
    public async Task SaveProfile_BasicProfile_SavedInDb()
    {
        // ARRANGE
        var profile = TestDataHelper.CreateNewCompleteTestProfile();

        // ACT
        await _profileDBAccess.SaveProfile(profile, default);

        // ASSERT
        var savedProfile =
            TestAssertDbContext.Profiles
            .Include(x => x.PlaceInfo).ThenInclude(x => x.TimezoneInfo)
            .Include(x => x.TimeConfigs)
            .Include(x => x.LocationConfigs)
            .SingleOrDefault();

        savedProfile.Should().BeEquivalentTo(profile);
    }

    #endregion SaveProfile

    #region UpdateLocationConfig

    [Fact]
    [Trait("Method", "UpdateLocationConfig")]
    public async Task UpdateLocationConfig_SingleLocation_OnlySingleLocationInProfile()
    {
        // ARRANGE
        var profile = TestDataHelper.CreateNewCompleteTestProfile();
        await TestArrangeDbContext.Profiles.AddAsync(profile);
        await TestArrangeDbContext.SaveChangesAsync();

        List<(EDynamicPrayerTimeProviderType DynamicPrayerTimeProvider, BaseLocationData LocationData)> values =
            [
                (EDynamicPrayerTimeProviderType.Fazilet, new FaziletLocationData { CountryName = "DeutschlandYeah", CityName = "BerlinYeah" })
            ];

        var placeInfo =
            new ProfilePlaceInfo
            {
                ExternalID = "1",
                Longitude = 1M,
                Latitude = 1M,
                InfoLanguageCode = "de",
                Country = "Deutschland",
                City = "Berlin",
                CityDistrict = "",
                PostCode = "",
                Street = "",
                TimezoneInfo = new TimezoneInfo
                {
                    DisplayName = "",
                    Name = "",
                    UtcOffsetSeconds = 0
                }
            };

        // ACT
        await _profileDBAccess.UpdateLocationConfig(profile, placeInfo, values, default);

        // ASSERT
        profile.LocationConfigs.Should().HaveCount(1);
        profile.LocationConfigs.First().LocationData.Should().Be(values[0].LocationData);
    }

    #endregion UpdateLocationConfig

    #region UpdateTimeConfig

    [Fact]
    [Trait("Method", "UpdateTimeConfig")]
    public async Task UpdateTimeConfig_SingleUpdate_UpdatedAsExpected()
    {
        // ARRANGE
        var profile = TestDataHelper.CreateNewCompleteTestProfile();
        await TestArrangeDbContext.Profiles.AddAsync(profile);
        await TestArrangeDbContext.SaveChangesAsync();

        var genericSettingConfiguration = new GenericSettingConfiguration
        {
            Source = EDynamicPrayerTimeProviderType.Muwaqqit,
            TimeType = ETimeType.FajrEnd,
            MinuteAdjustment = 11,
            IsTimeShown = false
        };

        // ACT
        await _profileDBAccess.UpdateTimeConfig(profile, ETimeType.FajrEnd, genericSettingConfiguration, default);

        // ASSERT
        profile.TimeConfigs.Should().Contain(x => x.CalculationConfiguration.Equals(genericSettingConfiguration));
        profile.TimeConfigs.First(x => x.CalculationConfiguration.Equals(genericSettingConfiguration)).CalculationConfiguration.Should().BeEquivalentTo(genericSettingConfiguration);
    }

    #endregion UpdateTimeConfig

    #region CopyProfile

    [Fact]
    [Trait("Method", "CopyProfile")]
    public async Task CopyProfile_BasicTestProfile_NewProfileWithNewRelatedData()
    {
        // ARRANGE
        var profile = TestDataHelper.CreateNewCompleteTestProfile();
        await TestArrangeDbContext.Profiles.AddAsync(profile);
        await TestArrangeDbContext.SaveChangesAsync();
        profile.ID.Should().Be(1, "this is a precondition");
        profile.SequenceNo.Should().Be(1, "this is a precondition");

        // ACT
        Profile copiedProfile = await _profileDBAccess.CopyProfile(profile, default);

        // ASSERT
        copiedProfile.Should().NotBeNull();
        copiedProfile.PlaceInfo.Should().NotBeNull();
        copiedProfile.PlaceInfo.TimezoneInfo.Should().NotBeNull();
        copiedProfile.LocationConfigs.Should().HaveSameCount(profile.LocationConfigs);
        copiedProfile.TimeConfigs.Should().HaveSameCount(profile.TimeConfigs);

        copiedProfile.Should()
            .BeEquivalentTo(profile, options => options
                    .ComparingByMembers<Profile>()
                    .ComparingByMembers<ProfilePlaceInfo>()
                    .ComparingByMembers<TimezoneInfo>()
                    .ComparingByMembers<ProfileLocationConfig>()
                    .ComparingByMembers<ProfileTimeConfig>()
                    .Excluding(x => x.ID)
                    .Excluding(x => x.SequenceNo)
                    .Excluding(x => x.InsertInstant)
                    .Excluding(x => x.PlaceInfo.ID)
                    .Excluding(x => x.PlaceInfo.ProfileID)
                    .Excluding(x => x.PlaceInfo.Profile)
                    .Excluding(x => x.PlaceInfo.InsertInstant)
                    .Excluding(x => x.PlaceInfo.TimezoneInfo.ID)
                    .Excluding(x => x.PlaceInfo.TimezoneInfo.InsertInstant)
                    .For(x => x.LocationConfigs).Exclude(x => x.ID)
                    .For(x => x.LocationConfigs).Exclude(x => x.ProfileID)
                    .For(x => x.LocationConfigs).Exclude(x => x.Profile)
                    .For(x => x.LocationConfigs).Exclude(x => x.InsertInstant)
                    .For(x => x.TimeConfigs).Exclude(x => x.ID)
                    .For(x => x.TimeConfigs).Exclude(x => x.ProfileID)
                    .For(x => x.TimeConfigs).Exclude(x => x.Profile)
                    .For(x => x.TimeConfigs).Exclude(x => x.InsertInstant)
                );

        copiedProfile.ID.Should().Be(2);
        copiedProfile.SequenceNo.Should().Be(2);

        copiedProfile.PlaceInfo.ID.Should().Be(2);
        copiedProfile.PlaceInfo.ProfileID.Should().Be(2);
        copiedProfile.PlaceInfo.Profile.Should().Be(copiedProfile);

        copiedProfile.PlaceInfo.TimezoneInfo.ID.Should().Be(2);

        copiedProfile.LocationConfigs
            .Should().AllSatisfy(locationConfig =>
            {
                locationConfig.ProfileID.Should().Be(2);
                locationConfig.Profile.Should().Be(copiedProfile);
            });

        copiedProfile.TimeConfigs
            .Should().AllSatisfy(timeConfig =>
            {
                timeConfig.ProfileID.Should().Be(2);
                timeConfig.Profile.Should().Be(copiedProfile);
            });
    }

    #endregion CopyProfile

    #region DeleteProfile

    [Fact]
    [Trait("Method", "DeleteProfile")]
    public async Task DeleteProfile_CopyTestProfileAndDeleteOriginalProfile_OriginalProfileDeleted()
    {
        // ARRANGE
        var profile = TestDataHelper.CreateNewCompleteTestProfile();
        await TestArrangeDbContext.Profiles.AddAsync(profile);
        await TestArrangeDbContext.SaveChangesAsync();
        profile.ID.Should().Be(1, "this is a precondition");
        profile.SequenceNo.Should().Be(1, "this is a precondition");
        Profile copiedProfile = await _profileDBAccess.CopyProfile(profile, default);

        // ACT 
        await _profileDBAccess.DeleteProfile(profile, default);

        // ASSERT
        var profiles = 
            await TestAssertDbContext
                .Profiles
                .Include(x => x.PlaceInfo).ThenInclude(x => x.TimezoneInfo)
                .Include(x => x.LocationConfigs)
                .Include(x => x.TimeConfigs)
                .ToListAsync(default);
        profiles.Should().HaveCount(1);
        profiles.First().Should().BeEquivalentTo(copiedProfile);
    }

    #endregion DeleteProfile
}
