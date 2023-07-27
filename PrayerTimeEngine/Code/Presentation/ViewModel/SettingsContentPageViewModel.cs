using PrayerTimeEngine.Code.Common;
using PrayerTimeEngine.Code.Common.Enum;
using PrayerTimeEngine.Code.Common.Extension;
using PrayerTimeEngine.Code.Domain.Calculator.Muwaqqit.Models;
using PrayerTimeEngine.Code.Domain.ConfigStore.Interfaces;
using PrayerTimeEngine.Code.Domain.ConfigStore.Models;
using PrayerTimeEngine.Code.Presentation.ViewModel.Custom;
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

        public event Action OnInitializeCustomUI_EventTrigger = delegate { };

        public event Action OnViewModelInitialize_EventTrigger = delegate { };

        #region fields

        private readonly IConfigStoreService _configStoreService;
        private readonly PrayerTimesConfigurationStorage _prayerTimesConfigurationStorage;

        #endregion fields

        #region properties

        public string TabTitle { get; private set; }
        public (EPrayerTime PrayerTime, EPrayerTimeEvent PrayerTimeEvent) PrayerTimeWithEvent { get; private set; }

        public List<ECalculationSource> CalculationSources { get; private set; }
        public ECalculationSource SelectedCalculationSource { get; private set; }

        public List<int> MinuteAdjustments { get; private set; }
        public int SelectedMinuteAdjustment { get; private set; }

        public bool IsTimeShown { get; private set; }
        public bool IsTimeShownCheckBoxVisible { get; private set; }

        public bool ShowMinuteAdjustmentPicker { get; private set; }
        public bool ShowCalculationSourcePicker { get; private set; }

        public ISettingConfigurationViewModel CustomSettingConfigurationViewModel { get; private set; }

        #endregion properties

        #region public methods

        public void Initialize(EPrayerTime PrayerTime, EPrayerTimeEvent PrayerTimeEvent)
        {
            TabTitle = $"{PrayerTime}-{PrayerTimeEvent}";
            PrayerTimeWithEvent = (PrayerTime, PrayerTimeEvent);
            ShowCalculationSourcePicker = (PrayerTimeWithEvent != (EPrayerTime.Duha, EPrayerTimeEvent.End));
            IsTimeShownCheckBoxVisible = PrayerTimeEvent != EPrayerTimeEvent.Start && PrayerTimeEvent != EPrayerTimeEvent.End;

            loadCalculationSource();
            loadMinuteAdjustmentSource();

            BaseCalculationConfiguration calculationConfiguration = _prayerTimesConfigurationStorage.GetConfiguration(PrayerTimeWithEvent);
            IsTimeShown = !IsTimeShownCheckBoxVisible || calculationConfiguration.IsTimeShown;

            SelectedCalculationSource = calculationConfiguration.Source;
            if (SelectedCalculationSource == ECalculationSource.None)
                OnSelectedCalculationSourceChanged();

            SelectedMinuteAdjustment = calculationConfiguration.MinuteAdjustment;
            CustomSettingConfigurationViewModel?.AssignSettingValues(calculationConfiguration);

            OnViewModelInitialize_EventTrigger();
        }

        public void OnSelectedCalculationSourceChanged()
        {
            if (SelectedCalculationSource == ECalculationSource.Muwaqqit
                && MuwaqqitDegreeCalculationConfiguration.DegreePrayerTimeEvents.ContainsKey(PrayerTimeWithEvent))
            {
                CustomSettingConfigurationViewModel =
                        new MuwaqqitDegreeSettingConfigurationViewModel(
                            PrayerTimeWithEvent,
                            MuwaqqitDegreeCalculationConfiguration.DegreePrayerTimeEvents[PrayerTimeWithEvent]);
            }
            else
            {
                CustomSettingConfigurationViewModel = null;
            }

            OnInitializeCustomUI_EventTrigger.Invoke();
        }

        public void OnDisappearing()
        {
            BaseCalculationConfiguration settings = getCurrentCalculationConfiguration();
            saveSettingsToProfile(settings);
        }

        #endregion public methods

        #region private methods

        private BaseCalculationConfiguration getCurrentCalculationConfiguration()
        {
            return
                this.CustomSettingConfigurationViewModel?
                    .BuildSetting(SelectedMinuteAdjustment, IsTimeShown)
                        ?? this.getGeneralCalculationConfiguration();
        }

        private BaseCalculationConfiguration getGeneralCalculationConfiguration()
        {
            return new GenericSettingConfiguration(SelectedMinuteAdjustment, SelectedCalculationSource, IsTimeShown);
        }

        private void loadCalculationSource()
        {
            List<ECalculationSource> calculationSources = 
                Enum.GetValues(typeof(ECalculationSource))
                    .Cast<ECalculationSource>()
                    .Where(x => PrayerTimeWithEvent.PrayerTimeEvent.IsSupportedBy(x))
                    .ToList();

            if (PrayerTimeWithEvent.PrayerTime == EPrayerTime.Duha)
            {
                calculationSources.Remove(ECalculationSource.Fazilet);
                calculationSources.Remove(ECalculationSource.Semerkand);
            }

            this.CalculationSources = calculationSources;
        }

        private void loadMinuteAdjustmentSource()
        {
            if (PrayerTimeWithEvent == (EPrayerTime.Duha, EPrayerTimeEvent.End))
            {
                this.MinuteAdjustments = Enumerable.Range(-40, 35).ToList();
            }
            else
            {
                this.MinuteAdjustments = Enumerable.Range(-15, 30).ToList();
            }
        }

        private void saveSettingsToProfile(BaseCalculationConfiguration settings)
        {
            List<Profile> profiles = _prayerTimesConfigurationStorage.GetProfiles().GetAwaiter().GetResult();
            profiles.First().Configurations[PrayerTimeWithEvent] = settings;
            _configStoreService.SaveProfiles(profiles).GetAwaiter().GetResult();
        }

        #endregion private methods
    }
}
