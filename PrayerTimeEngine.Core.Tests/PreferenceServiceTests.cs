using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NSubstitute.Extensions;
using PrayerTimeEngine.Core.Data.Preferences;
using PrayerTimeEngine.Core.Domain.Configuration.Models;
using PrayerTimeEngine.Core.Domain.Model;

namespace PrayerTimeEngine.Core.Tests
{
    public class PreferenceServiceTests : BaseTest
    {
        protected override void ConfigureServiceProvider(ServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton(Substitute.For<IPreferenceAccess>());
            serviceCollection.AddSingleton<PreferenceService>();
        }

        [Test]
        public void PreferenceService_SetAndGetProfile_SameProfileAsBefore()
        {
            // ARRANGE
            PreferenceService preferenceService = ServiceProvider.GetService<PreferenceService>();
            IPreferenceAccess preferenceAccess = ServiceProvider.GetService<IPreferenceAccess>();

            var profile = TestData.CreateNewTestProfile();

            // ACT
            string jsonValueProfile = null;
            preferenceAccess.SetValue(Arg.Is("Profile"), Arg.Do<string>(x => jsonValueProfile = x));
            preferenceService.SaveCurrentData(profile, new PrayerTimesBundle());
            preferenceAccess.Configure().GetValue("Profile", Arg.Any<string>()).Returns(jsonValueProfile);

            var retrievedProfile = preferenceService.GetCurrentProfile();

            // ASSERT
            Assert.IsNotNull(retrievedProfile);
            Assert.That(retrievedProfile, Is.EqualTo(profile));
            Assert.IsTrue(equalsLocationConfigs(profile.LocationConfigs, retrievedProfile.LocationConfigs));
            Assert.IsTrue(equalsTimeConfigs(profile.TimeConfigs, retrievedProfile.TimeConfigs));
        }

        [Test]
        public void PreferenceService_SetAndGetPrayerTimeBundle_SamePrayerTimeBundleAsBefore()
        {
            // ARRANGE
            PreferenceService preferenceService = ServiceProvider.GetService<PreferenceService>();
            IPreferenceAccess preferenceAccess = ServiceProvider.GetService<IPreferenceAccess>();

            var profile = new Profile() { ID = 1 };
            var bundle = TestData.CreateNewTestPrayerTimesBundle();

            // ACT
            string jsonValuePrayerTimeBundle = null;
            preferenceAccess.SetValue(Arg.Is("PrayerTimes_1"), Arg.Do<string>(x => jsonValuePrayerTimeBundle = x));
            preferenceService.SaveCurrentData(profile, bundle);
            preferenceAccess.Configure().GetValue("PrayerTimes_1", Arg.Any<string>()).Returns(jsonValuePrayerTimeBundle);

            var retrievedBundle = preferenceService.GetCurrentData(profile);

            // ASSERT
            Assert.IsNotNull(retrievedBundle);
            Assert.That(retrievedBundle, Is.EqualTo(bundle));
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