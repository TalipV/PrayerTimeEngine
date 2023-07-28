using PrayerTimeEngine.Code.Common.Enum;
using PrayerTimeEngine.Code.Common.Extension;
using PrayerTimeEngine.Code.Domain;
using PrayerTimeEngine.Code.Domain.Calculator.Muwaqqit.Models;
using PrayerTimeEngine.Code.Domain.ConfigStore.Interfaces;
using PrayerTimeEngine.Code.Domain.ConfigStore.Models;
using PrayerTimeEngine.Code.Presentation.ViewModel.Custom;
using PropertyChanged;
using System.Collections.Generic;

namespace PrayerTimeEngine.Code.Presentation.ViewModel
{
    [AddINotifyPropertyChangedInterface]
    public class SettingsContentPageViewModel
    {
        #region static fields

        public static readonly IReadOnlyCollection<double> FAJR_ISHA_SELECTABLE_DEGREES = new List<double>
        {
            -12.0, -12.5, -13.0, -13.5, -14.0,
            -14.5, -15.0, -15.5, -16.0, -16.5,
            -17.0, -17.5, -18.0, -18.5, -19.0,
            -19.5, -20.0
        }.AsReadOnly();

        public static readonly IReadOnlyCollection<double> MODERATE_SELECTABLE_DEGREES = new List<double>
        {
            -2.0, -2.5, -3.0, -3.5, -4.0, -4.5,
            -5.0, -5.5, -6.0, -6.5, -7.0, -7.5,
            -8.0, -8.5, -9.0
        }.AsReadOnly();

        public static readonly IReadOnlyCollection<double> MODERATE_SELECTABLE_DEGREES_POSITIVE =
            MODERATE_SELECTABLE_DEGREES.Select(Math.Abs).ToList().AsReadOnly();

        #endregion static fields

        public SettingsContentPageViewModel(
            PrayerTimesConfigurationStorage prayerTimesConfigurationStorage,
            IConfigStoreService configStoreService,
            TimeTypeAttributeService timeTypeAttributeService)
        {
            _configStoreService = configStoreService;
            _prayerTimesConfigurationStorage = prayerTimesConfigurationStorage;
            _timeTypeAttributeService = timeTypeAttributeService;
        }

        public event Action OnInitializeCustomUI_EventTrigger = delegate { };

        public event Action OnViewModelInitialize_EventTrigger = delegate { };

        #region fields

        private readonly PrayerTimesConfigurationStorage _prayerTimesConfigurationStorage;
        private readonly IConfigStoreService _configStoreService;
        private readonly TimeTypeAttributeService _timeTypeAttributeService;
        
        private bool _isInitialized = false;

        #endregion fields

        #region properties

        public string TabTitle { get; set; }
        public ETimeType TimeType { get; set; }

        public List<ECalculationSource> CalculationSources { get; set; }
        public ECalculationSource SelectedCalculationSource { get; set; }

        public List<int> MinuteAdjustments { get; set; }
        public int SelectedMinuteAdjustment { get; set; }

        public bool IsTimeShown { get; set; }
        public bool IsTimeShownCheckBoxVisible { get; set; }

        public bool ShowMinuteAdjustmentPicker { get; set; }
        public bool ShowCalculationSourcePicker { get; set; }

        public ISettingConfigurationViewModel CustomSettingConfigurationViewModel { get; set; }

        #endregion properties

        #region public methods

        public void Initialize(ETimeType timeType)
        {
            TabTitle = $"{timeType}";
            TimeType = timeType;
            ShowCalculationSourcePicker = TimeType != ETimeType.DuhaEnd;
            IsTimeShownCheckBoxVisible = !_timeTypeAttributeService.NotHideableTypes.Contains(timeType);

            loadCalculationSource();
            loadMinuteAdjustmentSource();

            BaseCalculationConfiguration calculationConfiguration = _prayerTimesConfigurationStorage.GetConfiguration(TimeType);
            IsTimeShown = !IsTimeShownCheckBoxVisible || calculationConfiguration.IsTimeShown;

            SelectedCalculationSource = calculationConfiguration.Source;
            OnSelectedCalculationSourceChanged();

            SelectedMinuteAdjustment = calculationConfiguration.MinuteAdjustment;
            CustomSettingConfigurationViewModel?.AssignSettingValues(calculationConfiguration);

            OnViewModelInitialize_EventTrigger();
        }

        public void OnSelectedCalculationSourceChanged()
        {
            if (!_isInitialized)
                return;

            if (SelectedCalculationSource == ECalculationSource.Muwaqqit
                && MuwaqqitDegreeCalculationConfiguration.DegreePrayerTimeEvents.ContainsKey(TimeType))
            {
                CustomSettingConfigurationViewModel =
                        new MuwaqqitDegreeSettingConfigurationViewModel(
                            TimeType,
                            MuwaqqitDegreeCalculationConfiguration.DegreePrayerTimeEvents[TimeType]);
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
                    .Where(x => TimeType.IsSupportedBy(x))
                    .ToList();

            this.CalculationSources = calculationSources;
        }

        private void loadMinuteAdjustmentSource()
        {
            if (TimeType == ETimeType.DuhaEnd)
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
            profiles.First().Configurations[TimeType] = settings;
            _configStoreService.SaveProfiles(profiles).GetAwaiter().GetResult();
        }

        #endregion private methods
    }
}
