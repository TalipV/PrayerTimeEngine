namespace PrayerTimeEngine.Core.Domain.ConfigurationManagement;
public interface IConfigurationImportExportService
{
    string SerializeConfiguration(Configuration configuration);
    Task<Configuration> Import(string content, CancellationToken cancellationToken);
}
