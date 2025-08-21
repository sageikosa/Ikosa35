using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Items.Shields;

namespace Uzi.Ikosa
{
    [Serializable]
    public class ShieldProficiencyDelta : ProficiencyDelta
    {
        public ShieldProficiencyDelta(ProficiencySet profSet)
            : base(profSet)
        {
        }

        public override IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
        {
            // find all wielded shields (shields being held that are slotted, not just held)
            var _penalty = 0;
            var _powerLevel = _Set.Creature.AdvancementLog.NumberPowerDice;
            foreach (var _shield in from _i in _Set.Creature.Body.ItemSlots.HeldObjects
                                    let _sb = _i as ShieldBase
                                    where (_sb != null) && (_sb.MainSlot != null)
                                    select _sb)
            {
                if (!_Set.IsProficientWith(_shield, _powerLevel))
                {
                    _penalty += _shield.CheckPenalty.EffectiveValue;
                }
            }

            if (_penalty != 0)
            {
                yield return new QualifyingDelta(_penalty, typeof(ShieldProficiencyDelta), @"Shield non-proficiency");
            }

            // default, no penalties
            yield break;
        }
    }
}
