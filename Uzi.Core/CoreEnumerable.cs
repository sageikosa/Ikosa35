using System.Collections.Generic;
using System.Linq;

namespace Uzi.Core
{
    public static class CoreEnumerable
    {
        /// <summary>Converts any object to an enumerable</summary>
        public static IEnumerable<T> ToEnumerable<T>(this T self)
        {
            yield return self;
            yield break;
        }

        /// <summary>
        /// Convert a sequence of values to an IEnumerable
        /// </summary>
        public static IEnumerable<Any> GetEnumerable<Any>(params Any[] any)
        {
            return any;
        }
    }
}
