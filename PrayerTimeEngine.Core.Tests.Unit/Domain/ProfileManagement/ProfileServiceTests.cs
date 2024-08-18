using NSubstitute;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain;
using PrayerTimeEngine.Core.Domain.Models;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Models;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Services;
using PrayerTimeEngine.Core.Tests.Common;
using PrayerTimeEngine.Core.Tests.Common.TestData;

namespace PrayerTimeEngine.Core.Tests.Unit.Domain.ProfileManagement
{
    public class ProfileServiceTests : BaseTest
    {
        private readonly IProfileDBAccess _profileDBAccessMock;
        private readonly ProfileService _profileService;

        public ProfileServiceTests()
        {
            _profileDBAccessMock = Substitute.For<IProfileDBAccess>();
            _profileService = new ProfileService(_profileDBAccessMock, new TimeTypeAttributeService());
        }

        public static TheoryData<ETimeType> configurableTimeTypeValues => new(
            values: new TimeTypeAttributeService().ConfigurableTypes);

        public static TheoryData<ECalculationSource> calculationSourceValues => new(
            values: Enum.GetValues(typeof(ECalculationSource))
                    .OfType<ECalculationSource>()
                    .Where(x => x != ECalculationSource.None));

        #region GetProfiles

        [Fact]
        [Trait("Method", "GetProfiles")]
        public async Task GetProfiles_NoProfilesInDb_ReturnDefaultProfile()
        {
            // ARRANGE
            _profileDBAccessMock.GetProfiles(Arg.Any<CancellationToken>()).Returns([]);

            // ACT
            List<Profile> profiles = await _profileService.GetProfiles(default);

            // ASSERT
            profiles.Should().HaveCount(1);
            profiles.First().ID.Should().Be(1);
            profiles.First().Name.Should().Be("Standard-Profil");
        }

        [Fact]
        [Trait("Method", "GetProfiles")]
        public async Task GetProfiles_ThreeProfilesInDb_ReturnTheThree()
        {
            // ARRANGE
            var profile1 = TestDataHelper.CreateNewCompleteTestProfile();
            var profile2 = TestDataHelper.CreateNewCompleteTestProfile();
            var profile3 = TestDataHelper.CreateNewCompleteTestProfile();
            _profileDBAccessMock.GetProfiles(Arg.Any<CancellationToken>()).Returns([profile1, profile2, profile3]);

            // ACT
            List<Profile> profiles = await _profileService.GetProfiles(default);

            // ASSERT
            profiles.Should().HaveCount(3);
            profiles.Should().Contain(profile1);
            profiles.Should().Contain(profile2);
            profiles.Should().Contain(profile3);
        }

        #endregion GetProfiles

        #region SaveProfile

        [Fact]
        [Trait("Method", "SaveProfile")]
        public async Task SaveProfile_SaveSomeProfile_DbSaveTriggeredForProfile()
        {
            // ARRANGE
            var profile = TestDataHelper.CreateNewCompleteTestProfile();

            // ACT
            await _profileService.SaveProfile(profile, default);

            // ASSERT
            _profileDBAccessMock.ReceivedWithAnyArgs(1).Awaiting(x => x.SaveProfile(default, default));
            _profileDBAccessMock.Received(1).Awaiting(x => x.SaveProfile(Arg.Is(profile), Arg.Any<CancellationToken>()));
        }

        #endregion SaveProfile

        #region GetTimeConfig

        [Theory]
        [MemberData(nameof(configurableTimeTypeValues))]
        [Trait("Method", "GetTimeConfig")]
        public void GetTimeConfig_ExistingTimeConfig_ShouldReturnConfig(ETimeType timeType)
        {
            // ARRANGE
            var profile = TestDataHelper.CreateNewCompleteTestProfile();

            // ACT
            GenericSettingConfiguration result = _profileService.GetTimeConfig(profile, timeType);

            // ASSERT
            result.Should().NotBeNull();
            result.TimeType.Should().Be(timeType);
        }

        [Theory]
        [MemberData(nameof(configurableTimeTypeValues))]
        [Trait("Method", "GetTimeConfig")]
        public void GetTimeConfig_NonExistingTimeConfig_ShouldReturnNull(ETimeType timeType)
        {
            // ARRANGE
            var profile = TestDataHelper.CreateNewCompleteTestProfile();
            profile.TimeConfigs.Remove(profile.TimeConfigs.First(x => x.TimeType == timeType));

            // ACT
            GenericSettingConfiguration result = _profileService.GetTimeConfig(profile, timeType);

            // ASSERT
            result.Should().BeNull();
        }

        #endregion GetTimeConfig

        #region GetLocationConfig

        [Theory]
        [MemberData(nameof(calculationSourceValues))]
        [Trait("Method", "GetLocationConfig")]
        public void GetLocationConfig_MatchFound_ReturnValue(ECalculationSource source)
        {
            // ARRANGE
            var profile = TestDataHelper.CreateNewCompleteTestProfile();

            // ACT
            BaseLocationData result = _profileService.GetLocationConfig(profile, source);

            // ASSERT
            result.Should().NotBeNull();
            result.Source.Should().Be(source);
        }

