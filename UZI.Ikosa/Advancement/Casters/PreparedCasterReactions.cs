using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Advancement
{
    [Serializable]
    public class PreparedCasterReactions : GroupMasterAdjunct, ICanReactToStepComplete
    {
        public PreparedCasterReactions(ReactorStepCompleteGroup group, IPreparedCasterClass caster)
            : base(caster, group)
        {
        }

        public Creature Creature => Anchor as Creature;
        public ReactorStepCompleteGroup ReactorStepCompleteGroup => Group as ReactorStepCompleteGroup;
        public IPreparedCasterClass PreparedCaster => Source as IPreparedCasterClass;

        public bool IsFunctional => PreparedCaster.IsPowerClassActive;

        public override object Clone()
            => new PreparedCasterReactions(ReactorStepCompleteGroup, PreparedCaster);

        public bool CanReactToStepComplete(CoreStep step)
            => (Creature.GetLocalActionBudget()?.IsTwitchAvailable ?? false)
            && (from _spell in PreparedCaster.AllSpellSlots
                let _def = _spell.PreparedSpell.SpellDef
                where _def.ActionTime.ActionTimeType == Contracts.TimeType.Reactive
                from _mode in _def.SpellModes
                let _r = _mode.GetCapability<IReactToStepCompleteCapable>()
                where _r?.WillReactToStepComplete(Creature, step) ?? false
                select _mode).Any();

        public void ReactToStepComplete(CoreStep step)
        {
            var _reactions = (from _spell in PreparedCaster.AllSpellSlots
                              let _def = _spell.PreparedSpell.SpellDef
                              where _def.ActionTime.ActionTimeType == Contracts.TimeType.Reactive
                              from _mode in _def.SpellModes
                              let _r = _mode.GetCapability<IReactToStepCompleteCapable>()
                              where _r?.WillReactToStepComplete(Creature, step) ?? false
                              select (Spell: _spell, Def: _def, Mode: _mode)).ToList();
            if (_reactions.Any())
            {
                // start a process to allow caster to select a reaction...
                var _cx = 100;
                string _orderGen() => $@"{_cx++:00#}";
                var _castings = (from _r in _reactions
                                 orderby _r.Spell.SlotLevel, _r.Spell.PreparedSpell.DisplayName
                                 select new CastSpell(_r.Spell.PreparedSpell, _r.Mode,
                                     new ActionTime(Contracts.TimeType.Twitch), _r.Spell, _orderGen()))
                                .ToList();

                // start a process to allow caster to select a reaction...
                step.StartNewProcess(
                    new ReactiveCheck(step.GetStepInfo(Creature), Creature, 
                        _castings.Select(_c => ((IActionProvider)PreparedCaster, _c as ActionBase))),
                    @"React to Condition");
            }
        }
    }
}
