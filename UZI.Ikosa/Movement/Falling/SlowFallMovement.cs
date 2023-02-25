using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class SlowFallMovement : BaseFallMovement
    {
        public SlowFallMovement(CoreObject coreObj, object source, int speed)
            : base(coreObj, source, speed)
        {
            _Distance = 0;
        }

        #region state
        private double _Distance;
        #endregion

        public override void RemoveFalling()
        {
            // remove falling adjunct
            CoreObject.Adjuncts.OfType<SlowFalling>()
                .Where(_f => _f.SlowFallMovement == this)
                .FirstOrDefault()?.Eject();

            GetCurrentSound(CoreObject)?.Eject();
        }

        public override string Name => @"Slow Fall";

        public override MovementBase Clone(Creature forCreature, object source)
            => new SlowFallMovement(CoreObject, Source, BaseValue);

        public override void AddInterval(double amount)
        {
            if (amount > 0)
            {
                _Distance += 10 * amount;
            }
        }

        public override void ProcessNoRegion(CoreStep step, Locator locator)
        {
            // hypothetical liquid fall movement to check
            var sinking = new SinkingMovement(5, CoreObject, this);
            var _next = sinking.NextRegion();
            if (_next != null)
            {
                // if we can enter the drink, we should enter the drink
                step.EnqueueNotify(new BadNewsNotify(CoreObject.ID, @"Movement", new Description(@"Falling", @"fell into liquid")),
                     CoreObject.ID);
                step.AppendFollowing(new SinkingStartStep(step.Process, locator, 5));
            }
            else
            {
                // otherwise, stop (the hard way)
                new FallingStopStep(step, locator, this);
            }
        }
    }
}
