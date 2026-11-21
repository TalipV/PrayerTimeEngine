using Android.App;
using Android.Content;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using PrayerTimeEngine.Presentation;

namespace PrayerTimeEngine.Platforms.Android;

/// <summary>
/// The native date picker dialog colors its buttons (OK / Cancel) with the theme's colorPrimary, which is the
/// dark background color of the app (see Resources/values/colors.xml), so the buttons were invisible.
/// Only the buttons of this dialog get the accent color instead of changing colorPrimary for the whole app.
/// </summary>
public class AccentButtonsDatePickerHandler : DatePickerHandler
{
    protected override DatePickerDialog CreateDatePickerDialog(int year, int month, int day)
    {
        DatePickerDialog dialog = base.CreateDatePickerDialog(year, month, day);

        // the buttons only exist once the dialog is shown
        dialog.ShowEvent += (_, _) =>
        {
            global::Android.Graphics.Color buttonColor = AppColors.Accent.ToPlatform();
            dialog.GetButton((int)DialogButtonType.Positive)?.SetTextColor(buttonColor);
            dialog.GetButton((int)DialogButtonType.Negative)?.SetTextColor(buttonColor);
        };

        return dialog;
    }
}
