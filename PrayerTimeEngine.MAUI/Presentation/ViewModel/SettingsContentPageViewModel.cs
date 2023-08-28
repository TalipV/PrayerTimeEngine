using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain;
using PrayerTimeEngine.Core.Domain.Configuration.Interfaces;
using PrayerTimeEngine.Core.Domain.Configuration.Models;
using PrayerTimeEngine.Presentation.ViewModel.Custom;
using PropertyChanged;

namespace PrayerTimeEngine.Presentation.ViewModel
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

        public static readonly IReadOnlyCollection<double> MODERATE_SELECTABLE_DEGREES_NEGATIVE = new List<double>
        {
            -3.5, -4.0, -4.5, -5.0, -5.5, -6.0, -6.5, -7.0, -7.5
        }.AsReadOnly();

        public static readonly IReadOnlyCollection<double> ISHTIBAQ_SELECTABLE_DEGREES = new List<double>
        {
            -9.0, -9.5, -10.0, -10.5, -11.0
        }.AsReadOnly();

        public static readonly IReadOnlyCollection<double> MODERATE_SELECTABLE_DEGREES_POSITIVE =
            MODERATE_SELECTABLE_DEGREES_NEGATIVE.Select(Math.Abs).ToList().AsReadOnly();

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
            ShowCalculationSourcePicker = !_timeTypeAttributeService.ConfigurableSimpleTypes.Contains(timeType);
            IsTimeShownCheckBoxVisible = !_timeTypeAttributeService.NotHideableTypes.Contains(timeType);

            CalculationSources = getCalculationSource();
            MinuteAdjustments = getMinuteAdjustmentSource();

            GenericSettingConfiguration calculationConfiguration = _prayerTimesConfigurationStorage.GetConfiguration(TimeType);
            IsTimeShown = !IsTimeShownCheckBoxVisible || calculationConfiguration.IsTimeShown;
            SelectedCalculationSource = calculationConfiguration.Source;
            SelectedMinuteAdjustment = calculationConfiguration.MinuteAdjustment;
            _isInitialized = true;

            OnSelectedCalculationSourceChanged();
            CustomSettingConfigurationViewModel?.AssignSettingValues(calculationConfiguration);
            OnViewModelInitialize_EventTrigger();
        }

        public void OnSelectedCalculationSourceChanged()
        {
            if (!_isInitialized)
                return;

            if (SelectedCalculationSource == ECalculationSource.Muwaqqit
                && _timeTypeAttributeService.DegreeTypes.Contains(TimeType))
            {
                CustomSettingConfigurationViewModel =
                    new MuwaqqitDegreeSettingConfigurationViewModel(TimeType);
            }
            else
            {
                CustomSettingConfigurationViewModel = null;
            }

            OnInitializeCustomUI_EventTrigger.Invoke();
        }

        public void OnDisappearing()
        {
            GenericSettingConfiguration settings = getCurrentCalculationConfiguration();
            saveSettingsToProfile(settings);
        }

        #endregion public methods

        #region private methods

        private GenericSettingConfiguration getCurrentCalculationConfiguration()
        {
            return
                CustomSettingConfigurationViewModel?
                    .BuildSetting(SelectedMinuteAdjustment, IsTimeShown)
                        ?? getGeneralCalculationConfiguration();
        }

        private GenericSettingConfiguration getGeneralCalculationConfiguration()
        {
            return
                new GenericSettingConfiguration
                {
                    TimeType = TimeType,
                    MinuteAdjustment = SelectedMinuteAdjustment,
                    Source = SelectedCalculationSource,
                    IsTimeShown = IsTimeShown
                };
        }

        private List<ECalculationSource> getCalculationSource()
        {
            if (!_timeTypeAttributeService.TimeTypeCompatibleSources.TryGetValue(TimeType, out IReadOnlyList<ECalculationSource> calculationSources))
            {
                return new List<ECalculationSource>();
            }

            return calculationSources.ToList();
        }

        private List<int> getMinuteAdjustmentSource()
        {
            if (TimeType == ETimeType.DuhaEnd)
            {
                return Enumerable.Range(-40, 35).ToList();
            }
            else if(TimeType == ETimeType.MaghribSufficientTime)
            {
                return new List<int>() { 15, 20, 25, 30, 35};
            }
            else
            {
                return Enumerable.Range(-15, 30).ToList();
            }
        }

        private void saveSettingsToProfile(GenericSettingConfiguration settings)
        {
            Profile profile = _prayerTimesConfigurationStorage.GetProfiles().GetAwaiter().GetResult().First();
            profile.Configurations[TimeType] = settings;
            _configStoreService.SaveProfile(profile).GetAwaiter().GetResult();
        }

        #endregion private methods
    }
}
