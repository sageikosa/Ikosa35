using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using Uzi.Ikosa.Magic;
using Uzi.Core;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Advancement;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    FeatInfo(@"Heighten Spell"),
    CasterLevelRequirement(1)
    ]
    public class HeightenSpellFeat : MetamagicFeatBase, IActionProvider
    {
        public HeightenSpellFeat(object source, int powerLevel)
            : base(source, powerLevel, 0)
        {
        }

        public Guid ID => CoreID;

        public static string StaticMetaTag => @"Heightened";
        public static string StaticMetaBenefit => @"Spell has higher spell level than normal (up to a maximum of 9th level).  Save difficulties increase and spell-resistance base increase.";

        public override string Benefit
            => DecorateWithLevelInfo(StaticMetaBenefit);

        public override string MetaMagicTag => StaticMetaTag;
        public override string MetaMagicBenefit => StaticMetaBenefit;

        public override ISpellDef ApplyMetamagic(ISpellDef spellDef, bool isSpontaneous)
        {
            // NOTE: heighten doesn't need to apply anything directly
            return spellDef;
        }

        #region IActionProvider Members
        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            // cast heightened spell (spontaneous casters)
            if (Creature != null)
            {
                var _budget = budget as LocalActionBudget;
                // TODO: budget...

                // TODO: heighten doesn't have a fixed slot adjustment...
            }
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
            => ToFeatInfo();
        #endregion
    }
}
