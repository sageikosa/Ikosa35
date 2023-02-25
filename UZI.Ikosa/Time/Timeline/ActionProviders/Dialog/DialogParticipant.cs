using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Time
{
    [Serializable]
    public class DialogParticipant : GroupMemberAdjunct, ITimelineActions, ICreatureBound
    {
        public DialogParticipant(DialogGroup dialog)
            : base(typeof(DialogGroup), dialog)
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
            => new DialogParticipant(DialogGroup);

        public DialogGroup DialogGroup => Group as DialogGroup;

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
            => new TimelineActionProviderInfo($@"Conversing", ID);
    }
}
