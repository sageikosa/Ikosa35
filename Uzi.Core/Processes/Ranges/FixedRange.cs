using System;
using Uzi.Core.Contracts;

namespace Uzi.Core
{
    [Serializable]
    public class FixedRange : Range
    {
        public FixedRange(double units)
        {
            Units = units;
        }

        public double Units { get; private set; }

        public override double EffectiveRange(CoreActor actor, int powerLevel)
        {
            return Units;
        }
        private static FixedRange _One = new FixedRange(1);
        public static FixedRange One { get { return _One; } }

        public override RangeInfo ToRangeInfo(CoreAction action, CoreActor actor)
        {
            return ToInfo<RangeInfo>(action, actor);
        }
    }
}
