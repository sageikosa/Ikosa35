using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class Pinnee : GroupMemberAdjunct, IActionProvider, IActionFilter
    {
        public Pinnee(PinGroup group)
            : base(typeof(PinGroup), group)
        {
        }

        public PinGroup PinGroup => Group as PinGroup;

        public override object Clone()
            => new Pinnee(PinGroup);

        protected override void OnActivate(object source)
        {
            if (Anchor is Creature _critter)
            {
                _critter.Conditions.Add(new Condition(Condition.Pinned, this));
            }
            base.OnActivate(source);
        }

        protected override void OnDeactivate(object source)
        {
            if (Anchor is Creature _critter)
            {
                _critter.Conditions.Remove(_critter.Conditions[Condition.Pinned, this]);
            }
            base.OnDeactivate(source);
        }

        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            // TODO: PINNED: opposed grapple to break pin (but not grapple)
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
        {
            throw new NotImplementedException();
        }

        public bool SuppressAction(object source, CoreActionBudget budget, CoreAction action)
        {
            // TODO: suppress most grapple sourced actions (must break pin first)
            return false;
        }
    }
}
