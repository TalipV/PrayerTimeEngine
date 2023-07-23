using PrayerTimeEngine.Code.Common;
using PrayerTimeEngine.Code.Common.Enums;
using PrayerTimeEngine.Code.Common.Extension;
using PrayerTimeEngine.Code.Domain.Fazilet.Models;
using PrayerTimeEngine.Code.Domain.Muwaqqit.Models;
using System.Text.Json;

namespace PrayerTimeEngine.Code.Interfaces
{
    public abstract class BaseCalculationConfiguration
    {
        public static Dictionary<Type, string> CalculationConfigurationTypeToDiscriminator =
            new Dictionary<Type, string>
            {
                [typeof(GeneralMinuteAdjustmentConfguration)] = "GeneralMinuteAdjustmentConfguration",
                [typeof(FaziletCalculationConfiguration)] = "FaziletCalculationConfiguration",
                [typeof(MuwaqqitCalculationConfiguration)] = "MuwaqqitCalculationConfiguration",
                [typeof(MuwaqqitDegreeCalculationConfiguration)] = "MuwaqqitDegreeCalculationConfiguration",
            };

        public static Dictionary<string, Type> DiscriminatorToCalculationConfigurationType =
            CalculationConfigurationTypeToDiscriminator.GetInverseDictionary();

        public static BaseCalculationConfiguration GetCalculationConfigurationFromJsonString(string jsonString, string discriminator)
        {
            if (DiscriminatorToCalculationConfigurationType.TryGetValue(discriminator, out Type targetType))
            {
                return (BaseCalculationConfiguration) JsonSerializer.Deserialize(jsonString, targetType);
            }
            else
            {
                throw new NotImplementedException($"No mapping for value '{discriminator}' for {nameof(discriminator)}");
            }
        }

        public static string GetDiscriminatorForConfigurationType(Type configurationType)
        {
            if (!typeof(BaseCalculationConfiguration).IsAssignableFrom(configurationType))
                throw new ArgumentException($"'{configurationType.Name}' does not inherit from {nameof(BaseCalculationConfiguration)}");

            if (CalculationConfigurationTypeToDiscriminator.TryGetValue(configurationType, out string discriminator))
            {
                return discriminator;
            }
            else
            {
                throw new NotImplementedException($"No mapping for value '{configurationType.Name}' for {nameof(configurationType)}");
            }
        }

        public BaseCalculationConfiguration(int minuteAdjustment) 
        { 
            MinuteAdjustment = minuteAdjustment;
        }

        public abstract ECalculationSource Source { get; }
        public int MinuteAdjustment { get; set; }
    }
}
