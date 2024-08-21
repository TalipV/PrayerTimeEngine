using PrayerTimeEngine.Core.Domain.ProfileManagement.Services;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Tests.Common;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Models;
using PrayerTimeEngine.Core.Tests.Common.TestData;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace PrayerTimeEngine.Core.Tests.Unit.Domain.ProfileManagement
{
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

            List<(ECalculationSource CalculationSource, BaseLocationData LocationData)> values =
                [
                    (ECalculationSource.Fazilet, new FaziletLocationData { CountryName = "DeutschlandYeah", CityName = "BerlinYeah" })
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
                Source = ECalculationSource.Muwaqqit,
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
    }
}
