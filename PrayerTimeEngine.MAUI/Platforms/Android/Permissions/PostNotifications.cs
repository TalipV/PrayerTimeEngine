using Android;
using static Microsoft.Maui.ApplicationModel.Permissions;

namespace PrayerTimeEngine.Platforms.Android.Permissions;

public class PostNotifications : BasePlatformPermission
{
    public override (string androidPermission, bool isRuntime)[] RequiredPermissions
    {
        get
        {
            if (!OperatingSystem.IsAndroidVersionAtLeast(33))
                throw new NotImplementedException();

            return
            [
                (Manifest.Permission.PostNotifications, true)
            ];
        }
    }
}
