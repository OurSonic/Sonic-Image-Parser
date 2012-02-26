using System.Collections.Generic;

namespace JSONLevelConverter
{
    public static class Extensions
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue @default)
        {
            TValue output;
            if (dict.TryGetValue(key, out output))
                return output;
            return @default;
        }
    }
}