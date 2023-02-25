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
    public class BullRush : ActionBase
    {
        public BullRush(MovementBase movement, Creature critter, string orderKey)
            : base(movement, new ActionTime(TimeType.Regular), true, true, orderKey)
        {
            _Creature = critter;
            if (_Creature.Feats.Contains(typeof(ImprovedBullRushFeat)))
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

        public MovementBase Movement => Source as MovementBase;
        public Creature Creature => _Creature;
        public bool Improved => _Improved;
        public override string Key => @"Attack.BullRush";
        public override string DisplayName(CoreActor actor) => $@"Bull Rush ({Movement.Name})";
        public override bool CombatList => true;

        public override bool IsStackBase(CoreActivity activity)
            => false;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => ObservedActivityInfoFactory.CreateInfo(@"Bull Rush", activity.Actor, observer, activity.Targets[0].Target as CoreObject);

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
