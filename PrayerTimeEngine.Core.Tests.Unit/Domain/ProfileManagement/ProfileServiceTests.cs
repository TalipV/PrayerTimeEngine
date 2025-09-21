using Microsoft.Extensions.Logging;
using NSubstitute;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Models;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Services;
using PrayerTimeEngine.Core.Tests.Common;
using PrayerTimeEngine.Core.Tests.Common.TestData;

namespace PrayerTimeEngine.Core.Tests.Unit.Domain.ProfileManagement;

public class ProfileServiceTests : BaseTest
{
    private readonly IProfileDBAccess _profileDBAccessMock;
    private readonly IDynamicPrayerTimeProviderFactory _dynamicPrayerTimeProviderFactory;
    private readonly ProfileService _profileService;

    public ProfileServiceTests()
    {
        _profileDBAccessMock = Substitute.For<IProfileDBAccess>();
        _dynamicPrayerTimeProviderFactory = Substitute.For<IDynamicPrayerTimeProviderFactory>();
        _profileService = new ProfileService(_profileDBAccessMock, _dynamicPrayerTimeProviderFactory, new TimeTypeAttributeService(), Substitute.For<ILogger<ProfileService>>());
    }

    public static TheoryData<ETimeType> configurableTimeTypeValues => [.. new TimeTypeAttributeService().ConfigurableTypes];

    public static TheoryData<EDynamicPrayerTimeProviderType> dynamicPrayerTimeProviderValues =>
        [.. Enum.GetValues<EDynamicPrayerTimeProviderType>().Where(x => x != EDynamicPrayerTimeProviderType.None)];

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
        var profile1 = TestDataHelper.CreateCompleteTestDynamicProfile();
        var profile2 = TestDataHelper.CreateCompleteTestDynamicProfile();
        var profile3 = TestDataHelper.CreateCompleteTestDynamicProfile();
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
        var profile = TestDataHelper.CreateCompleteTestDynamicProfile();

        // ACT
        await _profileService.SaveProfile(profile, default);

