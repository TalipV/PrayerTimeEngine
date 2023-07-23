using Microsoft.Maui.Controls;
using PrayerTimeEngine.Code.Common.Enums;
using PrayerTimeEngine.Code.Domain.Muwaqqit.Models;
using PrayerTimeEngine.Code.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrayerTimeEngine.Code.Presentation.Service.SettingConfiguration
{
    public class SettingConfigurationUIFactory
    {
        public static ISettingConfigurationUI CreateUI(BaseCalculationConfiguration configuration, (EPrayerTime, EPrayerTimeEvent) PrayerTimeWithEvent)
        {
            switch (configuration)
            {
                case MuwaqqitDegreeCalculationConfiguration _:
                    return new MuwaqqitDegreeSettingConfigurationUI(PrayerTimeWithEvent);
                case MuwaqqitCalculationConfiguration _:
                    return new MuwaqqitSettingConfigurationUI();
                default:
                    return null;
            }
        }
    }
}
