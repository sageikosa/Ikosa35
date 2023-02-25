using System;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class LiquidFallingStartStep : CoreStep
    {
        public LiquidFallingStartStep(CoreStep predecessor, Locator locator, FallMovement falling)
            : base(predecessor)
        {
            _Locator = locator;
            _LiquidFall = new LiquidFallMovement(falling.CoreObject, falling, falling.EffectiveValue);
        }

        #region state
        private LiquidFallMovement _LiquidFall;
        private Locator _Locator;
        #endregion

        public override string Name => @"Fell into liquid";
        public Locator Locator => _Locator;
        public LiquidFallMovement LiquidFallMovement => _LiquidFall;

        protected override StepPrerequisite OnNextPrerequisite()
            => null;

        public override bool IsDispensingPrerequisites
            => false;

        protected override bool OnDoStep()
        {
            LiquidFallMovement.CoreObject?.AddAdjunct(new LiquidFalling(LiquidFallMovement));

            // check directional movement
            var _region = LiquidFallMovement.NextRegion();

            if (_region != null)
            {
                // generate sound...
                LiquidFallMovement.GenerateImpactSound(true);

                // NOTE: this is pretty much guaranteed, 
                // as we don't enter this step unless the hypothetical liquid falling could perform a next region
                EnqueueNotify(new BadNewsNotify(LiquidFallMovement.CoreObject.ID, @"Movement", new Description(@"Liquid Falling", @"Started")),
                    LiquidFallMovement.CoreObject.ID);
                new LiquidFallingStep(this, Locator, _region, LiquidFallMovement, AnchorFaceListHelper.Create(Locator.GetGravityFace()), Locator.GetGravityFace());
            }
            return true;
        }
    }
}
