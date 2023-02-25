using System;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Skills;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class BalanceCheckStep : MovementProcessStep
    {
        #region construction
        public BalanceCheckStep(CoreActivity activity, Creature balancer, Deltable difficulty, bool accelerated)
            : base(activity)
        {
            _Balancer = balancer;
            var _check = new SuccessCheck(balancer.Skills.Skill<BalanceSkill>(), difficulty.EffectiveValue,
                activity, -5);
            _PreReq = new SuccessCheckPrerequisite(Activity, new Qualifier(balancer, Activity, balancer),
                @"BalanceCheck", @"Balance Check", _check, false);

            // must use accelerated?
            _Accelerated = accelerated;
            if (_Accelerated)
                _PreReq.IsUsingPenalty = true;
        }
        #endregion

        #region data
        private Creature _Balancer;
        private SuccessCheckPrerequisite _PreReq;
        private bool _Accelerated;
        #endregion

        public override string Name => @"Balance Check";
        public Creature Balancer => _Balancer;

        protected override StepPrerequisite OnNextPrerequisite()
            => IsDispensingPrerequisites ? _PreReq : null;

        public override bool IsDispensingPrerequisites
            => DispensedPrerequisites.Count() == 0;

        public Locator BalancerLocator
            => Activity.Targets.OfType<ValueTarget<MovementLocatorTarget>>()
            .Select(_mlt => _mlt.Value.Locator)
            .FirstOrDefault(_loc => _loc?.Chief == _Balancer);

        protected override bool OnDoStep()
        {
            if (_PreReq.IsReady)
            {
                void _doBalance()
                {
                    foreach (var _bal in Activity.Actor.Adjuncts.OfType<Balancing>().ToList())
                    {
                        _bal.Eject();
                    }
                    Activity.Actor.AddAdjunct(new Balancing(_PreReq.ToTarget()));
                };
                if (_Accelerated && !_PreReq.IsUsingPenalty)
                {
                    // no longer charging/running
                    _doBalance();
                    AppendFollowing(Activity.GetActivityResultNotifyStep(@"No longer charging or running"));
                    AppendFollowing(new MoveCostCheckStep(Activity));
                }
                else if (_PreReq.Success)
                {
                    // succeeded on balance
                    _doBalance();
                    AppendFollowing(Activity.GetActivityResultNotifyStep(@"Succeeded on balance check"));
                    AppendFollowing(new MoveCostCheckStep(Activity));
                }
                else if (_PreReq.SoftFail(4))
                {
                    // movement into this space will be the last
                    AppendFollowing(Activity.GetActivityResultNotifyStep(@"Failed balance by 4 or less"));
                    Activity.AppendCompletion(
                        new CanStillMoveStep(Activity, MovementAction.MovementBudget));
                }
                else
                {
                    // falling
                    AppendFollowing(Activity.GetActivityResultNotifyStep(@"Balance failure"));
                    Activity.AppendCompletion(
                        new FallingStartStep(Activity, BalancerLocator, 500, 0, 1));
                }
                return true;
            }

            // will finish this movement
            return false;
        }
    }
}
