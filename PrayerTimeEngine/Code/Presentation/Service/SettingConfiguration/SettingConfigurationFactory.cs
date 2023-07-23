using Microsoft.Maui.Controls;
using PrayerTimeEngine.Code.Common.Enums;
using PrayerTimeEngine.Code.Domain;
using PrayerTimeEngine.Code.Domain.Fazilet.Models;
using PrayerTimeEngine.Code.Domain.Muwaqqit.Models;
using PrayerTimeEngine.Code.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrayerTimeEngine.Code.Presentation.Service.SettingConfiguration
{
    public class SettingConfigurationFactory
    {
        public static BaseCalculationConfiguration CreateSettings(
            ECalculationSource calculationSource,
            (EPrayerTime, EPrayerTimeEvent) prayerTimeEvent)
        {
            switch (calculationSource)
            {
                case ECalculationSource.Muwaqqit:
                    if (MuwaqqitDegreeCalculationConfiguration.DegreePrayerTimeEvents.TryGetValue(prayerTimeEvent, out double degree))
                    {
                        return new MuwaqqitDegreeCalculationConfiguration(
                            0, 
                            PrayerTimesConfigurationStorage.INNSBRUCK_LONGITUDE,
                            PrayerTimesConfigurationStorage.INNSBRUCK_LATITUDE,
                            PrayerTimesConfigurationStorage.TIMEZONE,
                            degree);
                    }
                    else
                    {
                        return new MuwaqqitCalculationConfiguration(
                            0,
                            PrayerTimesConfigurationStorage.INNSBRUCK_LONGITUDE,
                            PrayerTimesConfigurationStorage.INNSBRUCK_LATITUDE,
                            PrayerTimesConfigurationStorage.TIMEZONE);
                    }
                case ECalculationSource.Fazilet:
                    return new FaziletCalculationConfiguration(
                        0,
                        PrayerTimesConfigurationStorage.COUNTRY_NAME,
                        PrayerTimesConfigurationStorage.CITY_NAME);
                default:
                    return null;
            }
        }
    }
}
