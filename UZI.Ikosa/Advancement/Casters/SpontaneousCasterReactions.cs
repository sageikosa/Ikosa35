using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Advancement
{
    [Serializable]
    public class SpontaneousCasterReactions : GroupMasterAdjunct, ICanReactToStepComplete
    {
        public SpontaneousCasterReactions(ReactorStepCompleteGroup group, ISpontaneousCaster caster)
            : base(caster, group)
        {
        }

        public Creature Creature => Anchor as Creature;
        public ReactorStepCompleteGroup ReactorStepCompleteGroup => Group as ReactorStepCompleteGroup;
        public ISpontaneousCaster SpontaneousCaster => Source as ISpontaneousCaster;

        public bool IsFunctional => SpontaneousCaster.IsPowerClassActive;

        public override object Clone()
            => new SpontaneousCasterReactions(ReactorStepCompleteGroup, SpontaneousCaster);

        public bool CanReactToStepComplete(CoreStep step)
            => (Creature.GetLocalActionBudget()?.IsTwitchAvailable ?? false)
            && (from _spell in SpontaneousCaster.KnownSpells.AllKnown
                let _def = _spell.SpellDef
                where _def.ActionTime.ActionTimeType == Contracts.TimeType.Reactive
                from _mode in _def.SpellModes
                let _r = _mode.GetCapability<IReactToStepCompleteCapable>()
                where _r?.WillReactToStepComplete(Creature, step) ?? false
                select _mode).Any();

        public void ReactToStepComplete(CoreStep step)
        {
            var _reactions = (from _spell in SpontaneousCaster.KnownSpells.AllKnown
                              let _def = _spell.SpellDef
                              where _def.ActionTime.ActionTimeType == Contracts.TimeType.Reactive
                              from _mode in _def.SpellModes
                              let _r = _mode.GetCapability<IReactToStepCompleteCapable>()
                              where _r?.WillReactToStepComplete(Creature, step) ?? false
                              select (Spell: _spell, Mode: _mode)).ToList();
            if (_reactions.Any())
            {
                // start a process to allow caster to select a reaction...
                var _cx = 100;
                string _orderGen() => $@"{_cx++:00#}";
                var _castings = (from _r in _reactions
                                 let _slot = SpontaneousCaster.GetAvailableSlot(_r.Spell.SlotLevel)
                                 orderby _slot.SlotLevel, _r.Spell.Name
                                 select new CastSpell(new SpellSource(SpontaneousCaster, _r.Spell.SlotLevel,
                                     _slot.SlotLevel, true, _r.Spell.SpellDef), _r.Mode,
                                     new ActionTime(Contracts.TimeType.Twitch), _slot, _orderGen()))
                                .ToList();

                step.StartNewProcess(
                    new ReactiveCheck(step.GetStepInfo(Creature),Creature,
                        _castings.Select(_c => ((IActionProvider)SpontaneousCaster, _c as ActionBase))),
                    @"React to Condition");
            }
        }
    }
}
