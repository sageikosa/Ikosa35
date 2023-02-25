using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Magic.Spells
{
    /// <summary>GroupMemberAdjunct adde to caster controlling the spiritual weapon</summary>
    [Serializable]
    public class SpiritualWeaponController : GroupMemberAdjunct, IActionProvider
    {
        /// <summary>GroupMemberAdjunct adde to caster controlling the spiritual weapon</summary>
        public SpiritualWeaponController(SpellSource source, SpiritualWeaponGroup group)
            : base(source, group)
        {
        }

        protected override void OnActivate(object source)
        {
            (Anchor as Creature)?.Actions.Providers.Add(this, this);
            base.OnActivate(source);
        }

        protected override void OnDeactivate(object source)
        {
            (Anchor as Creature)?.Actions.Providers.Remove(this);
            base.OnDeactivate(source);
        }

        public SpiritualWeaponGroup SpiritualWeaponGroup => Group as SpiritualWeaponGroup;
        public SpellSource SpellSource => Source as SpellSource;

        public override object Clone()
            => new SpiritualWeaponController(SpellSource, SpiritualWeaponGroup);

        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            if (budget is LocalActionBudget _budget)
            {
                if (_budget.CanPerformBrief)
                {
                    yield return new SpiritualWeaponRedirect(SpiritualWeaponGroup, new ActionTime(TimeType.Brief), @"200");
                }
                if (SpiritualWeaponGroup.Weapon.AttackNow <= (_budget.Creature.CurrentTime ?? 0))
                {
                    yield return new SpiritualWeaponAttackNow(SpiritualWeaponGroup, @"210"); 
                }
            }
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
            => new AdjunctInfo($@"Spiritual Weapon Control", ID);
    }
}
