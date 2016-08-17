using System;
using System.Collections.Generic;

namespace Es.Splay
{
    public static class VivifyEx
    {
        public static TV Vivify<TK, TV>(this IDictionary<TK, TV> dict, TK key, Func<TV> valueFactory)
        {
            TV v;
            if (dict.TryGetValue(key, out v))
                return v;

            v = valueFactory();
            dict[key] = v;
            return v;
        }
    }
}