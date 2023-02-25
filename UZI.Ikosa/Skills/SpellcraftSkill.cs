using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Skills
{
    [Serializable, SkillInfo(@"Spellcraft", @"INT", false, 0d)]
    public class SpellcraftSkill : SkillBase, IActionProvider
    {
        public SpellcraftSkill(Creature skillUser)
            : base(skillUser)
        {
            skillUser.Actions.Providers.Add(this, this);
        }

        #region IActionProvider Members
        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            if (IsTrained)
            {
                var _budget = budget as LocalActionBudget;
                // TODO: budget...

                /* NOTE: make these automatic with no action needed?

                // TODO: identify spell being cast (15 + spell level), no action, no retry
                // NOTE: spellcraft skill will need ICanReactByStep to allow choices and spellcraft actions

                // TODO: identify spell targetted at you after saving throw, no action, no retry
                // NOTE: spellcraft skill will need ICanReactByStep? to allow choices and spellcraft actions
                // NOTE: perhaps have one spellcraft group adjunct, and attach every skilled actor to it?

                */

                // TODO: learn spell, 8 hours, no retry until +1 rank
                // TODO: identify spell in place, no action, no retry
                // TODO: decipher without read magic, full-round, one attempt/day

                // TODO: identify a potion, 1 minute, no retry
            }
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
        {
            var _info = ToInfo<SkillInfo>(null);
            _info.Message = SkillName;
            _info.IsClassSkill = IsClassSkill;
            _info.KeyAbilityMnemonic = KeyAbilityMnemonic;
            _info.UseUntrained = UseUntrained;
            _info.IsTrained = IsTrained;
            _info.CheckFactor = CheckFactor;
            return _info;
        }
        #endregion
    }
}
