using System;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Universal;

namespace Uzi.Ikosa.Actions
{
    public static class ActivityInfoConverter
    {
        /// <summary>Used only to display actions in LocalActionBudgetInfo</summary>
        public static ActivityInfo ToActivityInfo(this CoreActivity self)
        {
            var _info = new ActivityInfo()
            {
                ActorID = self.Actor.ID,
                ActionID = self.Action.ActionSource.ID,
                ActionKey = self.Action.Key,
                DisplayName = self.Action.DisplayName(self.Actor),
                Description = self.Action.Description,
                Targets = self.Targets
                .Select(_at => _at.GetTargetInfo())
                .Where(_at => _at != null).ToArray()
            };
            return _info;
        }

        /// <summary>Gets CoreActor for the activity</summary>
        public static CoreActor GetActor(this ActivityInfo self)
            => IkosaStatics.InteractProvider.GetIInteract(self.ActorID) as CoreActor;

        /// <summary>Gets action for the activity</summary>
        public static ActionBase GetAction(this ActivityInfo self)
        {
            var _actor = self.GetActor();
            if (_actor != null)
            {
                if (IkosaStatics.ProcessManager.LocalTurnTracker?.GetBudget(_actor.ID) is LocalActionBudget _budget)
                    return (from _act in _budget.GetActions()
                            where _act.Action.ActionSource.ID == self.ActionID
                            && _act.Action.Key.Equals(self.ActionKey, StringComparison.OrdinalIgnoreCase)
                            select _act.Action).OfType<ActionBase>().FirstOrDefault();
            }
            return null;
        }

        public static CoreActivity CreateActivity(this ActivityInfo self)
            => self.GetAction()?.GetActivity(self.GetActor(), self.Targets);

        public static CoreActivity CreateOpportunisticActivity(this ActivityInfo self, OpportunisticPrerequisite prerequisite)
            => (from _atk in prerequisite.Attacks
                where _atk.action.ActionSource.ID == self.ActionID
                && _atk.action.Key.Equals(self.ActionKey, StringComparison.OrdinalIgnoreCase)
                select _atk.action)
            .FirstOrDefault()?.GetActivity(self.GetActor(), self.Targets);

        public static CoreActivity CreateReactiveActivity(this ActivityInfo self, ReactivePrerequisite prerequisite)
            => (from _react in prerequisite.Actions
                where _react.action.ActionSource.ID == self.ActionID
                && _react.action.Key.Equals(self.ActionKey, StringComparison.OrdinalIgnoreCase)
                select _react.action)
            .FirstOrDefault()?.GetActivity(self.GetActor(), self.Targets);
    }
}
