using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Time
{
    [Serializable]
    public class IdlingActor : GroupMemberAdjunct, ITimelineActions, ICreatureBound
    {
        public IdlingActor(IdlingGroup idling)
            : base(typeof(IdlingGroup), idling)
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
            => new IdlingActor(IdlingGroup);

        public IdlingGroup IdlingGroup => Group as IdlingGroup;

        public void EnteringTimeline()
        {
            // TODO: on every time tick, set next activity to Idling
        }

        public void LeavingTimeline()
        {
            // TODO: stop tracking time ticks...
        }

        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
            => new TimelineActionProviderInfo($@"Idling", ID);
    }
}
