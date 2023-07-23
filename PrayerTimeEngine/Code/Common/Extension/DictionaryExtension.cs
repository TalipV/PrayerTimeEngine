using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrayerTimeEngine.Code.Common.Extension
{
    public static class DictionaryExtension
    {
        public static Dictionary<TValue, TKey> GetInverseDictionary<TValue, TKey>(this Dictionary<TKey, TValue> dictionary)
        {
            return dictionary.ToDictionary((i) => i.Value, (i) => i.Key);
        }
    }
}
