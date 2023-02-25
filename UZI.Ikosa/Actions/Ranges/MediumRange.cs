using System;
using Uzi.Core;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Actions
{
    /// <summary>Typically 100-ft + 10-ft/power-level</summary>
    [Serializable]
    public class MediumRange : Range
    {
        /// <summary>Typically 100-ft + 10-ft/power-level</summary>
        public MediumRange()
        {
            _Factor = 1d;
        }

        /// <summary>Typically 100-ft + 10-ft/power-level (or doubled)</summary>
        public MediumRange(bool doubler)
        {
            _Factor = doubler ? 2d : 1d;
        }

        private double _Factor;
        public double Factor { get { return _Factor; } }

        public override double EffectiveRange(CoreActor creature, int casterLevel)
        {
            return (100d * Factor) + (10d * Factor * casterLevel);
        }

        public override RangeInfo ToRangeInfo(CoreAction action, CoreActor actor)
        {
            var _info = ToInfo<RangeInfo>(action, actor);
            return _info;
        }
    }
}
