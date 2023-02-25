using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Creatures;
using Uzi.Ikosa.Interactions.Action;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Time
{
    [Serializable]
    public class CampMember : GroupMemberAdjunct, ITimelineActions, ICreatureBound
    {
        // TODO: action provider: rest, sleep, keep watch, break camp

        public CampMember(CampingGroup camping)
            : base(typeof(CampingGroup), camping)
        {
        }

        #region state
        #endregion

        public Creature Creature => Anchor as Creature;

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            Creature?.Actions.Providers.Add(this, this);
        }

        protected override void OnDeactivate(object source)
        {
            Creature.IkosaPosessions.EjectLocalUnstored(true);
            Creature?.Actions.Providers.Remove(this);
            base.OnDeactivate(source);
        }

        public override object Clone()
            => new CampMember(CampingGroup);

        public CampingGroup CampingGroup => Group as CampingGroup;

        public void EnteringTimeline()
        {
        }

        public void LeavingTimeline()
        {
        }

        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            if (budget is LocalActionBudget _budget)
            {
                if (!_budget.IsInitiative)
                {
                    // sleep or rest
                    // TODO: any constraints on ability to perform? or checks to avoid allowing when already doing?
                    if (_budget.Creature.HasActiveAdjunct<DailySleep>())
                    {
                        yield return new StartSleep(CampingGroup, new ActionTime(Minute.UnitFactor, Contracts.TimeType.TimelineScheduling), @"101");
                    }

                    // can rest instead of needing to sleep
                    yield return new StartRest(CampingGroup, new ActionTime(Minute.UnitFactor, Contracts.TimeType.TimelineScheduling), @"102");

                    // break camp
                    yield return new BreakCamp(CampingGroup, new ActionTime(Minute.UnitFactor, Contracts.TimeType.TimelineScheduling), @"103");

                    // keep watch
                    yield return new StartKeepWatch(new ActionTime(TimeType.Total), @"104");

                    // adjust gear

                    // trade amongst camp members
                    foreach (var _member in CampingGroup.CampMembers.Where(_m => _m.Creature != Creature))
                    {
                        // allow offer trade to each camping member
                        yield return new OfferTrade(CampingGroup, _member.Creature, @"200");
                    }

                    // share knowledge
                }

                // TODO: hastily break camp...while in initiative?
            }
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
            => new TimelineActionProviderInfo($@"Camping", ID);
    }
}