        [Theory]
        [MemberData(nameof(calculationSourceValues))]
        [Trait("Method", "GetLocationConfig")]
        public void GetLocationConfig_NoMatchFound_ReturnNull(ECalculationSource source)
        {
            // ARRANGE
            var profile = TestDataHelper.CreateNewCompleteTestProfile();
            profile.LocationConfigs.Should().Contain(x => x.CalculationSource == source);
            profile.LocationConfigs.Remove(profile.LocationConfigs.First(x => x.CalculationSource == source));

            // ACT
            BaseLocationData result = _profileService.GetLocationConfig(profile, source);

            // ASSERT
            result.Should().BeNull();
        }

        #endregion GetLocationConfig

        #region UpdateLocationConfig

        [Fact]
        [Trait("Method", "UpdateLocationConfig")]
        public async Task UpdateLocationConfig_UpdateProfileLocationData_DbUpdateTriggeredForProfileLocationData()
        {
            // ARRANGE
            var profile = TestDataHelper.CreateNewCompleteTestProfile();
            List<(ECalculationSource, BaseLocationData)> locationData = [(ECalculationSource.Muwaqqit, Substitute.ForPartsOf<BaseLocationData>())];
            
            var placeInfo =
                new CompletePlaceInfo
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
            await _profileService.UpdateLocationConfig(profile, placeInfo, locationData, default);

            // ASSERT
            _profileDBAccessMock.ReceivedWithAnyArgs(1).Awaiting(x => x.UpdateLocationConfig(default, default, default, default));
            _profileDBAccessMock.Received(1).Awaiting(x => x.UpdateLocationConfig(Arg.Is(profile), Arg.Is(placeInfo), Arg.Is(locationData), Arg.Any<CancellationToken>()));
        }

        #endregion UpdateLocationConfig

        #region UpdateTimeConfig

        [Fact]
        [Trait("Method", "UpdateTimeConfig")]
        public async Task UpdateTimeConfig_UpdateProfileConfig_DbUpdateTriggeredForProfileConfig()
        {
            // ARRANGE
            var profile = TestDataHelper.CreateNewCompleteTestProfile();
            var setting = new GenericSettingConfiguration { TimeType = ETimeType.FajrStart };

            // ACT
            await _profileService.UpdateTimeConfig(profile, ETimeType.FajrStart, setting, default);

            // ASSERT
            _profileDBAccessMock.ReceivedWithAnyArgs(1).Awaiting(x => x.UpdateTimeConfig(default, default, default, default));
            _profileDBAccessMock.Received(1).Awaiting(x => x.UpdateTimeConfig(Arg.Is(profile), Arg.Is(ETimeType.FajrStart), Arg.Is(setting), Arg.Any<CancellationToken>()));
        }

        #endregion UpdateTimeConfig

        #region GetLocationDataDisplayText

        [Fact]
        [Trait("Method", "GetLocationDataDisplayText")]
        public void GetLocationDataDisplayText_JustBasicExecution_NoExceptions()
        {
            // ARRANGE
            var profile = TestDataHelper.CreateNewCompleteTestProfile();

            // ACT
            Action execution = () => _profileService.GetLocationDataDisplayText(profile);

            // ASSERT
            execution.Should().NotThrow();
        }

        #endregion GetLocationDataDisplayText

        #region GetPrayerTimeConfigDisplayText

        [Fact]
        [Trait("Method", "GetPrayerTimeConfigDisplayText")]
        public void GetPrayerTimeConfigDisplayText_JustBasicExecution_NoExceptions()
        {
            // ARRANGE
            var profile = TestDataHelper.CreateNewCompleteTestProfile();

            // ACT
            Action execution = () => _profileService.GetPrayerTimeConfigDisplayText(profile);

            // ASSERT
            execution.Should().NotThrow();
        }

        #endregion GetPrayerTimeConfigDisplayText

        #region GetActiveComplexTimeConfigs

        [Fact]
        [Trait("Method", "GetActiveComplexTimeConfigs")]
        public void GetActiveComplexTimeConfigs_ProfileWithMostlyActiveConfigs_ShouldReturnActiveConfigs()
        {
            // ARRANGE
            var profile = TestDataHelper.CreateNewCompleteTestProfile();

            // make two of them inactive
            profile.TimeConfigs.First().CalculationConfiguration.IsTimeShown = false;
            profile.TimeConfigs.Last().CalculationConfiguration = new GenericSettingConfiguration { Source = ECalculationSource.None, TimeType = ETimeType.IshaEnd };

            // ACT
            List<GenericSettingConfiguration> activeComplexConfigs = _profileService.GetActiveComplexTimeConfigs(profile);

            // ASSERT
            activeComplexConfigs.Should().HaveCount(14);
            activeComplexConfigs.Should().AllSatisfy(config =>
            {
                config.IsTimeShown.Should().BeTrue();
                config.Source.Should().NotBe(ECalculationSource.None);
            });
        }        
        
        [Fact]
        [Trait("Method", "GetActiveComplexTimeConfigs")]
        public void GetActiveComplexTimeConfigs_ProfileWithNoConfigs_ShouldReturnNothing()
        {
            // ARRANGE
            var profile = TestDataHelper.CreateNewCompleteTestProfile();
            profile.TimeConfigs.Clear();

            // ACT
            List<GenericSettingConfiguration> activeComplexConfigs = _profileService.GetActiveComplexTimeConfigs(profile);

            // ASSERT
            activeComplexConfigs.Should().BeEmpty();
        }

        #endregion GetActiveComplexTimeConfigs
    }
}