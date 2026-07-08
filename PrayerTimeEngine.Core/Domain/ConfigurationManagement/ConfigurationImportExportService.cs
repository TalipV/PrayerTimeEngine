using PrayerTimeEngine.Core.Domain.ConfigurationManagement.DTOs;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Interfaces;
using System.Text.Json;

namespace PrayerTimeEngine.Core.Domain.ConfigurationManagement;
public class ConfigurationImportExportService : IConfigurationImportExportService
{
    private readonly IProfileRepository _profileRepository;
    private readonly JsonSerializerOptions _jsonOptions;

    public ConfigurationImportExportService(IProfileRepository _profileRepository)
    {
        this._profileRepository = _profileRepository;

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };
    }

    public string SerializeConfiguration(Configuration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

        ConfigurationDTO configDTO = ConfigurationMapper.ToConfigurationDTO(configuration);
        return JsonSerializer.Serialize(configDTO, _jsonOptions);
    }

    public async Task<Configuration> Import(string content, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content, nameof(content));

        ConfigurationDTO configDTO = JsonSerializer.Deserialize<ConfigurationDTO>(content, _jsonOptions);
        Configuration configuration = ConfigurationMapper.ToConfiguration(configDTO); 
        
        await _profileRepository.SaveProfiles(configuration.Profiles, cancellationToken).ConfigureAwait(false);

        return configuration;
    }
}
