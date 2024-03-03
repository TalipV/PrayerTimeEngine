using PrayerTimeEngine.Core.Domain.ProfileManagement.Services;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Tests.Common;
using FluentAssertions;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models;
using FluentAssertions.Equivalency;

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
        public async Task GetProfiles_X_X()
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
            _appDbContext.ProfileLocations.Remove(profile3.LocationConfigs.First());
            _appDbContext.SaveChanges();
            var profiles = await _profileDBAccess.GetProfiles();

            profile3.LocationConfigs.Clear();

            // ASSERT
            profiles.Should().NotBeNull().And.HaveCount(3);
            profiles.FirstOrDefault(x => x.ID == 1).Should().BeEquivalentTo(profile1, config: NewMethod());
            profiles.FirstOrDefault(x => x.ID == 2).Should().BeEquivalentTo(profile2, config: NewMethod());
            profiles.FirstOrDefault(x => x.ID == 3).Should().BeEquivalentTo(profile3, config: NewMethod());
        }

        private static Func<EquivalencyAssertionOptions<Profile>, EquivalencyAssertionOptions<Profile>> NewMethod()
        {
            return options =>
            {
                options.IncludingAllDeclaredProperties();
                options.IncludingAllRuntimeProperties();
                options.IncludingFields();
                options.IncludingInternalFields();
                options.IncludingInternalProperties();
                options.IncludingNestedObjects();
                options.IncludingProperties();

                options.Including(x => x.LocationConfigs);
                options.Including(x => x.TimeConfigs);

                options.WithStrictOrdering();

                options.AllowingInfiniteRecursion();

                return options;
            };
        }

        [Fact]
        public async Task GetUntrackedReferenceOfProfile_X_X()
        {
            // ARRANGE

            // ACT
            //var profile = await _profileDBAccess.GetUntrackedReferenceOfProfile();

            // ASSERT
        }

        [Fact]
        public async Task SaveProfile_X_X()
        {
            // ARRANGE

            // ACT
            //await _profileDBAccess.SaveProfile();

            // ASSERT
        }

        [Fact]
        public async Task UpdateLocationConfig_X_X()
        {
            // ARRANGE

            // ACT
            //await _profileDBAccess.UpdateLocationConfig();

            // ASSERT
        }

        [Fact]
        public async Task UpdateTimeConfig_X_X()
        {
            // ARRANGE

            // ACT
            //await _profileDBAccess.UpdateTimeConfig();

            // ASSERT
        }
    }
}
