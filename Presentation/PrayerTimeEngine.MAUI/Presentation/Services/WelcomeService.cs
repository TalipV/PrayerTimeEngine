using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Extensions;
using PrayerTimeEngine.Core.Common;
using PrayerTimeEngine.Presentation.Popups;

namespace PrayerTimeEngine.Presentation.Services;

/// <summary>
/// Shows the <see cref="WelcomePopup"/> until the user has confirmed it once.
/// </summary>
public class WelcomeService(
        IPreferenceService preferenceService
    )
{
    private const string WELCOME_CONFIRMED_KEY = "WELCOME_CONFIRMED_VERSION";

    /// <summary>
    /// Raise it when the welcome text changes in a way everybody has to confirm again
    /// (e.g. new conditions of use); the popup is then shown once more, also to existing users.
    /// </summary>
    private const string WELCOME_VERSION = "1";

    /// <summary>To be called on the main thread once the main page is ready, before any other startup dialog.</summary>
    public async Task ShowIfNotConfirmedAsync()
    {
        if (preferenceService.GetValue(WELCOME_CONFIRMED_KEY, defaultValue: "") == WELCOME_VERSION)
        {
            return;
        }

        Page? rootPage = Application.Current?.Windows.FirstOrDefault()?.Page;
        Page? page = (rootPage as NavigationPage)?.CurrentPage ?? rootPage;

        if (page is null)
        {
            return;
        }

        IPopupResult<bool> popupResult = await page.ShowPopupAsync<bool>(new WelcomePopup(), WelcomePopup.CreatePopupOptions());

        // only the button counts as confirmation; otherwise the notice comes again on the next start
        if (popupResult.Result)
        {
            preferenceService.SetValue(WELCOME_CONFIRMED_KEY, WELCOME_VERSION);
        }
    }
}
