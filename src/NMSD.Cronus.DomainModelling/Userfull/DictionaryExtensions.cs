using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMSD.Cronus
{
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Merges two dictionaries while skips the elements within extra with keys already contained by the origin.
        /// </summary>
        public static IDictionary<TKey, TValue> Merge<TKey, TValue>(this IDictionary<TKey, TValue> origin, IDictionary<TKey, TValue> extra)
        {
            return origin.Concat(extra.Where(kvp => !origin.ContainsKey(kvp.Key))).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
    }
}
