using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;

namespace Uzi.Ikosa.Time
{
    [Serializable]
    public class DecideSetupCamp : ActionBase
    {
        public DecideSetupCamp(TeamGroup group, ActionTime actionTime, string orderKey)
            : base(group, actionTime, false, false, orderKey)
        {
        }

        public override string Key => @"Timeline.DecideSetupCamp";
        public override string DisplayName(CoreActor actor) => @"Decide Setup Camp";

        public TeamGroup TeamGroup => ActionSource as TeamGroup;

        protected override ActivityResponse OnCanPerformActivity(CoreActivity activity)
        {
            // if already deciding, cannot vote on another
            if (activity.Actor?.HasAdjunct<DecideCampMember>() ?? true)
            {
                return new ActivityResponse(false);
            }
            // if already in a camp, cannot vote on another
            if (activity.Actor?.HasAdjunct<CampMember>() ?? true)
            {
                return new ActivityResponse(false);
            }

            return base.OnCanPerformActivity(activity);
        }

        public override ActivityResponse CanPerformNow(CoreActionBudget budget)
        {
            // cannot being doing, or about to do something else
            var _budget = budget as LocalActionBudget;
            return new ActivityResponse(
                (!(_budget?.TurnTick.TurnTracker.IsInitiative ?? true))
                && (_budget?.HeldActivity == null)
                && (_budget?.NextActivity == null)
                && !(_budget.Actor?.HasAdjunct<DecideCampMember>() ?? false)
                && !(_budget.Actor?.HasAdjunct<CampMember>() ?? false)
                && base.CanPerformNow(budget).Success);
        }

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield break;
        }

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => ObservedActivityInfoFactory.CreateInfo(@"Deciding Setup Camp", activity.Actor, observer);

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;

        public override bool IsStackBase(CoreActivity activity)
            => false;

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            if (activity != null)
            {
                // get team
                if (activity.Actor is Creature _critter)
                {
                    // find or create a group that will decide for the team
                    var _map = _critter.Setting as LocalMap;
                    var _decision = _map.ContextSet.AdjunctGroups.All().OfType<DecideCampGroup>()
                        .FirstOrDefault(_dcg => _dcg.TeamGroup == TeamGroup);
                    if (_decision == null)
                    {
                        // initial proposal
                        _decision = new DecideCampGroup(TeamGroup);

                        // focus for the camp site is the actor's location
                        var _critterLocator = _critter.GetLocated()?.Locator;
                        var _campFocus = new CampSiteFocus(new GeometricSize(_critterLocator.GeometricRegion));
                        var _campLocator = new Locator(_campFocus, _critterLocator.MapContext,
                            _campFocus.GeometricSize, new Cubic(_critterLocator.Location, _campFocus.GeometricSize));
                        _campFocus.AddAdjunct(new DecideCampSite(_decision));
                    }

                    // critter is helping to decide
                    _critter.AddAdjunct(new DecideCampMember(_decision));
                    if (_decision.HasVoteSucceeded)
                    {
                        // add follow-on step to setup camp...
                        activity.EnqueueRegisterPreEmptively(Budget);
                        return new ConfirmedSetupCampStep(activity, _decision);
                    }
                }
            }
            return new RegisterActivityStep(activity, Budget);
        }
    }
}
