using System;
using Uzi.Core;
using Uzi.Ikosa.Contracts;
using Uzi.Visualize;

namespace Uzi.Ikosa.Actions
{
    /// <summary>
    /// Within melee range (of creature's touch), and as such, can include targets in hand.
    /// </summary>
    [Serializable]
    public class MeleeRange : Range, ICellRange
    {
        /// <summary>Within melee range (of creature's touch), and as such, can include targets in hand.</summary>
        public MeleeRange()
            : base()
        {
        }

        public override double EffectiveRange(CoreActor creature, int powerLevel)
        {
            // NOTE: listed as grid cells, not map units
            var _critter = creature as Creature;
            if (_critter != null)
            {
                return (double)_critter.Body.ReachSquares.EffectiveValue;
            }
            return 0;
        }

        public IGeometricRegion GetTargetVolume(CoreActor creature, int powerLevel, IGeometricRegion source)
        {
            if (source != null && creature != null)
            {
                var _range = (int)EffectiveRange(creature, powerLevel);
                return new Cubic(
                    source.LowerZ - _range, source.LowerY - _range, source.LowerX - _range,
                    source.UpperZ + _range, source.UpperY + _range, source.UpperX + _range);
            }
            return null;
        }

        public override Core.Contracts.RangeInfo ToRangeInfo(CoreAction action, CoreActor actor)
        {
            var _info = ToInfo<MeleeRangeInfo>(action, actor);
            var _critter = actor as Creature;
            if ((_critter != null) && (_critter.Body != null))
            {
                _info.ReachSquares = _critter.Body.ReachSquares.EffectiveValue;
            }
            return _info;
        }
    }
}
