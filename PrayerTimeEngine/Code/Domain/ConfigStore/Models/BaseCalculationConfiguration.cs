using PrayerTimeEngine.Code.Common.Enum;
using PrayerTimeEngine.Code.Common.Extension;
using PrayerTimeEngine.Code.Domain.Calculator.Muwaqqit.Models;
using System.Text.Json;

namespace PrayerTimeEngine.Code.Domain.ConfigStore.Models
{
    public abstract class BaseCalculationConfiguration
    {
        #region static

        public static Dictionary<Type, string> CalculationConfigurationTypeToDiscriminator =
            new Dictionary<Type, string>
            {
                [typeof(GenericSettingConfiguration)] = "GenericSettingConfiguration",
                [typeof(MuwaqqitCalculationConfiguration)] = "MuwaqqitCalculationConfiguration",
                [typeof(MuwaqqitDegreeCalculationConfiguration)] = "MuwaqqitDegreeCalculationConfiguration",
            };

        public static Dictionary<string, Type> DiscriminatorToCalculationConfigurationType =
            CalculationConfigurationTypeToDiscriminator.GetInverseDictionary();

        public static BaseCalculationConfiguration GetCalculationConfigurationFromJsonString(string jsonString, string discriminator)
        {
            if (DiscriminatorToCalculationConfigurationType.TryGetValue(discriminator, out Type targetType))
            {
                return (BaseCalculationConfiguration)JsonSerializer.Deserialize(jsonString, targetType);
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

        #endregion static

        public BaseCalculationConfiguration(int minuteAdjustment, bool isTimeShown)
        {
            MinuteAdjustment = minuteAdjustment;
            IsTimeShown = isTimeShown;
        }

        public abstract ECalculationSource Source { get; }
        public int MinuteAdjustment { get; set; } = 0;
        public bool IsTimeShown { get; set; } = false;
    }
}
