using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Items.Armor;
using Uzi.Ikosa.Items;

namespace Uzi.Ikosa
{
    [Serializable]
    public class ArmorProficiencyDelta : ProficiencyDelta
    {
        public ArmorProficiencyDelta(ProficiencySet profSet)
            : base(profSet)
        {
        }

        public override IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
        {
            // find all worn armors
            var _penalty = 0;
            var _powerLevel = _Set.Creature.AdvancementLog.NumberPowerDice;
            foreach (var _armor in from _is in _Set.Creature.Body.ItemSlots.AllSlots
                                   where _is.SlottedItem != null
                                   && _is.SlotType.Equals(ItemSlot.ArmorRobeSlot, StringComparison.OrdinalIgnoreCase)
                                   let _ab = _is.SlottedItem as ArmorBase
                                   where _ab != null
                                   select _ab)
            {
                if (!_Set.IsProficientWith(_armor, _powerLevel))
                {
                    _penalty += _armor.CheckPenalty.EffectiveValue;
                }
            }

            if (_penalty != 0)
            {
                yield return new QualifyingDelta(_penalty, typeof(ArmorProficiencyDelta), @"Armor non-proficiency");
            }

            // default, no penalties
            yield break;
        }
    }
}
