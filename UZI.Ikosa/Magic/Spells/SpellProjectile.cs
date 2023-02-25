using System;
using Uzi.Ikosa.Actions;
using Uzi.Core;

namespace Uzi.Ikosa.Magic
{
    /// <summary>
    /// To be used by spells ?
    /// </summary>
    [Serializable]
    public class SpellProjectile : IRangedSource
    {
        public SpellProjectile(RangedAim aim, CoreActor actor, PowerUse<SpellSource> powerUse, IInteract target)
        {
            var _iData = new ActionInteractData(powerUse);
            var _interact = new Interaction(actor, aim.Range, target, _iData);
            var _range = (int)(aim.Range.EffectiveRange(actor, powerUse.ActionClassLevel.QualifiedValue(_interact)));
            _RIncr = _range;
            _Max = _range;
        }

        public SpellProjectile(int rangeIncrement ,int maxRange)
        {
            _RIncr = rangeIncrement;
            _Max = maxRange;
        }

        private int _RIncr;
        private int _Max;

        public int RangeIncrement { get { return _RIncr; } }
        public int MaxRange { get { return _Max; } }
    }
}
