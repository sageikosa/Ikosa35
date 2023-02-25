using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions.Steps;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class BurstFree : ActionBase
    {
        public BurstFree(ICanBurstFree burstFree,  bool provokesMelee, string orderKey)
            : base(burstFree, burstFree.BurstFreeTime, provokesMelee, false, orderKey)
        {
        }

        public ICanBurstFree BurstFreeSource => Source as ICanBurstFree;
        public override string Key => @"Ability.BurstFree";
        public override string DisplayName(CoreActor actor) => $@"Burst free from {BurstFreeSource.BurstFromName(actor)}";

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield break;
        }

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => ObservedActivityInfoFactory.CreateInfo(@"Burst", activity.Actor, observer);

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;

        public override bool IsStackBase(CoreActivity activity)
            => false;

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            activity.EnqueueRegisterPreEmptively(Budget);
            if (activity.Actor is Creature _critter)
            {
                return new BurstFreeStep(activity, _critter, this);
            }
            return null;
        }
    }

    public interface ICanBurstFree : IActionSource
    {
        ActionTime BurstFreeTime { get; }
        string BurstFromName(CoreActor actor);
        Deltable BurstFreeDifficulty { get; }
        void DoBurstFree();
        IInteract BurstFrom { get; }
    }
}
