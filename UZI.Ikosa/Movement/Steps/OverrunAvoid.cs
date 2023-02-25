using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Actions;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class OverrunAvoid : PreReqListStepBase
    {
        public OverrunAvoid(CoreActivity activity, Creature target, bool isAware)
            : base(activity)
        {
            var _ovrRun = activity?.Action as Overrun;
            var _critter = activity?.Actor as Creature;
            if ((_critter != null) && (_ovrRun != null))
            {
                _PendingPreRequisites.Enqueue(new ChoicePrerequisite(this, activity.Actor, this, target,
                    @"Overrun.Avoid", @"Avoid or Resist Overrun...", DecideToAvoid(), true));
            }
        }

        public CoreActivity Activity
            => Process as CoreActivity;

        public MovementAction MovementAction
            => Activity.Action as MovementAction;

        #region private IEnumerable<OptionAimOption> DecideToAvoid()
        private IEnumerable<OptionAimOption> DecideToAvoid()
        {
            yield return new OptionAimValue<bool>
            {
                Key = @"True",
                Description = @"Avoid",
                Name = @"Avoid",
                Value = true
            };
            yield return new OptionAimValue<bool>
            {
                Key = @"False",
                Description = @"Resist",
                Name = @"Resist",
                Value = false
            };
            yield break;
        }
        #endregion

        protected override bool OnDoStep()
        {
            var _overrunning = MovementAction?.Budget?.BudgetItems[typeof(OverrunningBudget)] as OverrunningBudget;
            if (_overrunning != null)
            {
                // if block choice made, go to challenge step 1 (like trip, sorta...)
                var _choice = DispensedPrerequisites?.OfType<ChoicePrerequisite>()
                    .FirstOrDefault(_p => _p.BindKey.Equals(@"Overrun.Avoid"));
                if (!(_choice?.Selected as OptionAimValue<bool>)?.Value ?? false)
                {
                    // chose not to avoid...
                    AppendFollowing(new OverrunBlock(Activity, _choice.Fulfiller as Creature));
                }
            }
            return true;
        }
    }
}
