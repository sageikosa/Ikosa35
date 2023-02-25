using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Skills;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items
{
    /// <summary>[Adjunct, IActionProvider, IProtectable]</summary>
    [Serializable]
    public class StaffTrigger : SpellTrigger
    {
        #region Construction
        public StaffTrigger(SpellSource source, IPowerBattery battery, int chargePerUse)
            : base(source, battery, chargePerUse)
        {
        }
        #endregion

        #region public override IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        public override IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            if (IsActive && (PowerBattery.AvailableCharges >= ChargePerUse))
            {
                if ((budget.Actor is Creature _critter)
                    && IsExposedTo(_critter))
                {
                    var _budget = budget as LocalActionBudget;

                    // if creature has ranks in use magic device, some of the limits may be surmountable with checks during the action
                    var _hasUseMagicDevice = _critter.Skills[typeof(UseMagicItemSkill)].BaseValue > 0;

                    if ((_hasUseMagicDevice || UseDirectly(_critter)) && _budget.CanPerformRegular)
                    {
                        var _mx = 0;
                        foreach (ISpellMode _mode in SpellSource.SpellDef.SpellModes)
                        {
                            yield return new TriggerSpell(_critter, this, _mode, new ActionTime(TimeType.Regular), $@"{_mx:0#}");
                            _mx++;
                        }
                    }
                }
            }
            yield break;
        }
        #endregion

        #region public override SpellSource ActorSpellSource(CoreActor actor)
        /// <summary>Get SpellSource for a Staff Trigger</summary>
        public override SpellSource ActorSpellSource(CoreActor actor)
        {
            if (actor is Creature _critter)
            {
                var _spellType = SpellSource.SpellDef.SeedSpellDef.GetType();

                // get ItemCaster stats
                var _useLevel = SpellSource.CasterClass.ClassPowerLevel;
                var _difficulty = SpellSource.CasterClass.SpellDifficultyBase;

                // staff actor values
                if (UseDirectly(actor))
                {
                    var _iAct = new Interaction(actor, SpellSource, null, null);
                    // get appropriate caster class from actor (max caster level, rather than max difficulty base)
                    var _self = (from _c in _critter.Classes
                                 where typeof(ICasterClass).IsAssignableFrom(_c.GetType())
                                 let _castClass = _c as ICasterClass
                                 where (_castClass.MagicType == SpellSource.CasterClass.MagicType)              // arcane/divine match
                                 from _classSpell in _castClass.UsableSpells
                                 where _spellType.IsAssignableFrom(_classSpell.SpellDef.SeedSpellDef.GetType()) // has spell
                                 && (_castClass.SpellDifficultyAbility.MaxSpellLevel() >= _classSpell.Level)    // and can cast it
                                 orderby _castClass.ClassPowerLevel.QualifiedValue(_iAct)                       // NOTE: using max caster level
                                 select _castClass).FirstOrDefault();

                    // adjust caster level
                    if (_self.ClassPowerLevel.QualifiedValue(_iAct) > _useLevel.QualifiedValue(_iAct))
                        _useLevel = _self.ClassPowerLevel;

                    // get caster difficulty (this should drag spell focus along with it...)
                    _difficulty = _self.SpellDifficultyBase;
                }
                else
                {
                    // get base difficulty
                    _difficulty = new Deltable(10);
                    _difficulty.Deltas.Add(new SoftQualifiedDelta(_critter.ExtraSpellDifficulty));

                    // add ability
                    if (SpellSource.CasterClass.MagicType == MagicType.Divine)
                    {
                        // WISDOM
                        _difficulty.Deltas.Add(new SoftQualifiedDelta(_critter.Abilities.Wisdom));
                    }
                    else
                    {
                        // Charisma
                        if (SpellSource.SpellDef.ArcaneCharisma)
                        {
                            _difficulty.Deltas.Add(new SoftQualifiedDelta(_critter.Abilities.Charisma));
                        }
                        else
                        {
                            // or Intelligence if accessible via intelligence and intelligence is higher...
                            if (_critter.Abilities.Charisma.EffectiveValue > _critter.Abilities.Intelligence.EffectiveValue)
                                _difficulty.Deltas.Add(new SoftQualifiedDelta(_critter.Abilities.Charisma));
                            else
                                _difficulty.Deltas.Add(new SoftQualifiedDelta(_critter.Abilities.Intelligence));
                        }
                    }
                }

                // the item caster is the best caster level + the critter's difficulty + the critter's alignment
                var _staffCaster = new ItemCaster(SpellSource.CasterClass.MagicType, _useLevel, _critter.Alignment,
                    _difficulty, _critter.ID, SpellSource.CasterClass.CasterClassType);
                var _source = new SpellSource(_staffCaster, SpellSource.PowerLevel, SpellSource.SlotLevel, false, SpellSource.SpellDef);
                return _source;
            }
            else
            {
                // no change
                return SpellSource;
            }
        }
        #endregion
    }
}
