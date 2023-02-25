using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using Uzi.Ikosa.Magic;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Advancement;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    FeatInfo(@"Silent Spell"),
    CasterLevelRequirement(1)
    ]
    public class SilentSpellFeat : MetamagicFeatBase, IActionProvider
    {
        public SilentSpellFeat(object source, int powerLevel)
            : base(source, powerLevel, 1)
        {
        }

        public Guid ID => CoreID;

        public static string StaticMetaTag => @"Silent";
        public static string StaticMetaBenefit => @"Verbal components are not required for the spell to be cast.  Spells without verbal components are not affected.";

        public override string Benefit
            => DecorateWithLevelInfo(StaticMetaBenefit);

        public override string MetaMagicTag => StaticMetaTag;
        public override string MetaMagicBenefit => StaticMetaBenefit;

        public override ISpellDef ApplyMetamagic(ISpellDef spellDef, bool isSpontaneous)
            => new SilentSpellDef(spellDef, PresenterID, isSpontaneous);

        #region IActionProvider Members

        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            if (Creature != null)
            {
                var _budget = budget as LocalActionBudget;
                if (_budget.CanPerformTotal)
                {
                    foreach (var _info in SpontaneousInfo<SilentSpellDef>(_budget))
                    {
                        // still the casting
                        var _spellDef = ApplyMetamagic(_info.SpellSource.SpellDef, true);
                        var _quietSource = new SpellSource(_info.SpellSource.CasterClass, _info.SpellSource.PowerLevel,
                            _info.SpellSlot.SlotLevel, true, _spellDef);
                        yield return new CastSpell(_quietSource, _info.SpellMode, 
                            _spellDef.ActionTime, _info.SpellSlot, _spellDef.DisplayName);
                    }
                }
            }
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
            => ToFeatInfo();

        #endregion
    }
}
