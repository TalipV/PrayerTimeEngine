using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;

namespace PrayerTimeEngine.Presentation.Pages.Settings.SettingsContent.Custom;

public interface ISettingConfigurationViewModel
{
    GenericSettingConfiguration BuildSetting(int minuteAdjustment, bool isTimeShown);
    IView GetUI();
    void AssignSettingValues(GenericSettingConfiguration configuration);
}
