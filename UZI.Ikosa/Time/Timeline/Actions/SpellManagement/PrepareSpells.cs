using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Time
{
    /// <summary>Recharge spell slots and fill up to a quarter of total spell capacity</summary>
    [Serializable]
    public class PrepareSpells : ActionBase
    {
        /// <summary>Recharge spell slots and fill up to a quarter of total spell capacity</summary>
        public PrepareSpells(Creature critter, IPreparedCasterClass caster)
            : this(critter, caster, new ActionTime(Minute.UnitFactor * 15, TimeType.TimelineScheduling))
        {
        }

        /// <summary>Recharge spell slots and fill up to a quarter of total spell capacity</summary>
        public PrepareSpells(Creature critter, IPreparedCasterClass caster, ActionTime actionTime)
            : base(critter, actionTime, true, false, @"201")
        {
            // NOTE: 1 hour for all spells
            // no less than 15 minutes for smaller proportion
            // otherwise, half hour for 1/2 spell slots, 3/4 hour for 3/4 spell slots (etc)
            // regardless, this recharges all spell slots that can be recharged
            _Caster = caster;
        }

        #region data
        private IPreparedCasterClass _Caster;
        #endregion

        public override string Key => @"Timeline.PrepareSpells";
        public override string DisplayName(CoreActor actor) => @"Prepare Spells";
        public IPreparedCasterClass Caster => _Caster;

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield return new PrepareSpellSlotAim(@"PrepareSpells.Slots", @"Spell Slots to Prepare", Caster, true);
            yield break;
        }

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
        {
            return null;
        }

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;

        public override bool IsStackBase(CoreActivity activity)
            => true;

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            activity.EnqueueRegisterPreEmptively(Budget);
            var _time = activity.Actor?.GetCurrentTime() ?? 0;
            var _prepTarget = activity.GetFirstTarget<PrepareSpellSlotsTarget>(@"PrepareSpells.Slots");
            if (_prepTarget != null)
            {
                // recharge every uncharged spell slot in this action
                foreach (var _slot in from _slotSet in Caster.AllSpellSlotSets
                                      from _level in _slotSet.SlotSet.AllLevels
                                      from _s in _level.Slots
                                      where !_s.IsCharged && _s.CanRecharge(_time)
                                      select _s)
                {
                    _slot.RechargeSlot(_time);
                }

                // abandon every spell that is different (recharge them)
                foreach (var _tSet in _prepTarget.SlotSets)
                {
                    var _set = Caster.GetSpellSlotSets(_tSet.SetIndex);
                    foreach (var _tLevel in _tSet.SlotLevels)
                    {
                        // each targeted slot that will have a spell prepared into it
                        var _level = _set[_tLevel.SlotLevel];
                        foreach (var _tSlot in _tLevel.SpellSlots.OfType<PreparedSpellSlotInfo>().Where(_pss => _pss.SpellSource != null).ToList())
                        {
                            // get the slot
                            var _slot = _level[_tSlot.SlotIndex];

                            // if there is no spell in the slot, no need to recharge it, as it was either recharged above, or carried over
                            if (!(_slot.PreparedSpell?.SpellDef?.DoSpellDefsMatch(_tSlot.SpellSource.SpellDef) ?? true))
                            {
                                // spell in slot does not match target, so clear it
                                _slot.RechargeSlot(_time);
                            }
                        }

                        // remove recharge only targeted slots, nothing more to do with them
                        _tLevel.SpellSlots.RemoveAll(_ss => (_ss is PreparedSpellSlotInfo _pss) && (_pss.SpellSource == null));
                    }

                    // cleanup levels that have no spell being slotted
                    _tSet.SlotLevels.RemoveAll(_lvl => _lvl.SpellSlots.Count == 0);
                }

                // cleanup sets with nothing left
                _prepTarget.SlotSets.RemoveAll(_s => _s.SlotLevels.Count == 0);

                // notify (and prepare if necessary
                if (_prepTarget.SlotSets.Any())
                {
                    var _fill = new FillQuarterSpellSlotsStep(activity, Caster);
                    activity.EnqueueActivityResultOnStep(_fill, $@"All spell slots recharged for {Caster.ClassName}; some spells prepared.");
                    return _fill;
                }
                return activity.GetActivityResultNotifyStep($@"All spell slots recharged for {Caster.ClassName}; some spells prepared.");
            }
            return activity.GetActivityResultNotifyStep($@"No prepare spell slots target provided");
        }
    }
}
