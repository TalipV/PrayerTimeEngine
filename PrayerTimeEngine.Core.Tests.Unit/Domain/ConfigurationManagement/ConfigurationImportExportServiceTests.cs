using NSubstitute;
using PrayerTimeEngine.Core.Domain.ConfigurationManagement;
using PrayerTimeEngine.Core.Domain.ConfigurationManagement.DTOs;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;
using PrayerTimeEngine.Core.Tests.Common;
using PrayerTimeEngine.Core.Tests.Common.TestData;
using System.Text.Json;

namespace PrayerTimeEngine.Core.Tests.Unit.Domain.ConfigurationManagement;

public class ConfigurationImportExportServiceTests : BaseTest
{
    private readonly IProfileDBAccess _profileDBAccessMock;
    private readonly ConfigurationImportExportService _configurationImportExportService;

    public ConfigurationImportExportServiceTests()
    {
        _profileDBAccessMock = Substitute.For<IProfileDBAccess>();
        _configurationImportExportService = new ConfigurationImportExportService(_profileDBAccessMock);
    }

    #region SerializeConfiguration

    [Fact]
    [Trait("Method", "SerializeConfiguration")]
    public void SerializeConfiguration_TwoDynamicProfilesAndOneMosqueProfile_ExportJSONAsExpected()
    {
        // ARRANGE
        Profile[] inputProfiles = [
                TestDataHelper.CreateCompleteTestDynamicProfile(profileID: 1, profileName: "Profil One", profileSequenceNo: 2),
                TestDataHelper.CreateCompleteTestMosqueProfile(profileID: 3, profileName: "Profil Two", profileSequenceNo: 4),
                TestDataHelper.CreateCompleteTestDynamicProfile(profileID: 5, profileName: "Profil Three", profileSequenceNo: 6),
            ];
        inputProfiles = inputProfiles.OrderBy(x => x.SequenceNo).ToArray();

        var inputConfiguration = new Configuration
        {
            Profiles = inputProfiles
        };

        // ACT
        string jsonConfiguration = _configurationImportExportService.SerializeConfiguration(inputConfiguration);

        // ASSERT
        string serializedTestProfiles = File.ReadAllText(Path.Combine(TestDataHelper.CONFIGURATION_TEST_DATA_FILE_PATH, "SerializedConfigurationTwoDynamicOneMosque.txt"));
        jsonConfiguration.Should().Be(serializedTestProfiles);
    }

    #endregion SerializeConfiguration

    #region Import

    [Fact]
    [Trait("Method", "Import")]
    public async Task Import_ValidConfiguration_CallsProfileDbAccess()
    {
        // ARRANGE
        Profile[] inputProfiles = [
            TestDataHelper.CreateCompleteTestDynamicProfile(profileID: 11, profileName: "X", profileSequenceNo: 1)
        ];

        var inputConfig = new Configuration { Profiles = inputProfiles };
        string json = _configurationImportExportService.SerializeConfiguration(inputConfig);

        // ACT
        await _configurationImportExportService.Import(json, default);

        // ASSERT
        await _profileDBAccessMock.Received(1).SaveProfiles(
            Arg.Is<ICollection<Profile>>(p => p.Any(x => x.Name == "X")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Method", "Import")]
    public async Task Import_EmptyJson_NoProfilesImported()
    {
        // ARRANGE

        // ACT
        Configuration outputConfiguration = await _configurationImportExportService.Import(
            $$"""
            {
                "{{nameof(ConfigurationDTO.DynamicProfileConfigs)}}": [],
                "{{nameof(ConfigurationDTO.MosqueProfileConfigs)}}": []
            }
            """,
            default);

        // ASSERT
        outputConfiguration.Profiles.Should().BeEmpty();

        await _profileDBAccessMock.Received(1).SaveProfiles(
            Arg.Is<ICollection<Profile>>(p => p.Count == 0),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Method", "Import")]
    public async Task Import_InvalidJson_Throws()
    {
        // ARRANGE
        string invalidJson = "{ invalid json...";

        // ACT / ASSERT
        await Assert.ThrowsAsync<JsonException>(async () =>
            await _configurationImportExportService.Import(invalidJson, default));
    }

    #endregion Import
}