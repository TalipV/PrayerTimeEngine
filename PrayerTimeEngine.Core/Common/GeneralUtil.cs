namespace PrayerTimeEngine.Core.Common
{
    internal class GeneralUtil
    {
        // I don't know where to put this but I know that I don't want to always write this
        public static bool BetterEquals(object value1, object value2)
        {
            if (value1 is not null && value2 is not null)
            {
                return value1.Equals(value2);
            }
            else if (value1 is null && value2 is null)
            {
                return true;
            }
            else 
            {
                // one of them is null
                return false;
            }
        }
    }
}
