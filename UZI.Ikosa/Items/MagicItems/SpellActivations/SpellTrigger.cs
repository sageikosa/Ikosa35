using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Skills;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items
{
    /// <summary>
    /// Often used in wands.  [Adjunct, IActionProvider, IProtectable]
    /// </summary>
    [Serializable]
    public class SpellTrigger : SpellActivation, IProtectable, IIdentification
    {
        #region Construction
        public SpellTrigger(SpellSource source, IPowerBattery battery, int chargePerUse)
            : base(source, battery, new SpellActivationCost(), true, null)
        {
            _ChargePerUse = chargePerUse;
        }
        #endregion

        #region data
        private int _ChargePerUse;
        #endregion

        public IPowerBattery PowerBattery => PowerCapacity as IPowerBattery;
        public int ChargePerUse => _ChargePerUse;

        #region public override IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        public override IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            if (IsActive && PowerBattery.CanUseCharges(ChargePerUse))
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
                        foreach (var _mode in SpellSource.SpellDef.SpellModes)
                        {
                            yield return new TriggerSpell(budget.Actor, this, _mode, new ActionTime(TimeType.Regular), $@"{_mx:0#}");
                            _mx++;
                        }
                    }
                }
            }
            yield break;
        }
        #endregion

        public override Info GetProviderInfo(CoreActionBudget budget)
            => new AdjunctInfo(@"Spell Trigger", ID);

        #region public bool UseDirectly(CoreActor actor)
        /// <summary>Indicates the creature can trigger the magic without a use magic device check</summary>
        /// <param name="actor"></param>
        public virtual bool UseDirectly(CoreActor actor)
        {
            if (actor is Creature _critter)
            {
                var _spellType = SpellSource.SpellDef.SeedSpellDef.GetType();
                return (from _c in _critter.Classes
                        where typeof(ICasterClass).IsAssignableFrom(_c.GetType())
                        let _castClass = _c as ICasterClass
                        from _classSpell in _castClass.UsableSpells
                        where _spellType.IsAssignableFrom(_classSpell.SpellDef.SeedSpellDef.GetType())
                        select _classSpell).FirstOrDefault() != null;
            }
            return false;
        }
        #endregion

        public bool IsExposedTo(Creature critter)
            => this.HasExposureTo(critter);

        /// <summary>Effective SpellSource for an activity</summary>
        public virtual SpellSource ActorSpellSource(CoreActor actor)
            // wand-like triggers just use the source as provided
            => SpellSource;

        public override object Clone()
            => new SpellTrigger(SpellSource, PowerBattery, ChargePerUse);

        public override decimal LevelUnitPrice
            => 15m;

        #region IIdentification Members

        public IEnumerable<Info> IdentificationInfos
        {
            get
            {
                yield return new SpellTriggerInfo
                {
                    SpellSource = SpellSource.ToSpellSourceInfo(),
                    ChargesPerUse = ChargePerUse
                };
                yield break;
            }
        }

        #endregion
    }
}