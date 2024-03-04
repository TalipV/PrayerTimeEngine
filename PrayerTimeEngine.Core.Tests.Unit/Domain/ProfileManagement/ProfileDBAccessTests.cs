using PrayerTimeEngine.Core.Domain.ProfileManagement.Services;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Tests.Common;
using FluentAssertions;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models;
using FluentAssertions.Equivalency;
using Microsoft.EntityFrameworkCore;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Models;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Models;

namespace PrayerTimeEngine.Core.Tests.Unit.Domain.ProfileManagement
{
    public class ProfileDBAccessTests : BaseTest
    {
        private readonly AppDbContext _appDbContext;
        private readonly ProfileDBAccess _profileDBAccess;

        public ProfileDBAccessTests()
        {
            _appDbContext = GetHandledDbContext();
            _profileDBAccess = new ProfileDBAccess(_appDbContext);
        }

        [Fact]
        public async Task GetProfiles_SavedThreeDifferentProfiles_RetrievedNormally()
        {
            // ARRANGE
            var profile1 = TestData.CreateNewCompleteTestProfile(profileID: 1);
            var profile2 = TestData.CreateNewCompleteTestProfile(profileID: 2);
            var profile3 = TestData.CreateNewCompleteTestProfile(profileID: 3);
            await _appDbContext.Profiles.AddAsync(profile1);
            await _appDbContext.Profiles.AddAsync(profile2);
            await _appDbContext.Profiles.AddAsync(profile3);
            await _appDbContext.SaveChangesAsync();

            // ACT
            var profiles = await _profileDBAccess.GetProfiles(default);

            // ASSERT
            profiles.Should().NotBeNull().And.HaveCount(3);
            profiles.FirstOrDefault(x => x.ID == 1).Should().BeEquivalentTo(profile1);
            profiles.FirstOrDefault(x => x.ID == 2).Should().BeEquivalentTo(profile2);
            profiles.FirstOrDefault(x => x.ID == 3).Should().BeEquivalentTo(profile3);
        }

        [Fact]
        public async Task GetUntrackedReferenceOfProfile_ExistingProfile_ProfileRetrieved()
        {
            // ARRANGE
            var profile = TestData.CreateNewCompleteTestProfile();
            await _appDbContext.Profiles.AddAsync(profile);
            await _appDbContext.SaveChangesAsync();

            // ACT
            var untracktedProfile = await _profileDBAccess.GetUntrackedReferenceOfProfile(profile.ID, default);

            // ASSERT
            profile.Should().BeEquivalentTo(untracktedProfile);
            ReferenceEquals(profile, untracktedProfile).Should().BeFalse(because: "they should be equal but not exactly the same");
            _appDbContext.Entry(untracktedProfile).State.Should().Be(Microsoft.EntityFrameworkCore.EntityState.Detached);
        }        
        
        [Fact]
        public async Task GetUntrackedReferenceOfProfile_NonExistingProfile_NothingReturned()
        {
            // ARRANGE
            // ACT
            var untracktedProfile = await _profileDBAccess.GetUntrackedReferenceOfProfile(500, default);

            // ASSERT
            untracktedProfile.Should().BeNull();
        }

        [Fact]
        public async Task SaveProfile_X_X()
        {
            // ARRANGE
            var profile = TestData.CreateNewCompleteTestProfile();

            // ACT
            await _profileDBAccess.SaveProfile(profile, default);

            // ASSERT
            _appDbContext.Profiles.SingleOrDefault().Should().BeEquivalentTo(profile);
        }

        [Fact]
        public async Task UpdateLocationConfig_X_X()
        {
            // ARRANGE
            var profile = TestData.CreateNewCompleteTestProfile();
            await _appDbContext.Profiles.AddAsync(profile);
            await _appDbContext.SaveChangesAsync();

            List<(ECalculationSource CalculationSource, BaseLocationData LocationData)> values =
                [
                    (ECalculationSource.Fazilet, new FaziletLocationData { CountryName = "DeutschlandYeah", CityName = "BerlinYeah" })
                ];

            // ACT
            await _profileDBAccess.UpdateLocationConfig(profile, "Berlin", values, default);

            // ASSERT
            profile.LocationConfigs.Should().HaveCount(1);
            profile.LocationConfigs.First().LocationData.Should().Be(values[0].LocationData);
        }

        [Fact]
        public async Task UpdateTimeConfig_X_X()
        {
            // ARRANGE
            var profile = TestData.CreateNewCompleteTestProfile();
            await _appDbContext.Profiles.AddAsync(profile);
            await _appDbContext.SaveChangesAsync();

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
    }
}
