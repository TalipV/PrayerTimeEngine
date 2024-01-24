namespace PrayerTimeEngine.Core.Common
{
    public class AppConfig
    {
        public static readonly string DATABASE_PATH = 
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                "PrayerTimeEngineDB_ET.db");
    }
}
