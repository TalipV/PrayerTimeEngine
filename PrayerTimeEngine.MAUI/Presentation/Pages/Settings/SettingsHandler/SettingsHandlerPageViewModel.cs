using MvvmHelpers;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;
using PrayerTimeEngine.Presentation.Pages.Settings.SettingsContent;
using PrayerTimeEngine.Presentation.Services.Navigation;
using PrayerTimeEngine.Presentation.Services.SettingsContentPageFactory;
using PropertyChanged;

namespace PrayerTimeEngine.Presentation.Pages.Settings.SettingsHandler;

/// <summary>
/// Dummy class for <see cref="INavigationService"/>.
/// </summary>
public abstract class CustomBaseViewModel : BaseViewModel
{
    public abstract void Initialize(params object[] parameter);
}

[AddINotifyPropertyChangedInterface]
public partial class SettingsHandlerPageViewModel(
        SettingsContentPageFactory settingsContentPageFactory,
        TimeTypeAttributeService timeTypeAttributeService
    ) : CustomBaseViewModel
{
    public event Action Initialized = delegate { };

    #region fields

    #endregion fields

    #region properties

    public List<SettingsContentPage> SettingsContentPages = [];

    #endregion properties

    #region public methods

    public override void Initialize(params object[] parameter)
    {
        if (parameter.Length != 2)
        {
            throw new ArgumentException($"{nameof(parameter)} does not contain exactly two values");
        }

        if (parameter[0] is not DynamicProfile profile)
        {
            throw new ArgumentException($"{nameof(parameter)}[0] is not a {nameof(Profile)}");
        }

        if (parameter[1] is not EPrayerType prayerTime)
        {
            throw new ArgumentException($"{nameof(parameter)}[1] is not an {nameof(EPrayerType)}");
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
