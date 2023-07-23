using PrayerTimeEngine.Code.Common;
using PrayerTimeEngine.Code.Common.Enums;
using PrayerTimeEngine.Code.Domain;
using PrayerTimeEngine.Code.Domain.ConfigStore.Interfaces;
using PrayerTimeEngine.Code.Domain.Fazilet.Models;
using PrayerTimeEngine.Code.Domain.Muwaqqit.Models;
using PrayerTimeEngine.Code.Interfaces;
using PrayerTimeEngine.Code.Presentation.Service.SettingConfiguration;
using PropertyChanged;

namespace PrayerTimeEngine.Code.Presentation.ViewModel
{
    [AddINotifyPropertyChangedInterface]
    public class SettingsContentPageViewModel
    {
        #region static fields

        public static readonly List<double> FAJR_ISHA_SELECTABLE_DEGREES = new List<double>
        {
            -12.0, -12.5, -13.0, -13.5, -14.0,
            -14.5, -15.0, -15.5, -16.0, -16.5,
            -17.0, -17.5, -18.0, -18.5, -19.0,
            -19.5, -20.0
        };

        public static readonly List<double> MODERATE_SELECTABLE_DEGREES = new List<double>
        {
            -2.0, -2.5, -3.0, -3.5, -4.0, -4.5,
            -5.0, -5.5, -6.0, -6.5, -7.0, -7.5,
            -8.0, -8.5, -9.0
        };

        #endregion static fields

        public SettingsContentPageViewModel(
            PrayerTimesConfigurationStorage prayerTimesConfigurationStorage,
            IConfigStoreService configStoreService)
        {
            _configStoreService = configStoreService;
            _prayerTimesConfigurationStorage = prayerTimesConfigurationStorage;
        }

        public event Action OnCustomSettingUIReady = delegate { };

        #region fields

        private bool _isInitializing = false;

        private readonly IConfigStoreService _configStoreService;
        private readonly PrayerTimesConfigurationStorage _prayerTimesConfigurationStorage;

        #endregion fields

        #region properties

        public List<ECalculationSource> CalculationSources { get; set; } = new List<ECalculationSource>();
        public List<int> MinuteAdjustments { get; } = Enumerable.Range(-15, 30).ToList();
        public string TabTitle { get; set; }
        public ECalculationSource SelectedCalculationSource { get; set; } = ECalculationSource.None;
        public int SelectedMinuteAdjustment { get; set; }
        public bool ShowMinuteAdjustmentPicker { get; set; }
        public ISettingConfigurationUI CustomSettingConfigurationUI { get; private set; }

        public (EPrayerTime prayerTime, EPrayerTimeEvent prayerTimeEvent) PrayerTimeWithEvent { get; private set; }

        #endregion properties

        #region public methods

        public void Initialize(EPrayerTime prayerTime, EPrayerTimeEvent prayerTimeEvent)
        {
            try
            {
                _isInitializing = true;
                TabTitle = $"{prayerTime}-{prayerTimeEvent}";
                PrayerTimeWithEvent = (prayerTime, prayerTimeEvent);
                ShowMinuteAdjustmentPicker = prayerTime != EPrayerTime.Duha || prayerTimeEvent != EPrayerTimeEvent.End;
                fixDataBindingProblem();

                loadCalculationSource();

                if (!_prayerTimesConfigurationStorage.GetProfiles().GetAwaiter().GetResult().First().Configurations
                        .TryGetValue(PrayerTimeWithEvent, out BaseCalculationConfiguration calculationConfiguration)
                        || calculationConfiguration == null)
                {
                    SelectedCalculationSource = ECalculationSource.None;
                    SelectedMinuteAdjustment = 0;
                    return;
                }

                SelectedCalculationSource = calculationConfiguration.Source;
                SelectedMinuteAdjustment = calculationConfiguration.MinuteAdjustment;
                loadCustomSettingsUI(calculationConfiguration);
            }
            finally
            {
                _isInitializing = false;
            }
        }

        public void OnCalculationSourceChanged(ECalculationSource calculationSource)
        {
            if (_isInitializing)
            {
                return;
            }

            // get new settings with default values
            BaseCalculationConfiguration settings =
                SettingConfigurationFactory.CreateSettings(
                    calculationSource,
                    PrayerTimeWithEvent);

            loadCustomSettingsUI(settings);
        }

        public void OnDisappearing()
        {
            BaseCalculationConfiguration settings = 
                CustomSettingConfigurationUI?.GetSettings(SelectedMinuteAdjustment);
            
            _prayerTimesConfigurationStorage.GetProfiles().GetAwaiter().GetResult().First().Configurations[PrayerTimeWithEvent] = settings;
            _configStoreService.SaveProfiles(_prayerTimesConfigurationStorage.GetProfiles().GetAwaiter().GetResult()).GetAwaiter().GetResult();
        }

        #endregion public methods

        #region private methods

        private void fixDataBindingProblem()
        {
            // without initial property change trigger the intended initial selection won't properly bind
            SelectedCalculationSource = ECalculationSource.Muwaqqit;
            SelectedCalculationSource = ECalculationSource.None;
            SelectedMinuteAdjustment = -5;
            SelectedMinuteAdjustment = 5;
        }

        private void loadCalculationSource()
        {
            List<ECalculationSource> calculationSources = Enum.GetValues(typeof(ECalculationSource)).Cast<ECalculationSource>().ToList();

            if (PrayerTimeWithEvent.prayerTime == EPrayerTime.Duha
                || (PrayerTimeWithEvent.prayerTimeEvent != EPrayerTimeEvent.Start
                && PrayerTimeWithEvent.prayerTimeEvent != EPrayerTimeEvent.End))
            {
                calculationSources.Remove(ECalculationSource.Fazilet);
            }

            this.CalculationSources = calculationSources;
        }

        private void loadCustomSettingsUI(BaseCalculationConfiguration settings)
        {
            // get UI for new settings
            CustomSettingConfigurationUI = SettingConfigurationUIFactory.CreateUI(settings, PrayerTimeWithEvent);

            // apply values to UI
            CustomSettingConfigurationUI?.AssignConfigurationValues(settings);
            OnCustomSettingUIReady.Invoke();
        }

        #endregion private methods
    }
}
