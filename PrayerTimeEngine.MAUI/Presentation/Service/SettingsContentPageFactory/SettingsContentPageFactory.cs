using PrayerTimeEngine.Code.Presentation.View;

namespace PrayerTimeEngine.Presentation.Service.SettingsContentPageFactory
{
    public class SettingsContentPageFactory
    {
        private IServiceProvider serviceProvider;

        public SettingsContentPageFactory(
            IServiceProvider serviceProvider
        )
        {
            this.serviceProvider = serviceProvider;
        }

        public SettingsContentPage Create()
        {
            // Use the service provider to create a new instance of SettingsContentPage
            return serviceProvider.GetRequiredService<SettingsContentPage>();
        }
    }

}
