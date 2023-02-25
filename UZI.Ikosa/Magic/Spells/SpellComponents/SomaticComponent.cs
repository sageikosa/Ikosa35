using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Dice;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Weapons;

namespace Uzi.Ikosa.Magic
{
    [Serializable]
    public class SomaticComponent : SpellComponent, IMonitorChange<Condition>, IMonitorChange<ISlottedItem>
    {
        public override string ComponentName => @"Somatic";

        private List<HoldingSlot> _HoldSlots = null;

        /// <summary>Cannot be helpless, and must have a free hand, or be holding a double weapon in two hands.</summary>
        public override bool CanStartActivity(Creature caster)
            => !caster.Conditions.Any(_c => _c.Name == Condition.Helpless)
            && caster.Body.ItemSlots.AllSlots.OfType<HoldingSlot>()
                .Any(_h =>
                {
                    if (_h.SlottedItem == null)
                        return true;

                    if (_h.SlottedItem is IMeleeWeapon _melee)
                    {
                        return (_melee?.HeadCount > 1) && (_melee?.SecondarySlot != null);
                    }
                    return false;
                });

        public override void StartUse(CoreActivity activity)
        {
            if (activity?.Actor is Creature _caster)
            {
                _caster.Conditions.AddChangeMonitor(this);
                _HoldSlots = _caster.Body.ItemSlots.AllSlots.OfType<HoldingSlot>().ToList();
                foreach (var _h in _HoldSlots)
                {
                    _h.AddChangeMonitor(this);
                }
                if (((activity.Action as CastSpell)?.PowerActionSource?.CasterClass.MagicType ?? Contracts.MagicType.Arcane)
                    == Contracts.MagicType.Arcane)
                {
                    // get effective range
                    var _qualifier = new Qualifier(activity.Actor, activity, null);
                    var _rangeLow = _caster.ArcaneSpellFailureChance.QualifiedValue(_qualifier, Deltable.GetDeltaCalcNotify(activity.Actor?.ID, @"Arcane Somatic Failure").DeltaCalc);

                    // play the percentages
                    if (DieRoller.RollDie(activity.Actor.ID, 100, @"Somatic", @"Failure Chance", activity.Actor.ID) <= _rangeLow)
                    {
                        HasFailed = true;
                    }
                }

                // TODO: spell-craft check for spell-craft trained creatures that can see the caster (in any tracker)
            }
        }

        public override void StopUse(CoreActivity activity)
        {
            if (activity?.Actor is Creature _caster)
            {
                _caster.Conditions.RemoveChangeMonitor(this);
                foreach (var _h in _HoldSlots)
                {
                    _h.RemoveChangeMonitor(this);
                }
                _HoldSlots = null;
            }
        }

        public override bool WillUseSucceed(CoreActivity activity)
            => CanStartActivity(activity.Actor as Creature);

        public void PreTestChange(object sender, AbortableChangeEventArgs<Condition> args) { }
        public void PreValueChanged(object sender, ChangeValueEventArgs<Condition> args) { }

        public void ValueChanged(object sender, ChangeValueEventArgs<Condition> args)
        {
            if (args?.Action == @"Add")
            {
                // became helpless before finishing
                if (args?.NewValue?.Name == Condition.Helpless)
                {
                    HasFailed = true;
                }
            }
        }

        public void PreTestChange(object sender, AbortableChangeEventArgs<ISlottedItem> args) { }
        public void PreValueChanged(object sender, ChangeValueEventArgs<ISlottedItem> args) { }

        public void ValueChanged(object sender, ChangeValueEventArgs<ISlottedItem> args)
        {
            // if there are no empty holding slots, fail...
            if (!_HoldSlots.Any(_h =>
                {
                    if (_h.SlottedItem == null)
                        return true;

                    if (_h.SlottedItem is IMeleeWeapon _melee)
                    {
                        return (_melee?.HeadCount > 1) && (_melee?.SecondarySlot != null);
                    }
                    return false;
                }))
            {
                HasFailed = true;
            }
        }
    }
}
