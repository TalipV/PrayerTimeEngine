using Newtonsoft.Json;
using PrayerTimeEngine.Common.Extension;
using PrayerTimeEngine.Domain.Calculators.Muwaqqit.Models;
using PrayerTimeEngine.Domain.ConfigStore.Models;
using PrayerTimeEngine.Domain.Configuration.Interfaces;

namespace PrayerTimeEngine.Domain.Configuration.Services
{
    public class ConfigurationSerializationService : IConfigurationSerializationService
    {
        private static readonly Dictionary<Type, string> CalculationConfigurationTypeToDiscriminator =
            new Dictionary<Type, string>
            {
                [typeof(GenericSettingConfiguration)] = "GenericSettingConfiguration",
                [typeof(MuwaqqitDegreeCalculationConfiguration)] = "MuwaqqitDegreeCalculationConfiguration",
            };

        private static readonly Dictionary<string, Type> DiscriminatorToCalculationConfigurationType =
            CalculationConfigurationTypeToDiscriminator.GetInverseDictionary();

        public GenericSettingConfiguration Deserialize(string jsonString, string discriminator)
        {
            if (DiscriminatorToCalculationConfigurationType.TryGetValue(discriminator, out Type targetType))
            {
                return (GenericSettingConfiguration)JsonConvert.DeserializeObject(jsonString, targetType);
            }
            else
            {
                throw new NotImplementedException($"No mapping for value '{discriminator}' for {nameof(discriminator)}");
            }
        }

        public string Serialize(GenericSettingConfiguration configuration)
        {
            return JsonConvert.SerializeObject(configuration);
        }

        public string GetDiscriminator(Type configurationType)
        {
            if (!typeof(GenericSettingConfiguration).IsAssignableFrom(configurationType))
                throw new ArgumentException($"'{configurationType.Name}' does not inherit from {nameof(GenericSettingConfiguration)}");

            if (CalculationConfigurationTypeToDiscriminator.TryGetValue(configurationType, out string discriminator))
            {
                return discriminator;
            }
            else
            {
                throw new NotImplementedException($"No mapping for value '{configurationType.Name}' for {nameof(configurationType)}");
            }
        }

        public Type GetTypeFromDiscriminator(string discriminator)
        {
            if (DiscriminatorToCalculationConfigurationType.TryGetValue(discriminator, out Type targetType))
            {
                return targetType;
            }
            else
            {
                throw new NotImplementedException($"No mapping for value '{discriminator}' for {nameof(discriminator)}");
            }
        }
    }

}
