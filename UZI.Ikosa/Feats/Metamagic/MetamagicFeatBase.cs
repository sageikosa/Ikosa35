using System;
using System.Collections.Generic;
using Uzi.Ikosa.Magic;
using System.Linq;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Feats
{
    [Serializable]
    public abstract class MetamagicFeatBase : FeatBase
    {
        protected MetamagicFeatBase(object source, int powerLevel, int slotAdjustment)
            : base(source, powerLevel)
        {
            SlotAdjustment = slotAdjustment;
        }

        #region protected IEnumerable<SpontaneousMetaMagicInfo> SpontaneousInfo<Exclude>(LocalActionBudget budget) where Exclude : MetaMagicSpellDef
        /// <summary>
        /// Provides information needed to generate actions for spontaneous caster use of metamagic feat
        /// </summary>
        protected IEnumerable<SpontaneousMetaMagicInfo> SpontaneousInfo<Exclude>(LocalActionBudget budget)
            where Exclude : MetaMagicSpellDef
        {
            // loop through all actions providers (except for this one)
            // get all CastSpell actions that are from a spontaneous casting source
            foreach (var _spellCast in from _prov in Creature.Actions.GetActionProviders()
                                       where _prov != this
                                       from _cast in _prov.GetActions(budget).OfType<CastSpell>()
                                       where _cast.PowerActionSource.IsSpontaneous
                                       select _cast)
            {
                // check to see if the spell definition is not already meta-magicked
                // ... or, if it is, check that it isn't already wrapped in this meta-magic type
                if ((!(_spellCast.PowerActionSource.SpellDef is MetaMagicSpellDef _mmSpellDef)) || !_mmSpellDef.HasWrapper<Exclude>())
                {
                    // see if caster class has slot availability to meet adjusted slot requirements ...
                    var _minSlotLevel = _spellCast.PowerActionSource.SlotLevel + SlotAdjustment;
                    var _caster = Creature.Classes.OfType<SpontaneousCaster>()
                        .FirstOrDefault(_c => _spellCast.PowerActionSource.CasterClass.CasterClassType.Equals(_c.GetType()));
                    if (_caster != null)
                    {
                        // ... by checking for a charged slot at the minimum adjusted level
                        var _available = (from _lvl in _caster.GetSpellSlotSets(0).AllLevels
                                          where _lvl.Level >= _minSlotLevel
                                          from _slot in _lvl.Slots
                                          where _slot.IsCharged
                                          orderby _slot.SlotLevel
                                          select new SpontaneousMetaMagicInfo
                                          {
                                              SpellSource = _spellCast.PowerActionSource,
                                              SpellMode = _spellCast.SpellMode,
                                              SpellSlot = _slot
                                          }).FirstOrDefault();
                        if (_available != null)
                        {
                            yield return _available;
                        }
                    }
                }
            }
            yield break;
        }
        #endregion

        public int SlotAdjustment { get; protected set; }

        /// <summary>Simple metamagic description</summary>
        public abstract string MetaMagicTag { get; }

        /// <summary>Benefit without slot adjustment</summary>
        public abstract string MetaMagicBenefit { get; }

        /// <summary>
        /// Define to provide a metamagic wrapper on a spell.  Used when preparing a spell slot, and when casting spontaneously
        /// </summary>
        public abstract ISpellDef ApplyMetamagic(ISpellDef spellDef, bool isSpontaneous);

        protected string DecorateWithLevelInfo(string src)
            => $@"{src} (needs spell slot-level +{SlotAdjustment})";

        public override bool MeetsPreRequisite(Creature creature)
            => !creature.Feats.Contains(GetType());
    }

    public class SpontaneousMetaMagicInfo
    {
        /// <summary>Original source for spell</summary>
        public SpellSource SpellSource { get; set; }
        /// <summary>Spell slot needed to cast the spell</summary>
        public SpellSlot SpellSlot { get; set; }
        /// <summary>Original mode for spell</summary>
        public ISpellMode SpellMode { get; set; }
    }
}
