using MvvmHelpers;
using PrayerTimeEngine.Code.Presentation.View;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain;
using PrayerTimeEngine.Core.Domain.Configuration.Models;
using PrayerTimeEngine.Presentation.Service.Navigation;
using PrayerTimeEngine.Presentation.Service.SettingsContentPageFactory;
using PropertyChanged;

namespace PrayerTimeEngine.Presentation.ViewModel
{
    /// <summary>
    /// Dummy class for <see cref="INavigationService"/>.
    /// </summary>
    public abstract class CustomBaseViewModel : BaseViewModel
    {
        public abstract void Initialize(params object[] parameter);
    }

    [AddINotifyPropertyChangedInterface]
    public class SettingsHandlerPageViewModel(
            SettingsContentPageFactory settingsContentPageFactory,
            TimeTypeAttributeService timeTypeAttributeService
        ) : CustomBaseViewModel
    {
        public event Action Initialized = delegate { };

        #region fields

        #endregion fields

        #region properties

        public List<SettingsContentPage> SettingsContentPages = new();

        #endregion properties

        #region public methods

        public override void Initialize(params object[] parameter)
        {
            if (parameter[0] is not Profile profile)
            {
                throw new ArgumentException($"{nameof(parameter)} is not an {nameof(EPrayerType)}");
            }

            if (parameter[1] is not EPrayerType prayerTime)
            {
                throw new ArgumentException($"{nameof(parameter)} is not an {nameof(EPrayerType)}");
            }

            foreach (ETimeType timeType in timeTypeAttributeService.PrayerTypeToTimeTypes[prayerTime].Intersect(timeTypeAttributeService.ConfigurableTypes))
            {
                SettingsContentPage settingsContentPage = settingsContentPageFactory.Create();
                SettingsContentPageViewModel tabViewModel = settingsContentPage.BindingContext as SettingsContentPageViewModel;
                tabViewModel.Initialize(profile, timeType);
                SettingsContentPages.Add(settingsContentPage);
            }

            Initialized.Invoke();
        }

        #endregion public methods
    }
}
