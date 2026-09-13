namespace PrayerTimeEngine.Presentation;

public class DebugUtil
{
    public static string GenerateDebugID()
    {
        return Guid.NewGuid().ToString()[..5];
    }
}
