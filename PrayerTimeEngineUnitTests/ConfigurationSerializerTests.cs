using PrayerTimeEngine.Common.Enum;
using PrayerTimeEngine.Domain.Calculators.Muwaqqit.Models;
using PrayerTimeEngine.Domain.ConfigStore.Models;
using PrayerTimeEngine.Domain.Configuration.Services;

namespace PrayerTimeEngineUnitTests
{
    public class ConfigurationSerializerTests
    {
        [Test]
        public void SerializingAndDeserializing_DifferentConfigurations_DeserializedValueLikeSerializedValue()
        {
            // ARRANGE
            GenericSettingConfiguration genericSettingConfigurationFazilet =
                new GenericSettingConfiguration(
                    timeType: ETimeType.FajrEnd,
                    minuteAdjustment: -5,
                    calculationSource: ECalculationSource.Fazilet,
                    isTimeShown: false);

            GenericSettingConfiguration genericSettingConfigurationMuwaqqit =
                new GenericSettingConfiguration(
                    ETimeType.AsrEnd,
                    minuteAdjustment: -15,
                    calculationSource: ECalculationSource.Muwaqqit,
                    isTimeShown: true);

            MuwaqqitDegreeCalculationConfiguration muwaqqitDegreeCalculationConfiguration =
                new MuwaqqitDegreeCalculationConfiguration(
                    ETimeType.MaghribEnd,
                    12,
                    13.0,
                    isTimeShown: false);

            ConfigurationSerializationService configurationSerializerService = new ConfigurationSerializationService();

            // ACT
            string faziletSerialized = configurationSerializerService.Serialize(genericSettingConfigurationFazilet);
            GenericSettingConfiguration faziletDeserialized = configurationSerializerService.Deserialize(faziletSerialized, configurationSerializerService.GetDiscriminator(genericSettingConfigurationFazilet.GetType())) as GenericSettingConfiguration;

            string muwaqqitSerialized = configurationSerializerService.Serialize(genericSettingConfigurationMuwaqqit);
            GenericSettingConfiguration muwaqqitDeserialized = configurationSerializerService.Deserialize(muwaqqitSerialized, configurationSerializerService.GetDiscriminator(genericSettingConfigurationMuwaqqit.GetType())) as GenericSettingConfiguration;

            string degreeSerialized = configurationSerializerService.Serialize(muwaqqitDegreeCalculationConfiguration);
            MuwaqqitDegreeCalculationConfiguration degreeDeserialized = configurationSerializerService.Deserialize(degreeSerialized, configurationSerializerService.GetDiscriminator(muwaqqitDegreeCalculationConfiguration.GetType())) as MuwaqqitDegreeCalculationConfiguration;

            // ASSERT
            Assert.That(faziletDeserialized, Is.Not.Null);
            Assert.That(faziletDeserialized.TimeType, Is.EqualTo(genericSettingConfigurationFazilet.TimeType));
            Assert.That(faziletDeserialized.MinuteAdjustment, Is.EqualTo(genericSettingConfigurationFazilet.MinuteAdjustment));
            Assert.That(faziletDeserialized.Source, Is.EqualTo(genericSettingConfigurationFazilet.Source));
            Assert.That(faziletDeserialized.IsTimeShown, Is.EqualTo(genericSettingConfigurationFazilet.IsTimeShown));

            Assert.That(muwaqqitDeserialized, Is.Not.Null);
            Assert.That(muwaqqitDeserialized.TimeType, Is.EqualTo(genericSettingConfigurationMuwaqqit.TimeType));
            Assert.That(muwaqqitDeserialized.MinuteAdjustment, Is.EqualTo(genericSettingConfigurationMuwaqqit.MinuteAdjustment));
            Assert.That(muwaqqitDeserialized.Source, Is.EqualTo(genericSettingConfigurationMuwaqqit.Source));
            Assert.That(muwaqqitDeserialized.IsTimeShown, Is.EqualTo(genericSettingConfigurationMuwaqqit.IsTimeShown));

            Assert.That(degreeDeserialized, Is.Not.Null);
            Assert.That(degreeDeserialized.TimeType, Is.EqualTo(muwaqqitDegreeCalculationConfiguration.TimeType));
            Assert.That(degreeDeserialized.MinuteAdjustment, Is.EqualTo(muwaqqitDegreeCalculationConfiguration.MinuteAdjustment));
            Assert.That(degreeDeserialized.IsTimeShown, Is.EqualTo(muwaqqitDegreeCalculationConfiguration.IsTimeShown));
            Assert.That(degreeDeserialized.Degree, Is.EqualTo(muwaqqitDegreeCalculationConfiguration.Degree));
        }

    }
}
