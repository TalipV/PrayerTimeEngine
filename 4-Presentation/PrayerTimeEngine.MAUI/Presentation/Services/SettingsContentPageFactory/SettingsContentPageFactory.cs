using PrayerTimeEngine.Presentation.Pages.Settings.SettingsContent;

namespace PrayerTimeEngine.Presentation.Services.SettingsContentPageFactory;

public class SettingsContentPageFactory(IServiceProvider serviceProvider)
{
    public SettingsContentPage Create()
    {
        // Use the service provider to create a new instance of SettingsContentPage
        return serviceProvider.GetRequiredService<SettingsContentPage>();
    }
}
