using System;
using Uzi.Core.Contracts;

namespace Uzi.Core
{
    [Serializable]
    public class PowerLevelRange: Range
    {
        public PowerLevelRange(double unitsPerLevel)
        {
            UnitsPerLevel = unitsPerLevel;
        }

        public double UnitsPerLevel { get; private set; }

        public override double EffectiveRange(CoreActor actor, int powerLevel)
        {
            return UnitsPerLevel * powerLevel;
        }

        public override RangeInfo ToRangeInfo(CoreAction action, CoreActor actor)
        {
            return ToInfo<RangeInfo>(action, actor);
        }
    }
}
