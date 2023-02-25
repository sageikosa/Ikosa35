using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class Pinner : GroupMasterAdjunct, IActionProvider, IActionFilter
    {
        public Pinner(PinGroup group)
            : base(typeof(PinGroup), group)
        {
        }

        public PinGroup PinGroup => Group as PinGroup;

        public override object Clone()
            => new Pinner(PinGroup);

        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            // TODO: PINNING: damage opponent, use opponent's weapon, move grapple (+4); optional stop pinned from speaking
            // TODO: PINNING: disarm opponent, opponent gets +4 opposed
            // TODO: PINNING: release, not grappling character (free-action)
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
        {
            throw new NotImplementedException();
        }

        public bool SuppressAction(object source, CoreActionBudget budget, CoreAction action)
        {
            // TODO: PINNING: no draw, or use own weapon
            // TODO: PINNING: no escape grapple via check
            // TODO: PINNING: no retrieve spell component
            // TODO: PINNING: no pinning other characters
            // TODO: PINNING: no break pin of third-party
            return false;
        }
    }
}
