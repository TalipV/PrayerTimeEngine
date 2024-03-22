using PrayerTimeEngine.Presentation.View;

namespace PrayerTimeEngine.Presentation.Service.SettingsContentPageFactory
{
    public class SettingsContentPageFactory(IServiceProvider serviceProvider)
    {
        public SettingsContentPage Create()
        {
            // Use the service provider to create a new instance of SettingsContentPage
            return serviceProvider.GetRequiredService<SettingsContentPage>();
        }
    }

}
