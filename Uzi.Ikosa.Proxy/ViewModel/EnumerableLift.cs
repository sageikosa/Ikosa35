using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public static class EnumerableLift
    {
        public static IEnumerable<Thing> ToEnumerable<Thing>(this Thing thing)
        {
            yield return thing;
        }
    }
}
