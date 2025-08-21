using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Items.Weapons.Ranged;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    ParameterizedFeatInfo(@"Rapid Reload", @"Improves reload time", typeof(ProficientDerivedWeaponsLister<CrossbowBase>)),
    FighterBonusFeat
    ]
    public class RapidReloadFeat<CBow> : FeatBase, IActionProvider where CBow : CrossbowBase
    {
        public RapidReloadFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }

        public Guid ID => CoreID;
        public override string Name => $@"Rapid Reload for {typeof(CBow).Name}";
        public override string PreRequisite => $@"Proficiency with {typeof(CBow).Name}";
        public override string Benefit => $@"Improves reload time of {typeof(CBow).Name}";

        public override bool MeetsPreRequisite(Creature creature)
        {
            if (IgnorePreRequisite)
            {
                return true;
            }

            return (!creature.Feats.Contains(GetType()) &&
                (creature.Proficiencies.IsProficientWith<CBow>(PowerLevel)));
        }

        // TODO:
        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget) { yield break; }

        public Info GetProviderInfo(CoreActionBudget budget)
            => ToFeatInfo();
    }
}
