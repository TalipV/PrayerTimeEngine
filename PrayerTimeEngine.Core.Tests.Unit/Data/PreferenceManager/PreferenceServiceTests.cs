using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.Extensions;
using PrayerTimeEngine.Core.Data.PreferenceManager;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models;
using PrayerTimeEngine.Core.Tests.Common;

namespace PrayerTimeEngine.Core.Tests.Unit.Data.PreferenceManager
{
    public class PreferenceServiceTests : BaseTest
    {
        private readonly IPreferenceAccess _preferenceAccessMock;
        private readonly PreferenceService _preferenceService;

        public PreferenceServiceTests()
        {
            _preferenceAccessMock = Substitute.For<IPreferenceAccess>();
            _preferenceService = new PreferenceService(_preferenceAccessMock);
        }

        [Fact]
        public void SaveCurrentDataAndGetCurrentProfile_SaveProfileAndGetProfile_SameProfileAsBefore()
        {
            // ARRANGE
            var profile = TestData.CreateNewCompleteTestProfile();

            // ACT
            string savedProfileJsonValue = null;
            _preferenceAccessMock.SetValue(Arg.Is("Profile"), Arg.Do<string>(x => savedProfileJsonValue = x));
            _preferenceService.SaveCurrentData(profile, new PrayerTimesBundle());
            _preferenceAccessMock.GetValue("Profile", Arg.Any<string>()).Returns(savedProfileJsonValue);
            Profile retrievedProfile = _preferenceService.GetCurrentProfile();

            // ASSERT
            retrievedProfile.Should().NotBeNull().And.Be(profile);
            equalsLocationConfigs(profile.LocationConfigs, retrievedProfile.LocationConfigs).Should().BeTrue();
            equalsTimeConfigs(profile.TimeConfigs, retrievedProfile.TimeConfigs).Should().BeTrue();
        }

        [Fact]
        public void PreferenceService_SetAndGetPrayerTimeBundle_SamePrayerTimeBundleAsBefore()
        {
            // ARRANGE
            var profile = new Profile() { ID = 1 };
            var bundle = TestData.CreateNewTestPrayerTimesBundle();

            // ACT
            string jsonValuePrayerTimeBundle = null;
            _preferenceAccessMock.SetValue(Arg.Is("PrayerTimes_1"), Arg.Do<string>(x => jsonValuePrayerTimeBundle = x));
            _preferenceService.SaveCurrentData(profile, bundle);
            _preferenceAccessMock.Configure().GetValue("PrayerTimes_1", Arg.Any<string>()).Returns(jsonValuePrayerTimeBundle);
            PrayerTimesBundle retrievedBundle = _preferenceService.GetCurrentData(profile);

            // ASSERT
            retrievedBundle.Should().NotBeNull().And.Be(bundle);
        }

        private bool equalsLocationConfigs(
            ICollection<ProfileLocationConfig> profileLocationConfigs1,
            ICollection<ProfileLocationConfig> profileLocationConfigs2)
        {
            var array1 = profileLocationConfigs1.OrderBy(x => x.CalculationSource).ToArray();
            var array2 = profileLocationConfigs2.OrderBy(x => x.CalculationSource).ToArray();

            if (array1.Length != array2.Length)
                return false;

            for (int i = 0; i < array1.Length; i++)
            {
                ProfileLocationConfig locationConfig1 = array1[i];
                ProfileLocationConfig locationConfig2 = array2[i];

                if (!locationConfig1.Equals(locationConfig2))
                    return false;
            }

            return true;
        }

        private bool equalsTimeConfigs(
            ICollection<ProfileTimeConfig> profileTimeConfigs1,
            ICollection<ProfileTimeConfig> profileTimeConfigs2)
        {
            var array1 = profileTimeConfigs1.OrderBy(x => x.TimeType).ToArray();
            var array2 = profileTimeConfigs2.OrderBy(x => x.TimeType).ToArray();

            if (array1.Length != array2.Length)
                return false;

            for (int i = 0; i < array1.Length; i++)
            {
                ProfileTimeConfig profileTimeConfig1 = array1[i];
                ProfileTimeConfig profileTimeConfig2 = array2[i];

                if (!profileTimeConfig1.Equals(profileTimeConfig2))
                    return false;
            }

            return true;
        }
    }
}