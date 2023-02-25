using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Time;
using Uzi.Ikosa.Magic.SpellLists;

namespace Uzi.Ikosa.Advancement
{
    [Serializable]
    public abstract class PreparedCaster : SlottedCasterClass<PreparedSpellSlot>, IActionProvider, IPreparedCasterClass
    {
        protected PreparedCaster(byte powerDie)
            : base(powerDie)
        {
        }

        protected PreparedCaster(byte powerDie, PowerDieCalcMethod calcMethod)
            : base(powerDie, calcMethod)
        {
        }

        protected override void OnAdd()
        {
            Creature.Actions.Providers.Add(this, (IActionProvider)this);
            Creature.AddAdjunct(new PreparedCasterReactions(new ReactorStepCompleteGroup(this), this));
            base.OnAdd();
        }

        protected override void OnRemove()
        {
            base.OnRemove();
            Creature.Adjuncts
                .OfType<PreparedCasterReactions>()
                .FirstOrDefault(_r => _r.PreparedCaster == this)?
                .Eject();
            Creature.Actions.Providers.Remove(this);
        }

        #region IActionProvider Members
        protected IEnumerable<CoreAction> BaseActions(CoreActionBudget budget)
        {
            if (IsPowerClassActive)
            {
                if (budget is LocalActionBudget _budget)
                {
                    // cast spells from slots
                    foreach (var _spellSlot in AllSpellSlots.Where(_slot => _slot.IsCharged && (_slot.PreparedSpell!=null)))
                    {
                        // only create casting actions for those we know we can perform
                        if (_budget.HasTime(_spellSlot.PreparedSpell.SpellDef.ActionTime))
                        {
                            // some spells have multiple modes, so include them all
                            foreach (var _spellMode in _spellSlot.PreparedSpell.SpellDef.SpellModes)
                                yield return new CastSpell(_spellSlot.PreparedSpell, _spellMode,
                                    _spellSlot.PreparedSpell.SpellDef.ActionTime, _spellSlot,
                                    _spellSlot.PreparedSpell.SpellDef.DisplayName);
                        }
                    }
                }

                var _time = Creature?.GetCurrentTime() ?? 0d;
                if (!MustRestToRecharge)
                {
                    if (AllSpellSlots.Any(_s => _s.CanRecharge(_time)))
                    {
                        // has uncharged slots that can be recharged 
                        yield return new PrepareSpells(Creature, this);
                    }
                    else if (AllSpellSlots.Any(_s => _s.IsCharged && (_s.PreparedSpell == null)))
                    {
                        // has charged slots that do not have spells in them
                        yield return new PrepareMoreSpells(Creature, this);
                    }
                }
                else
                {
                    if (AllSpellSlots.Any(_s => _s.IsCharged && (_s.PreparedSpell == null)))
                    {
                        // has charged slots that do not have spells in them
                        yield return new PrepareMoreSpells(Creature, this);
                    }
                }
            }
            yield break;
        }

        public virtual IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            foreach (var _act in BaseActions(budget))
                yield return _act;
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
            => ToPowerClassInfo();
        #endregion

        // IPreparedCasterClass Members
        public abstract IEnumerable<Magic.SpellLists.ClassSpell> PreparableSpells(int setIndex);

        public override IEnumerable<ClassSpell> SlottableSpells(int setIndex)
            => PreparableSpells(setIndex);
    }
}
