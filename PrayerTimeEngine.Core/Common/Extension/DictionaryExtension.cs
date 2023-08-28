namespace PrayerTimeEngine.Core.Common.Extension
{
    public static class DictionaryExtension
    {
        public static Dictionary<TValue, TKey> GetInverseDictionary<TValue, TKey>(this Dictionary<TKey, TValue> dictionary)
        {
            return dictionary.ToDictionary((i) => i.Value, (i) => i.Key);
        }
    }
}
