using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace PrayerTimeEngine.Presentation.Service
{
    public class ToastMessageService(IDispatcher dispatcher)
    {
        public void Show(string text)
        {
            dispatcher.Dispatch(async () =>
            {
                await Toast.Make(
                        message: text,
                        duration: ToastDuration.Short,
                        textSize: 14)
                .Show();
            });
        }

        public void ShowWarning(string text)
        {
            dispatcher.Dispatch(async () =>
            {
                await Toast.Make(
                        message: $"WARNUNG: {text}",
                        duration: ToastDuration.Short,
                        textSize: 14)
                .Show();
            });
        }

        public void ShowError(string text)
        {
            dispatcher.Dispatch(async () =>
            {
                await Toast.Make(
                        message: $"FEHLER: {text}",
                        duration: ToastDuration.Short,
                        textSize: 14)
                .Show();
            });
        }
    }
}
