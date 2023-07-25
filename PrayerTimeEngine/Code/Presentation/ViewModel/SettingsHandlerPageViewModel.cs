using MvvmHelpers;
using PrayerTimeEngine.Code.Common.Enum;
using PrayerTimeEngine.Code.Presentation.Service.Navigation;
using PrayerTimeEngine.Code.Presentation.Service.SettingsContentPageFactory;
using PrayerTimeEngine.Code.Presentation.View;
using PropertyChanged;

namespace PrayerTimeEngine.Code.Presentation.ViewModel
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
        public SettingsHandlerPageViewModel(ISettingsContentPageFactory settingsContentPageFactory)
        {
            _settingsContentPageFactory = settingsContentPageFactory;
        }

        public event Action Initialized = delegate { };

        #region fields

        private readonly ISettingsContentPageFactory _settingsContentPageFactory;

        #endregion fields

        #region properties

        public List<SettingsContentPage> SettingsContentPages = new();

        #endregion properties

        #region public methods

        public override void Initialize(object parameter)
        {
            if (parameter is not EPrayerTime prayerTime)
            {
                throw new ArgumentException($"{nameof(parameter)} is not an {nameof(EPrayerTime)}");
            }

            foreach (EPrayerTimeEvent prayerTimeEvent in getPrayerTimeEvents(prayerTime))
            {
                SettingsContentPage settingsContentPage = _settingsContentPageFactory.Create();
                SettingsContentPageViewModel tabViewModel = settingsContentPage.BindingContext as SettingsContentPageViewModel;
                tabViewModel.Initialize(prayerTime, prayerTimeEvent);
                SettingsContentPages.Add(settingsContentPage);
            }

            Initialized.Invoke();
        }

        #endregion public methods

        #region private methods

        private List<EPrayerTimeEvent> getPrayerTimeEvents(EPrayerTime prayerTime)
        {
            List<EPrayerTimeEvent> prayerTimeEvents = new List<EPrayerTimeEvent> { EPrayerTimeEvent.Start, EPrayerTimeEvent.End};

            switch (prayerTime)
            {
                case EPrayerTime.Fajr:
                    prayerTimeEvents.Add(EPrayerTimeEvent.Fajr_Fadilah);
                    prayerTimeEvents.Add(EPrayerTimeEvent.Fajr_Karaha);
                    break;

                case EPrayerTime.Asr:
                    prayerTimeEvents.Add(EPrayerTimeEvent.AsrMithlayn);
                    prayerTimeEvents.Add(EPrayerTimeEvent.Asr_Karaha);
                    break;

                case EPrayerTime.Maghrib:
                    prayerTimeEvents.Add(EPrayerTimeEvent.IshtibaqAnNujum);
                    break;

                default:
                    break;
            }

            return prayerTimeEvents;
        }
        
        #endregion private methods
    }
}
