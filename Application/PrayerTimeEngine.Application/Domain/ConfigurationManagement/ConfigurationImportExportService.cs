using System.Text.Json;
using PrayerTimeEngine.Core.Domain.ConfigurationManagement.DTOs;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Interfaces;

namespace PrayerTimeEngine.Core.Domain.ConfigurationManagement;

public class ConfigurationImportExportService : IConfigurationImportExportService
{
    private readonly IProfileRepository _profileRepository;
    private readonly JsonSerializerOptions _jsonOptions;

    public ConfigurationImportExportService(IProfileRepository profileRepository)
    {
        this._profileRepository = profileRepository;

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };
    }

    public string SerializeConfiguration(Configuration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

        var configDTO = ConfigurationMapper.ToConfigurationDTO(configuration);
        return JsonSerializer.Serialize(configDTO, _jsonOptions);
    }

    public async Task<Configuration> Import(string content, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content, nameof(content));

        ConfigurationDTO configDTO = JsonSerializer.Deserialize<ConfigurationDTO>(content, _jsonOptions);
        var configuration = ConfigurationMapper.ToConfiguration(configDTO);

        await _profileRepository.SaveProfiles(configuration.Profiles, cancellationToken).ConfigureAwait(false);

        return configuration;
    }
}
