using System;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Skills;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class ClimbCheckStep : MovementProcessStep
    {
        #region construction
        public ClimbCheckStep(CoreActivity activity, Creature climber, Deltable difficulty, ClimbMovement climb)
            : base(activity)
        {
            _Climber = climber;
            _Climb = climb;
            if ((_Climber != null) && (_Climb != null))
            {
                var _check = new SuccessCheck(climber.Skills.Skill<ClimbSkill>(), difficulty.EffectiveValue,
                    activity, -5);
                _PreReq = new SuccessCheckPrerequisite(Activity, new Qualifier(climber, Activity, climber),
                    @"ClimbCheck", @"Climb Check", _check, false);

                // must use accelerated?
                if (_Climb.IsAccelerated)
                    _PreReq.IsUsingPenalty = true;

                if (!_Climb.IsCheckRequired)
                {
                    // force a success despite any penalties
                    _PreReq.CheckRoll = new Deltable(100);
                }
            }
        }
        #endregion

        #region data
        private Creature _Climber;
        private SuccessCheckPrerequisite _PreReq;
        private ClimbMovement _Climb;
        #endregion

        public override string Name => @"Climb Check";
        public Creature Climber => _Climber;

        protected override StepPrerequisite OnNextPrerequisite()
            => IsDispensingPrerequisites ? _PreReq : null;

        public override bool IsDispensingPrerequisites
            => DispensedPrerequisites.Count() == 0;

        public Locator ClimberLocator
            => Activity.Targets.OfType<ValueTarget<MovementLocatorTarget>>()
            .Select(_mlt => _mlt.Value.Locator)
            .FirstOrDefault(_loc => _loc?.Chief == _Climber);

        protected override bool OnDoStep()
        {
            if (_PreReq.IsReady)
            {
                void _doClimb()
                {
                    foreach (var _clmb in Activity.Actor.Adjuncts.OfType<Climbing>().ToList())
                    {
                        _clmb.Eject();
                    }
                    Activity.Actor.AddAdjunct(new Climbing(_PreReq.ToTarget()));
                };

                // TODO: charging, accelerated and climbing
                if (_Climb.IsAccelerated && !_PreReq.IsUsingPenalty)
                {
                    // no longer charging/running
                    _doClimb();
                    AppendFollowing(Activity.GetActivityResultNotifyStep(@"No longer charging"));
                    AppendFollowing(new MoveCostCheckStep(Activity));
                }
                else if (_PreReq.Success)
                {
                    // succeeded on climb
                    _doClimb();
                    AppendFollowing(Activity.GetActivityResultNotifyStep(@"Succeeded on climb check"));
                    AppendFollowing(new MoveCostCheckStep(Activity));
                }
                else if (_PreReq.SoftFail(4))
                {
                    // movement into this space will be the last
                    AppendFollowing(Activity.GetActivityResultNotifyStep(@"Failed by 4 or less"));
                    Activity.AppendCompletion(
                        new CanStillMoveStep(Activity, MovementAction.MovementBudget));
                }
                else
                {
                    // falling
                    AppendFollowing(Activity.GetActivityResultNotifyStep(@"Failure"));
                    Activity.AppendCompletion(
                        new FallingStartStep(Activity, ClimberLocator, 500, 0, 1));
                }
                return true;
            }

            // will finish this movement
            return false;
        }
    }
}
