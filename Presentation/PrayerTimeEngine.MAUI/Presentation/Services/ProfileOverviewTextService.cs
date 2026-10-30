using System.Text;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Models;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Muwaqqit.Models;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Models;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;
using PrayerTimeEngine.Presentation.Localization;

namespace PrayerTimeEngine.Presentation.Services;

/// <summary>
/// The texts of the overview dialogs of the options menu. Built here from the profile data instead
/// of in the application layer, so they can use the translated texts of <see cref="AppResources"/>.
/// </summary>
public class ProfileOverviewTextService(
        IProfileService profileService,
        TimeTypeAttributeService timeTypeAttributeService
    )
{
    public string GetTimeConfigurationText(DynamicProfile profile)
    {
        var text = new StringBuilder();

        foreach (KeyValuePair<ETimeSection, List<ETimeType>> sectionTimeTypes in timeTypeAttributeService.SectionToTimeTypes)
        {
            text.AppendLine(LocalizedNames.GetSectionName(sectionTimeTypes.Key));

            foreach (ETimeType timeType in sectionTimeTypes.Value)
            {
                if (!timeTypeAttributeService.ConfigurableTypes.Contains(timeType))
                {
                    continue;
                }

                // profiles which were saved before a time type was introduced have no config for it
                if (profileService.GetTimeConfig(profile, timeType) is not GenericSettingConfiguration config)
                {
                    continue;
                }

                text.Append("- ");
                text.Append(string.Format(AppResources.Overview_TimeConfigurationEntry, LocalizedNames.GetTimeTypeName(timeType), config.Source));

                if (config is MuwaqqitDegreeCalculationConfiguration degreeConfig)
                {
                    text.Append($" ({string.Format(AppResources.Overview_Degree, degreeConfig.Degree)})");
                }

                if (config.MinuteAdjustment != 0)
                {
                    text.Append($", {string.Format(AppResources.Overview_MinuteAdjustment, config.MinuteAdjustment)}");
                }

                text.AppendLine();
            }

            text.AppendLine();
        }

        return text.ToString().TrimEnd();
    }

    public string GetLocationDataText(DynamicProfile profile)
    {
        var muwaqqitLocationData = profileService.GetLocationConfig(profile, EDynamicPrayerTimeProviderType.Muwaqqit) as MuwaqqitLocationData;
        var faziletLocationData = profileService.GetLocationConfig(profile, EDynamicPrayerTimeProviderType.Fazilet) as FaziletLocationData;
        var semerkandLocationData = profileService.GetLocationConfig(profile, EDynamicPrayerTimeProviderType.Semerkand) as SemerkandLocationData;

        string coordinates = muwaqqitLocationData is null
            ? AppResources.Overview_NotSet
            : $"{muwaqqitLocationData.Latitude} / {muwaqqitLocationData.Longitude}";

        return $"""
            Muwaqqit
            - {AppResources.Overview_Coordinates}: {coordinates}
            - {AppResources.Overview_TimeZone}: {OrNotSet(muwaqqitLocationData?.TimezoneName)}

            Fazilet
            - {AppResources.Overview_Country}: {OrNotSet(faziletLocationData?.CountryName)}
            - {AppResources.Overview_City}: {OrNotSet(faziletLocationData?.CityName)}

            Semerkand
            - {AppResources.Overview_Country}: {OrNotSet(semerkandLocationData?.CountryName)}
            - {AppResources.Overview_City}: {OrNotSet(semerkandLocationData?.CityName)}
            """;
    }

    private static string OrNotSet(string? value)
        => string.IsNullOrWhiteSpace(value) ? AppResources.Overview_NotSet : value;
}
