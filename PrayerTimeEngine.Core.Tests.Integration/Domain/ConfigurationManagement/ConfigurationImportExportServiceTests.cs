using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain.ConfigurationManagement;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Services;
using PrayerTimeEngine.Core.Tests.Common;
using PrayerTimeEngine.Core.Tests.Common.TestData;

namespace PrayerTimeEngine.Core.Tests.Integration.Domain.ConfigurationManagement;

public class ConfigurationImportExportServiceTests : BaseTest
{
    private ServiceProvider getServiceProvider()
    {
        return createServiceProvider(
            serviceCollection =>
            {
                serviceCollection.AddSingleton(GetHandledDbContextFactory());
                serviceCollection.AddSingleton<IProfileDBAccess, ProfileDBAccess>();
                serviceCollection.AddSingleton<ConfigurationImportExportService>();
            });
    }

    [Fact]
    [Trait("Method", "SerializeConfigurationANDImport")]
    public async Task SerializeConfigurationANDImport_TwoDynamicProfilesAndOneMosqueProfile_ImportedProfilesAsExportedOnes()
    {
        // ARRANGE
        ServiceProvider serviceProvider = getServiceProvider();
        using var dbContext = serviceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext();

        var configurationImportExportService = serviceProvider.GetRequiredService<ConfigurationImportExportService>();

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
        string jsonConfiguration = configurationImportExportService.SerializeConfiguration(inputConfiguration);
        Configuration outputConfiguration = await configurationImportExportService.Import(jsonConfiguration, default);

        // ASSERT
        (await dbContext.DynamicProfiles.CountAsync()).Should().Be(2);
        (await dbContext.MosqueProfiles.CountAsync()).Should().Be(1);
        Profile[] outputProfiles = outputConfiguration.Profiles
            .OrderBy(x => x.SequenceNo)
            .Should().HaveCount(3).And.Subject
            .ToArray();

        outputProfiles.Should().BeEquivalentTo(
            inputProfiles,
            config: AssertionConfigurations.MixedProfileContentEquivalency
        );
    }

    #region SerializeConfiguration

    // NOTHING because this method doesn't rely on dependencies and therefore can only be 'unit' tested

    #endregion SerializeConfiguration

    #region Import

    [Fact]
    [Trait("Method", "Import")]
    public async Task Import_TwoDynamicProfilesAndOneMosqueProfile_ImportedProfilesAsExpected()
    {
        // ARRANGE
        ServiceProvider serviceProvider = getServiceProvider();
        using var dbContext = serviceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext();

        var configurationImportExportService = serviceProvider.GetRequiredService<ConfigurationImportExportService>();

        Profile[] inputProfiles = [
                TestDataHelper.CreateCompleteTestDynamicProfile(profileID: 1, profileName: "Profil One", profileSequenceNo: 2),
                TestDataHelper.CreateCompleteTestMosqueProfile(profileID: 3, profileName: "Profil Two", profileSequenceNo: 4),
                TestDataHelper.CreateCompleteTestDynamicProfile(profileID: 5, profileName: "Profil Three", profileSequenceNo: 6),
            ];
        inputProfiles = inputProfiles.OrderBy(x => x.SequenceNo).ToArray();

        string serializedTestProfiles = File.ReadAllText(Path.Combine(TestDataHelper.CONFIGURATION_TEST_DATA_FILE_PATH, "SerializedConfigurationTwoDynamicOneMosque.txt"));

        // ACT
        Configuration outputConfiguration = await configurationImportExportService.Import(serializedTestProfiles, default);

        // ASSERT
        (await dbContext.DynamicProfiles.CountAsync()).Should().Be(2);
        (await dbContext.MosqueProfiles.CountAsync()).Should().Be(1);
        Profile[] outputProfiles = outputConfiguration.Profiles
            .OrderBy(x => x.SequenceNo)
            .Should().HaveCount(3).And.Subject
            .ToArray();

        outputProfiles.Should().BeEquivalentTo(
            inputProfiles,
            AssertionConfigurations.MixedProfileContentEquivalency
        );
    }

    #endregion Import
}