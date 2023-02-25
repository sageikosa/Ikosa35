using System;
using Uzi.Core;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Actions
{
    /// <summary>Typically 400-ft + 40-ft/power-level</summary>
    [Serializable]
    public class FarRange : Range
    {
        /// <summary>Typically 400-ft + 40-ft/power-level</summary>
        public FarRange()
        {
            _Factor = 1d;
        }

        /// <summary>Typically 400-ft + 40-ft/power-level (or doubled)</summary>
        public FarRange(bool doubler)
        {
            _Factor = doubler ? 2d : 1d;
        }

        private double _Factor;
        public double Factor { get { return _Factor; } }

        public override double EffectiveRange(CoreActor creature, int casterLevel)
        {
            return (400d * Factor) + (40d * Factor * casterLevel);
        }

        public override RangeInfo ToRangeInfo(CoreAction action, CoreActor actor)
        {
            var _info = ToInfo<RangeInfo>(action, actor);
            return _info;
        }
    }
}
