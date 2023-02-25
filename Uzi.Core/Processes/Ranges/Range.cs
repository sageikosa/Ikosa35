using System;
using Uzi.Core.Contracts;

namespace Uzi.Core
{
    /// <summary>
    /// Allows values to be determined based upon actor and power level conditions
    /// </summary>
    [Serializable]
    public abstract class Range
    {
        /// <summary>
        /// Calculate effective range
        /// </summary>
        /// <param name="actor">Actor trying to evaluate the range</param>
        /// <param name="powerLevel">used by range for variable factors (if needed)</param>
        /// <returns></returns>
        public abstract double EffectiveRange(CoreActor actor, int powerLevel);

        protected RInfo ToInfo<RInfo>(CoreAction action, CoreActor actor)
            where RInfo: RangeInfo, new()
        {
            var _info = new RInfo();
            _info.ClassLevel = action.CoreActionClassLevel(actor, this);
            _info.Value = this.EffectiveRange(actor, _info.ClassLevel);
            return _info;
        }

        public abstract RangeInfo ToRangeInfo(CoreAction action, CoreActor actor);
    }
}
