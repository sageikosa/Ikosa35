using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Time
{
    [Serializable]
    public class Lodger : GroupMemberAdjunct, ITimelineActions, ICreatureBound
    {
        public Lodger(LodgingGroup lodging)
            : base(typeof(LodgingGroup), lodging)
        {
        }

        public Creature Creature => Anchor as Creature;

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            Creature?.Actions.Providers.Add(this, this);
        }

        protected override void OnDeactivate(object source)
        {
            Creature?.Actions.Providers.Remove(this);
            base.OnDeactivate(source);
        }

        public override object Clone()
            => new Lodger(LodgingGroup);

        public LodgingGroup LodgingGroup => Group as LodgingGroup;

        public void EnteringTimeline()
        {
        }

        public void LeavingTimeline()
        {
        }

        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
            => new TimelineActionProviderInfo($@"Lodging", ID);
    }
}
