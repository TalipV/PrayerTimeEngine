using PrayerTimeEngine.Code.Presentation.View;

namespace PrayerTimeEngine.Presentation.Service.SettingsContentPageFactory
{
    public class SettingsContentPageFactory : ISettingsContentPageFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public SettingsContentPageFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public SettingsContentPage Create()
        {
            // Use the service provider to create a new instance of SettingsContentPage
            return _serviceProvider.GetRequiredService<SettingsContentPage>();
        }
    }

}
