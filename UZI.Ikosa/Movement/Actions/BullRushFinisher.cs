using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using Uzi.Ikosa.Feats;
using Uzi.Core;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Actions;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class BullRushFinisher : ActionBase
    {
        public BullRushFinisher(MovementBase movement, Creature critter, string orderKey)
            : base(movement, new ActionTime(TimeType.SubAction), true, true, orderKey)
        {
            _Creature = critter;
            if (Creature.Feats.Contains(typeof(ImprovedBullRushFeat)))
            {
                _Target = false;
                _Improved = true;
            }
            else
                _Improved = false;

            // TODO: +4 push back on improved
        }

        private Creature _Creature;
        private bool _Improved;

        public Creature Creature => _Creature;
        public bool Improved => _Improved;
        public override string Key => @"Attack.BullRushCharge";
        public override string DisplayName(CoreActor actor) => $@"Charging Bull Rush ({Movement.Name})";
        public override bool CombatList => true;
        public MovementBase Movement => Source as MovementBase;

        public override bool IsStackBase(CoreActivity activity)
            => false;

        public override bool WillClearStack(CoreActionBudget budget, CoreActivity activity)
            => false;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => ObservedActivityInfoFactory.CreateInfo(@"Bull Rush", activity.Actor, observer, activity.Targets[0].Target as CoreObject);

        /// <summary>Overrides default IsHarmless setting, and sets it to false for attack actions</summary>
        public override bool IsHarmless
            => false;

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            // TODO:
            throw new NotImplementedException();
        }

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            // TODO:
            throw new NotImplementedException();
        }

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
        {
            if (ProvokesTarget)
            {
                // TODO:
            }
            return false;
        }
    }
}