        // ASSERT
        await _profileDBAccessMock.ReceivedWithAnyArgs(1).SaveProfile(default, default);
        await _profileDBAccessMock.Received(1).SaveProfile(Arg.Is(profile), Arg.Any<CancellationToken>());
    }

    #endregion SaveProfile

    #region GetTimeConfig

    [Theory]
    [MemberData(nameof(configurableTimeTypeValues))]
    [Trait("Method", "GetTimeConfig")]
    public void GetTimeConfig_ExistingTimeConfig_ShouldReturnConfig(ETimeType timeType)
    {
        // ARRANGE
        var profile = TestDataHelper.CreateCompleteTestDynamicProfile();

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
        var profile = TestDataHelper.CreateCompleteTestDynamicProfile();
        profile.TimeConfigs.Remove(profile.TimeConfigs.First(x => x.TimeType == timeType));

        // ACT
        GenericSettingConfiguration result = _profileService.GetTimeConfig(profile, timeType);

        // ASSERT
        result.Should().BeNull();
    }

    #endregion GetTimeConfig

    #region GetLocationConfig

    [Theory]
    [MemberData(nameof(dynamicPrayerTimeProviderValues))]
    [Trait("Method", "GetLocationConfig")]
    public void GetLocationConfig_MatchFound_ReturnValue(EDynamicPrayerTimeProviderType source)
    {
        // ARRANGE
        var profile = TestDataHelper.CreateCompleteTestDynamicProfile();

        // ACT
        BaseLocationData result = _profileService.GetLocationConfig(profile, source);

        // ASSERT
        result.Should().NotBeNull();
        result.Source.Should().Be(source);
    }

    [Theory]
    [MemberData(nameof(dynamicPrayerTimeProviderValues))]
    [Trait("Method", "GetLocationConfig")]
    public void GetLocationConfig_NoMatchFound_ReturnNull(EDynamicPrayerTimeProviderType source)
    {
        // ARRANGE
        var profile = TestDataHelper.CreateCompleteTestDynamicProfile();
        profile.LocationConfigs.Should().Contain(x => x.DynamicPrayerTimeProvider == source);
        profile.LocationConfigs.Remove(profile.LocationConfigs.First(x => x.DynamicPrayerTimeProvider == source));

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
        var profile = TestDataHelper.CreateCompleteTestDynamicProfile();
        var expectedDynamicPrayerTimeProviderType = Enum.GetValues<EDynamicPrayerTimeProviderType>()
            .Where(x => x != EDynamicPrayerTimeProviderType.None)
            .ToHashSet();

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
        await _profileService.UpdateLocationConfig(profile, placeInfo, default);

        // ASSERT
        await _profileDBAccessMock.ReceivedWithAnyArgs(1).UpdateLocationConfig(default, default, default, default);
        await _profileDBAccessMock.Received(1).UpdateLocationConfig(
            Arg.Is(profile), 
            Arg.Is(placeInfo), 
            Arg.Is<List<(EDynamicPrayerTimeProviderType, BaseLocationData)>>(x => x.Select(x => x.Item1).ToHashSet().SetEquals(expectedDynamicPrayerTimeProviderType)), 
            Arg.Any<CancellationToken>());
    }

    #endregion UpdateLocationConfig

    #region UpdateTimeConfig

    [Fact]
    [Trait("Method", "UpdateTimeConfig")]
    public async Task UpdateTimeConfig_UpdateProfileConfig_DbUpdateTriggeredForProfileConfig()
    {
        // ARRANGE
        var profile = TestDataHelper.CreateCompleteTestDynamicProfile();
        var setting = new GenericSettingConfiguration { TimeType = ETimeType.FajrStart };

        // ACT
        await _profileService.UpdateTimeConfig(profile, ETimeType.FajrStart, setting, default);

        // ASSERT
        await _profileDBAccessMock.ReceivedWithAnyArgs(1).UpdateTimeConfig(default, default, default, default);
        await _profileDBAccessMock.Received(1).UpdateTimeConfig(Arg.Is(profile), Arg.Is(ETimeType.FajrStart), Arg.Is(setting), Arg.Any<CancellationToken>());
    }

    #endregion UpdateTimeConfig

    #region GetLocationDataDisplayText

    [Fact]
    [Trait("Method", "GetLocationDataDisplayText")]
    public void GetLocationDataDisplayText_JustBasicExecution_NoExceptions()
    {
        // ARRANGE
        var profile = TestDataHelper.CreateCompleteTestDynamicProfile();

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
        var profile = TestDataHelper.CreateCompleteTestDynamicProfile();

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
        var profile = TestDataHelper.CreateCompleteTestDynamicProfile();

        // make two of them inactive
        profile.TimeConfigs.First().CalculationConfiguration.IsTimeShown = false;
        profile.TimeConfigs.Last().CalculationConfiguration = new GenericSettingConfiguration { Source = EDynamicPrayerTimeProviderType.None, TimeType = ETimeType.IshaEnd };

        // ACT
        List<GenericSettingConfiguration> activeComplexConfigs = _profileService.GetActiveComplexTimeConfigs(profile);

        // ASSERT
        activeComplexConfigs.Should().HaveCount(14);
        activeComplexConfigs.Should().AllSatisfy(config =>
        {
            config.IsTimeShown.Should().BeTrue();
            config.Source.Should().NotBe(EDynamicPrayerTimeProviderType.None);
        });
    }

    [Fact]
    [Trait("Method", "GetActiveComplexTimeConfigs")]
    public void GetActiveComplexTimeConfigs_ProfileWithNoConfigs_ShouldReturnNothing()
    {
        // ARRANGE
        var profile = TestDataHelper.CreateCompleteTestDynamicProfile();
        profile.TimeConfigs.Clear();

        // ACT
        List<GenericSettingConfiguration> activeComplexConfigs = _profileService.GetActiveComplexTimeConfigs(profile);

        // ASSERT
        activeComplexConfigs.Should().BeEmpty();
    }

    #endregion GetActiveComplexTimeConfigs
}