using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Abilities;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Skills
{
    [Serializable, SkillInfo("Search", MnemonicCode.Int)]
    public class SearchSkill : SkillBase, IActionProvider
    {
        public SearchSkill(Creature skillUser)
            : base(skillUser)
        {
        }

        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            // Only find if within 10 feet of searchable...
            // ... object aim (<=10'), cell aim (+1 cell unit + fuzz), or implicit same cell (+ fuzz)...
            // object...searchables on connected, such as traps on items or items in horde/chest
            // cell...directly located that are searchable (such as traps/secret doors/footprints)

            var _budget = budget as LocalActionBudget;
            if (_budget.CanPerformTotal)
            {
                yield return new SearchArea(Creature, new ActionTime(TimeType.Total), @"101");
                yield return new SearchObject(Creature, new ActionTime(TimeType.Total), @"102");
            }

            // TODO: find item in horde or chest (Difficulty = 10; total)

            // TODO: auto-search fail/retry?
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
    }
}
