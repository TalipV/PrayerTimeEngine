using PrayerTimeEngine.Domain.ConfigStore.Models;

namespace PrayerTimeEngine.Domain.Configuration.Interfaces
{
    public interface IConfigurationSerializationService
    {
        GenericSettingConfiguration Deserialize(string jsonString, string discriminator);
        string Serialize(GenericSettingConfiguration configuration);
        string GetDiscriminator(Type configurationType);
        Type GetTypeFromDiscriminator(string discriminator);
    }
}
