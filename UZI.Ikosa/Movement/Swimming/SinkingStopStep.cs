using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class SinkingStopStep : CoreStep
    {
        public SinkingStopStep(CoreStep predecessor, Locator locator, SinkingMovement movement)
            : base(predecessor)
        {
            _Locator = locator;
            _SinkMove = movement;
        }

        #region state
        private Locator _Locator;
        private SinkingMovement _SinkMove;
        #endregion

        public override string Name => @"Sinking stopped";
        public Locator Locator => _Locator;
        public SinkingMovement SinkingMovement { get => _SinkMove; set => _SinkMove = value; }
        public override bool IsDispensingPrerequisites => false;

        protected override StepPrerequisite OnNextPrerequisite()
            => null;

        protected override bool OnDoStep()
        {
            foreach (var _interact in Locator.ICoreAs<IInteract>())
            {
                if (_interact is Creature)
                {
                    var _critter = _interact as Creature;

                    // generate impact sound
                    SinkingMovement.GenerateImpactSound();

                    // trove merge needed on sinking stop 
                    // sinking != falling, so prone won't apply in creature fall-stop handler
                    var _msg = new List<string>();
                    var _fallStop = new FallStop(Locator, _msg);
                    _interact.HandleInteraction(new StepInteraction(this, null, this, _interact, _fallStop));
                }
            }

            // remove sinking adjunct
            SinkingMovement.RemoveSinking();
            return true;
        }
    }
}
