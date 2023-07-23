using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrayerTimeEngine.Code.Common.Extension
{
    public static class IEnumerableExtension
    {
        public static List<T> ToSingleElementLst<T>(this T obj)
        {
            return new List<T>() { obj };
        }
    }
}
