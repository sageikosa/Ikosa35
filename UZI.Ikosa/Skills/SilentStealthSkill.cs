using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Abilities;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Skills
{
    /// <summary>DEX; untrained; check</summary>
    [Serializable, SkillInfo(@"Silent Stealth", MnemonicCode.Dex, true, 1d)]
    public class SilentStealthSkill : SkillBase, IActionProvider
    {
        public SilentStealthSkill(Creature skillUser)
            : base(skillUser)
        {
        }

        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            yield return new SilentStealthChoice(Creature);
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
