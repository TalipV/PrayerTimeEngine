using PrayerTimeEngine.Core.Domain.ConfigurationManagement.DTOs;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Models;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;

namespace PrayerTimeEngine.Core.Domain.ConfigurationManagement;

internal static class ConfigurationMapper
{
    public static ConfigurationDTO ToConfigurationDTO(Configuration configuration)
    {
        return new ConfigurationDTO
        {
            DynamicProfileConfigs = [.. configuration.Profiles.OfType<DynamicProfile>().Select(DynamicProfileToDTO)],
            MosqueProfileConfigs = [.. configuration.Profiles.OfType<MosqueProfile>().Select(MosqueProfileToDTO)],
        };
    }

    public static Configuration ToConfiguration(ConfigurationDTO configurationDTO)
    {
        List<ProfileConfigDTO> profileDTOs = [];
        profileDTOs.AddRange(configurationDTO.DynamicProfileConfigs);
        profileDTOs.AddRange(configurationDTO.MosqueProfileConfigs);

        if (profileDTOs.Any(x => x == null))
        {
            throw new InvalidOperationException("Configuration contains a null profile entry.");
        }

        List<Profile> profiles = [];

        foreach (ProfileConfigDTO profileDTO in profileDTOs)
        {
            if (profileDTO is DynamicProfileConfigDTO dynamicProfileConfigDTO)
            {
                profiles.Add(DynamicProfileDTOToEntity(dynamicProfileConfigDTO));
            }
            else if (profileDTO is MosqueProfileConfigDTO mosqueProfileConfigDTO)
            {
                profiles.Add(MosqueProfileDTOToEntity(mosqueProfileConfigDTO));
            }
            else
            {
                throw new NotImplementedException($"Type '{profileDTO?.GetType().FullName}' of profile not implemented");
            }
        }

        return new Configuration()
        {
            Profiles = profiles
        };
    }

    private static MosqueProfileConfigDTO MosqueProfileToDTO(MosqueProfile profile)
    {
        return new MosqueProfileConfigDTO
        {
            Name = profile.Name,
            SequenceNo = profile.SequenceNo,
            ExternalID = profile.ExternalID,
            MosqueProviderType = profile.MosqueProviderType
        };
    }

    private static DynamicProfileConfigDTO DynamicProfileToDTO(DynamicProfile profile)
    {
        return new DynamicProfileConfigDTO
        {
            Name = profile.Name,
            SequenceNo = profile.SequenceNo,
            PlaceInfo = PlaceInfoToDTO(profile.PlaceInfo),
            TimeConfigs = profile.TimeConfigs?.Select(TimeConfigToDTO).ToList(),
            LocationConfigs = profile.LocationConfigs?.Select(LocationConfigToDTO).ToList()
        };
    }

    private static PlaceInfoDTO PlaceInfoToDTO(ProfilePlaceInfo placeInfo)
    {
        if (placeInfo == null)
        {
            return null;
        }

        return new PlaceInfoDTO
        {
            ExternalID = placeInfo.ExternalID,
            Longitude = placeInfo.Longitude,
            Latitude = placeInfo.Latitude,
            InfoLanguageCode = placeInfo.InfoLanguageCode,
            Country = placeInfo.Country,
            State = placeInfo.State,
            City = placeInfo.City,
            CityDistrict = placeInfo.CityDistrict,
            PostCode = placeInfo.PostCode,
            Street = placeInfo.Street,

            TimezoneDisplayName = placeInfo.TimezoneInfo.DisplayName,
            TimezoneName = placeInfo.TimezoneInfo.Name,
            TimezoneUtcOffsetSeconds = placeInfo.TimezoneInfo.UtcOffsetSeconds,
        };
    }

    private static TimeConfigDTO TimeConfigToDTO(ProfileTimeConfig timeConfig)
    {
        if (timeConfig == null)
        {
            return null;
        }

        return new TimeConfigDTO
        {
            TimeType = timeConfig.TimeType,
            CalculationConfiguration = timeConfig.CalculationConfiguration
        };
    }

    private static LocationConfigDTO LocationConfigToDTO(ProfileLocationConfig locationConfig)
    {
        if (locationConfig == null)
        {
            return null;
        }

        return new LocationConfigDTO
        {
            ProviderType = locationConfig.DynamicPrayerTimeProvider,
            LocationData = locationConfig.LocationData
        };
    }

    private static DynamicProfile DynamicProfileDTOToEntity(DynamicProfileConfigDTO dynamicProfileConfigDTO)
    {
        var dynamicProfile = new DynamicProfile()
        {
            Name = dynamicProfileConfigDTO.Name,
            PlaceInfo = new ProfilePlaceInfo
            {
                ExternalID = dynamicProfileConfigDTO.PlaceInfo.ExternalID,
                Longitude = dynamicProfileConfigDTO.PlaceInfo.Longitude,
                Latitude = dynamicProfileConfigDTO.PlaceInfo.Latitude,
                InfoLanguageCode = dynamicProfileConfigDTO.PlaceInfo.InfoLanguageCode,
                Country = dynamicProfileConfigDTO.PlaceInfo.Country,
                State = dynamicProfileConfigDTO.PlaceInfo.State,
                City = dynamicProfileConfigDTO.PlaceInfo.City,
                CityDistrict = dynamicProfileConfigDTO.PlaceInfo.CityDistrict,
                PostCode = dynamicProfileConfigDTO.PlaceInfo.PostCode,
                Street = dynamicProfileConfigDTO.PlaceInfo.Street,
                TimezoneInfo = new TimezoneInfo
                {
                    DisplayName = dynamicProfileConfigDTO.PlaceInfo.TimezoneDisplayName,
                    Name = dynamicProfileConfigDTO.PlaceInfo.TimezoneName,
                    UtcOffsetSeconds = dynamicProfileConfigDTO.PlaceInfo.TimezoneUtcOffsetSeconds,
                },
            },

            LocationConfigs = [.. dynamicProfileConfigDTO.LocationConfigs.Select(locationConfigDTO => new ProfileLocationConfig
            {
                DynamicPrayerTimeProvider = locationConfigDTO.ProviderType,
                LocationData = locationConfigDTO.LocationData,
            })],
            TimeConfigs = [.. dynamicProfileConfigDTO.TimeConfigs.Select(timeConfigDTO => new ProfileTimeConfig
            {
                TimeType = timeConfigDTO.TimeType,
                CalculationConfiguration = timeConfigDTO.CalculationConfiguration,
            })],

            SequenceNo = dynamicProfileConfigDTO.SequenceNo,
        };

        return dynamicProfile;
    }

    private static MosqueProfile MosqueProfileDTOToEntity(MosqueProfileConfigDTO mosqueProfileConfigDTO)
    {
        var mosqueProfile = new MosqueProfile
        {
            ExternalID = mosqueProfileConfigDTO.ExternalID,
            Name = mosqueProfileConfigDTO.Name,
            MosqueProviderType = mosqueProfileConfigDTO.MosqueProviderType,
            SequenceNo = mosqueProfileConfigDTO.SequenceNo,
        };

        return mosqueProfile;
    }
}
