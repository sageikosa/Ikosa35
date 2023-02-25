using System;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Skills;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class HopUpCheckStep : MovementProcessStep
    {
        #region construction
        public HopUpCheckStep(CoreActivity activity, Deltable difficulty)
            : base(activity)
        {
            var _critter = activity.Actor as Creature;
            var _check = new SuccessCheck(_critter.Skills.Skill<JumpSkill>(), difficulty.EffectiveValue,
                activity, -5);
            _PreReq = new SuccessCheckPrerequisite(Activity, new Qualifier(_critter, Activity, _critter),
                @"JumpCheck", @"Jump Check", _check, false);
        }
        #endregion

        #region private data
        private SuccessCheckPrerequisite _PreReq;
        #endregion

        public override string Name => @"Jump Check";

        protected override StepPrerequisite OnNextPrerequisite()
            => IsDispensingPrerequisites ? _PreReq : null;

        // haven't given anything
        public override bool IsDispensingPrerequisites
            => DispensedPrerequisites.Count() == 0;

        protected override bool OnDoStep()
        {
            if (_PreReq.IsReady)
            {
                if (_PreReq.Success)
                {
                    // succeeded on jump
                    if ((MovementAction as HopUp).Validate(Activity))
                    {
                        AppendFollowing(Activity.GetActivityResultNotifyStep(@"Jump succeeded"));
                        AppendFollowing(MovementAction.Movement.CostFactorStep(Activity));
                    }
                    else
                    {
                        AppendFollowing(Activity.GetActivityResultNotifyStep(@"Invalid direction provided"));
                    }
                }
                else
                {
                    // fail to jump
                    AppendFollowing(
                        Activity.GetActivityResultNotifyStep(@"Jump failed"),
                        new CanStillMoveStep(Activity, MovementAction.MovementBudget));
                }
                return true;
            }

            // will finish this movement
            return false;
        }
    }
}
