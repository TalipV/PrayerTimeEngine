using MvvmHelpers;
using PrayerTimeEngine.Code.Presentation.View;
using PrayerTimeEngine.Common.Enum;
using PrayerTimeEngine.Domain;
using PrayerTimeEngine.Presentation.Service.Navigation;
using PrayerTimeEngine.Presentation.Service.SettingsContentPageFactory;
using PropertyChanged;
using System.Linq;

namespace PrayerTimeEngine.Presentation.ViewModel
{
    /// <summary>
    /// Dummy class for <see cref="INavigationService"/>.
    /// </summary>
    public abstract class CustomBaseViewModel : BaseViewModel
    {
        public abstract void Initialize(object parameter);
    }

    [AddINotifyPropertyChangedInterface]
    public class SettingsHandlerPageViewModel : CustomBaseViewModel
    {
        public SettingsHandlerPageViewModel(
            ISettingsContentPageFactory settingsContentPageFactory,
            TimeTypeAttributeService timeTypeAttributeService)
        {
            _settingsContentPageFactory = settingsContentPageFactory;
            _timeTypeAttributeService = timeTypeAttributeService;
        }

        public event Action Initialized = delegate { };

        #region fields

        private readonly ISettingsContentPageFactory _settingsContentPageFactory;
        private readonly TimeTypeAttributeService _timeTypeAttributeService;

        #endregion fields

        #region properties

        public List<SettingsContentPage> SettingsContentPages = new();

        #endregion properties

        #region public methods

        public override void Initialize(object parameter)
        {
            if (parameter is not EPrayerType prayerTime)
            {
                throw new ArgumentException($"{nameof(parameter)} is not an {nameof(EPrayerType)}");
            }

            foreach (ETimeType timeType in _timeTypeAttributeService.PrayerTypeToTimeTypes[prayerTime].Except(_timeTypeAttributeService.SimpleTypes))
            {
                SettingsContentPage settingsContentPage = _settingsContentPageFactory.Create();
                SettingsContentPageViewModel tabViewModel = settingsContentPage.BindingContext as SettingsContentPageViewModel;
                tabViewModel.Initialize(timeType);
                SettingsContentPages.Add(settingsContentPage);
            }

            Initialized.Invoke();
        }

        #endregion public methods

    }
}
