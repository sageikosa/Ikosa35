using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;

namespace Uzi.Ikosa.Items.Weapons.Ranged
{
    [Serializable]
    public class FoldNet : ActionBase
    {
        public FoldNet(ThrowingNet net, string orderKey)
            : base(net, new ActionTime(4), true, false, orderKey)
        {
        }

        public ThrowingNet ThrowingNet => Source as ThrowingNet;

        public override string Key => @"Net.Fold";
        public override string DisplayName(CoreActor actor) => $@"Fold Net: {ThrowingNet.GetKnownName(actor)}";
        public override bool IsStackBase(CoreActivity activity) => false;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => ObservedActivityInfoFactory.CreateInfo(@"Fold Net", activity.Actor, observer, ThrowingNet);

        #region protected override CoreStep OnPerformActivity(CoreActivity activity)
        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            // register activity
            if ((_Budget != null) && (_Budget.Actor != null))
            {
                if ((_Budget.Actor is Creature _critter)
                    && _critter.Proficiencies.IsProficientWith(ThrowingNet, _critter.AdvancementLog.NumberPowerDice))
                {
                    // proficient users can perform in half the time
                    TimeCost = new ActionTime(2);
                }
            }

            activity.EnqueueRegisterPreEmptively(Budget);
            return new FoldNetStep(activity);
        }
        #endregion

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            // sourced by the net, so no need to aim
            yield break;
        }

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;
    }

    [Serializable]
    public class FoldNetStep : CoreStep
    {
        public FoldNetStep(CoreActivity activity)
            : base(activity)
        {
        }

        public CoreActivity Activity => Process as CoreActivity; 

        protected override StepPrerequisite OnNextPrerequisite() => null; 
        public override bool IsDispensingPrerequisites => false; 

        protected override bool OnDoStep()
        {
            if (Activity != null)
            {
                var _fold = Activity.Action as FoldNet;
                if (_fold != null)
                {
                    _fold.ThrowingNet.IsFolded = true;
                    // TODO: additional information...refresh, describe
                }
            }
            return true;
        }
    }
}
