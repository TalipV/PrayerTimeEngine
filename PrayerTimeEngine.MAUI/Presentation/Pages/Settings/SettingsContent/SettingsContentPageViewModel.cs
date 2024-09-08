using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;
using PrayerTimeEngine.Presentation.Pages.Settings.SettingsContent.Custom;
using PropertyChanged;

namespace PrayerTimeEngine.Presentation.Pages.Settings.SettingsContent;

[AddINotifyPropertyChangedInterface]
public class SettingsContentPageViewModel(
        IProfileService profileService,
        TimeTypeAttributeService timeTypeAttributeService
    )
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
        -3.5, -4.0, -4.5, -5.0, -5.5, -6.0, -6.5, -7.0, -7.5, -8.0, -8.5, -9.0, -9.5, -10.0
    }.AsReadOnly();

    public static readonly IReadOnlyCollection<double> ISHTIBAQ_SELECTABLE_DEGREES = new List<double>
    {
        -9.0, -9.5, -10.0, -10.5, -11.0
    }.AsReadOnly();

    public static readonly IReadOnlyCollection<double> MODERATE_SELECTABLE_DEGREES_POSITIVE =
        MODERATE_SELECTABLE_DEGREES_NEGATIVE.Select(Math.Abs).ToList().AsReadOnly();

    #endregion static fields

    public event Action OnInitializeCustomUI_EventTrigger = delegate { };
    public event Action OnViewModelInitialize_EventTrigger = delegate { };

    #region fields

    private bool _isInitialized = false;

    #endregion fields

    #region properties

    public string TabTitle { get; set; }
    public ETimeType TimeType { get; set; }

    public List<EDynamicPrayerTimeProviderType> DynamicPrayerTimeProviders { get; set; }
    public EDynamicPrayerTimeProviderType SelectedDynamicPrayerTimeProvider { get; set; }

    public List<int> MinuteAdjustments { get; set; }
    public int SelectedMinuteAdjustment { get; set; }

    public bool IsTimeShown { get; set; }
    public bool IsTimeShownCheckBoxVisible { get; set; }

    public bool ShowMinuteAdjustmentPicker { get; set; }
    public bool ShowDynamicPrayerTimeProviderPicker { get; set; }

    public ISettingConfigurationViewModel CustomSettingConfigurationViewModel { get; set; }

    public DynamicProfile Profile { get; set; }

    #endregion properties

    #region public methods

    public void Initialize(DynamicProfile profile, ETimeType timeType)
    {
        TabTitle = $"{timeType}";
        TimeType = timeType;
        ShowDynamicPrayerTimeProviderPicker = !timeTypeAttributeService.ConfigurableSimpleTypes.Contains(timeType);
        IsTimeShownCheckBoxVisible = !timeTypeAttributeService.NotHideableTypes.Contains(timeType);

        DynamicPrayerTimeProviders = getDynamicPrayerTimeProvider();
        MinuteAdjustments = getMinuteAdjustmentSource();

        Profile = profile;
        GenericSettingConfiguration calculationConfiguration =
            profileService.GetTimeConfig(Profile, TimeType)
            ?? new GenericSettingConfiguration { TimeType = TimeType };

        IsTimeShown = !IsTimeShownCheckBoxVisible || calculationConfiguration.IsTimeShown;
        SelectedDynamicPrayerTimeProvider = calculationConfiguration.Source;
        SelectedMinuteAdjustment = calculationConfiguration.MinuteAdjustment;
        _isInitialized = true;

        OnSelectedDynamicPrayerTimeProviderChanged();
        CustomSettingConfigurationViewModel?.AssignSettingValues(calculationConfiguration);
        OnViewModelInitialize_EventTrigger();
    }

    public void OnSelectedDynamicPrayerTimeProviderChanged()
    {
        if (!_isInitialized)
            return;

        if (SelectedDynamicPrayerTimeProvider == EDynamicPrayerTimeProviderType.Muwaqqit
            && timeTypeAttributeService.DegreeTypes.Contains(TimeType))
        {
            CustomSettingConfigurationViewModel = new MuwaqqitDegreeSettingConfigurationViewModel(TimeType);
        }
        else
        {
            CustomSettingConfigurationViewModel = null;
        }

        OnInitializeCustomUI_EventTrigger.Invoke();
    }

    public Task OnDisappearing()
    {
        GenericSettingConfiguration settings = getCurrentCalculationConfiguration();
        return profileService.UpdateTimeConfig(Profile, TimeType, settings, default);
    }

    #endregion public methods

    #region private methods

    private GenericSettingConfiguration getCurrentCalculationConfiguration()
    {
        if (CustomSettingConfigurationViewModel is not null)
        {
            return CustomSettingConfigurationViewModel.BuildSetting(SelectedMinuteAdjustment, IsTimeShown);
        }

        return getGeneralCalculationConfiguration();
    }

    private GenericSettingConfiguration getGeneralCalculationConfiguration()
    {
        return
            new GenericSettingConfiguration
            {
                TimeType = TimeType,
                MinuteAdjustment = SelectedMinuteAdjustment,
                Source = SelectedDynamicPrayerTimeProvider,
                IsTimeShown = IsTimeShown
            };
    }

    private List<EDynamicPrayerTimeProviderType> getDynamicPrayerTimeProvider()
    {
        if (!timeTypeAttributeService.TimeTypeCompatibleSources.TryGetValue(TimeType, out IReadOnlyList<EDynamicPrayerTimeProviderType> dynamicPrayerTimeProviders))
        {
            return [];
        }

        return [.. dynamicPrayerTimeProviders];
    }

    private List<int> getMinuteAdjustmentSource()
    {
        if (TimeType == ETimeType.DuhaEnd)
        {
            return Enumerable.Range(-40, 35).ToList();
        }
        else if (TimeType == ETimeType.MaghribSufficientTime)
        {
            return [15, 20, 25, 30, 35];
        }
        else
        {
            return Enumerable.Range(start: -30, count: 46).ToList();
        }
    }

    #endregion private methods
}
