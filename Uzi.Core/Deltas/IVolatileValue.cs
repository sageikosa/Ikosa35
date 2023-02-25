using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core.Contracts;

namespace Uzi.Core
{
    public interface IVolatileValue : IControlChange<DeltaValue>
    {
        /// <summary>Unqualified effective value</summary>
        int EffectiveValue { get; }

        /// <summary>Effective value for a condition</summary>
        int QualifiedValue(Qualifier qualification, DeltaCalcInfo calcInfo = null);
        VolatileValueInfo ToVolatileValueInfo(Qualifier qualifier = null);
    }

    public static class IVolatileValueStatics
    {
        public static DeltaCalcInfo GetDeltaCalcInfo(this IVolatileValue self, Qualifier qualifier, string title)
        {
            var _calc = new DeltaCalcInfo(qualifier?.Actor?.ID ?? Guid.Empty, title);
            self.QualifiedValue(qualifier, _calc);
            return _calc;
        }
    }
}
