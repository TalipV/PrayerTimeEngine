using PrayerTimeEngine.Domain.ConfigStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
