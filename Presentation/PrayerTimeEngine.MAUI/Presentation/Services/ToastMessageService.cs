using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using PrayerTimeEngine.Presentation.Localization;

namespace PrayerTimeEngine.Presentation.Services;

// AppColors lives in the PrayerTimeEngine.Presentation namespace

public class ToastMessageService(IDispatcher dispatcher)
{
    /// <summary>
    /// Shows a snackbar with a single action button. Unlike <see cref="Show"/> (a plain toast),
    /// a snackbar can carry a tappable button and stays visible for <paramref name="duration"/>.
    /// </summary>
    public void ShowAction(string text, string actionText, Action action, TimeSpan? duration = null)
    {
        dispatcher.Dispatch(async () =>
        {
            var visualOptions = new SnackbarOptions
            {
                BackgroundColor = AppColors.Surface,
                TextColor = AppColors.Text,
                ActionButtonTextColor = AppColors.Text,
                CornerRadius = new CornerRadius(10),
            };

            await Snackbar.Make(
                    message: text,
                    action: action,
                    actionButtonText: actionText,
                    duration: duration ?? TimeSpan.FromSeconds(5),
                    visualOptions: visualOptions)
            .Show();
        });
    }

    public void Show(string text)
    {
        dispatcher.Dispatch(async () => await Toast.Make(
                    message: text,
                    duration: ToastDuration.Short,
                    textSize: 14)
            .Show());
    }

    public void ShowWarning(string text)
    {
        dispatcher.Dispatch(async () => await Toast.Make(
                    message: string.Format(AppResources.Toast_Warning, text),
                    duration: ToastDuration.Short,
                    textSize: 14)
            .Show());
    }

    public void ShowError(string text)
    {
        dispatcher.Dispatch(async () => await Toast.Make(
                    message: string.Format(AppResources.Toast_Error, text),
                    duration: ToastDuration.Short,
                    textSize: 14)
            .Show());
    }
}
