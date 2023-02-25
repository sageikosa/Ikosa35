using System;
using System.Collections.Generic;

namespace Uzi.Core
{
    /// <summary>ConstDeltable cannot have its BaseValue changed once constructed</summary>
    [Serializable]
    public class ConstDeltable : Deltable, ISupplyQualifyDelta
    {
        /// <summary>ConstDeltable cannot have its BaseValue changed once constructed</summary>
        public ConstDeltable(int seed)
            : base(seed)
        {
        }

        public override int BaseValue
        {
            get
            {
                return base.BaseValue;
            }
            set
            {
                // ignore
            }
        }

        public virtual IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
            => QualifiedDeltas(qualify, this, @"base");
    }
}
