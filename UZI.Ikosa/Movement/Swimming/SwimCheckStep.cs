using System;
using System.Diagnostics;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Skills;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class SwimCheckStep : MovementProcessStep
    {
        #region construction
        public SwimCheckStep(CoreActivity activity, Creature swimmer, Deltable difficulty)
            : base(activity)
        {
            _Swimmer = swimmer;
            if (_Swimmer != null)
            {
                var _check = new SuccessCheck(swimmer.Skills.Skill<SwimSkill>(), difficulty.EffectiveValue,
                    activity, -5);
                _PreReq = new SuccessCheckPrerequisite(Activity, new Qualifier(swimmer, Activity, swimmer),
                    @"SwimCheck", @"Swim Check", _check, false);
            }
        }
        #endregion

        #region data
        private Creature _Swimmer;
        private SuccessCheckPrerequisite _PreReq;
        #endregion

        public override string Name => @"Swim Check";
        public Creature Swimmer => _Swimmer;

        protected override StepPrerequisite OnNextPrerequisite()
            => IsDispensingPrerequisites
            ? _PreReq
            : null;

        public override bool IsDispensingPrerequisites
            => DispensedPrerequisites.Count() == 0;

        public Locator SwimmerLocator
            => Activity.Targets.OfType<ValueTarget<MovementLocatorTarget>>()
            .Select(_mlt => _mlt.Value.Locator)
            .FirstOrDefault(_loc => _loc?.Chief == _Swimmer);

        #region protected override bool OnDoStep()
        protected override bool OnDoStep()
        {
            var _critter = Activity.Actor as Creature;
            if (_PreReq.IsReady)
            {
                void _swimResult(bool fail)
                {
                    foreach (var _swm in _critter.Adjuncts.OfType<Swimming>().ToList())
                    {
                        _swm.Eject();
                    }
                    if (!fail)
                    {
                        _critter.AddAdjunct(new Swimming(_PreReq.ToTarget()));
                    }
                }
                ;

                // TODO: charging and swimming
                if (_PreReq.Success)
                {
                    // succeeded on swim
                    _swimResult(false);
                    AppendFollowing(Activity.GetActivityResultNotifyStep(@"Succeeded on swim check"));
                    AppendFollowing(new MoveCostCheckStep(Activity));
                }
                else if (_PreReq.SoftFail(4))
                {
                    // movement into this space will be the last
                    _swimResult(false);
                    AppendFollowing(Activity.GetActivityResultNotifyStep(@"Failed by 4 or less"));
                    Activity.AppendCompletion(
                        new CanStillMoveStep(Activity, MovementAction.MovementBudget));
                }
                else
                {
                    // failing: no longer considered swimming, so submergence possible
                    _swimResult(true);
                    AppendFollowing(Activity.GetActivityResultNotifyStep(@"Failure"));

                    // get a second auto-check at the soft-fail level; fail that and sink
                    if (_critter.Skills.Skill<SwimSkill>().AutoCheck(_PreReq.Check.Difficulty - 5, null))
                    {
                        // stop movement
                        Activity.AppendCompletion(
                            new CanStillMoveStep(Activity, MovementAction.MovementBudget));
                    }
                    else
                    {
                        // sinking feeling
                        Activity.AppendCompletion(
                            new SinkingStartStep(Activity, SwimmerLocator, 5));
                    }
                }
                return true;
            }

            // will finish this movement
            return false;
        }
        #endregion
    }
}
