using System;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Universal;
using Uzi.Visualize;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class SinkingStartStep : CoreStep
    {
        #region construction
        public SinkingStartStep(CoreProcess process, Locator locator, int speed)
            : base(process)
        {
            Init(locator, speed);
        }

        public SinkingStartStep(Locator locator, int speed)
            : base((CoreProcess)null)
        {
            Init(locator, speed);
        }

        #region private void Init(Locator locator, int speed)
        private void Init(Locator locator, int speed)
        {
            _Locator = locator;
            _Sinking = new SinkingMovement(speed, locator.Chief as Creature, typeof(SinkingMovement));
        }
        #endregion

        #endregion

        #region private data
        private Locator _Locator;
        private SinkingMovement _Sinking;
        #endregion

        public override string Name => @"Sinking";
        public Locator Locator => _Locator;
        public SinkingMovement SinkingMovement => _Sinking;

        private MovementBudget GetBudget()
            => Locator.IkosaProcessManager?.LocalTurnTracker.GetBudget(Locator.Chief?.ID ?? Guid.Empty)
            ?.BudgetItems[typeof(MovementBudget)] as MovementBudget;

        protected override bool OnDoStep()
        {
            // init once the step starts, just in case status needs to flow
            SinkingMovement?.CoreObject?.AddAdjunct(new Sinking(SinkingMovement));

            // check directional movement
            var _region = SinkingMovement.NextRegion();

            // once swim failing starts, no more movement
            var _budget = GetBudget();
            if (_budget != null)
            {
                _budget.CanStillMove = false;
            }

            if (_region != null)
            {
                EnqueueNotify(new BadNewsNotify(SinkingMovement.CoreObject.ID, @"Movement", new Description(@"Sinking", @"Started")),
                    SinkingMovement.CoreObject.ID);
                var _grav = Locator.GetGravityFace();
                new SinkingStep(this, Locator, _region, SinkingMovement,
                    AnchorFaceListHelper.Create(_grav), _grav);
            }
            else
            {
                // hypothetical falling movement to check
                var _fall = new FallMovement(SinkingMovement.CoreObject, this, 500, 0, 0);
                var _fallRegion = _fall.NextRegion();
                if (_fallRegion != null)
                {
                    EnqueueNotify(new BadNewsNotify(SinkingMovement.CoreObject.ID, @"Movement", new Description(@"Sinking", @"sunk out of liquid")),
                        SinkingMovement.CoreObject.ID);
                    new FallingStartStep(this, Locator, 500, 0, 1);
                }
                else
                {
                    EnqueueNotify(new GoodNewsNotify(SinkingMovement.CoreObject.ID, @"Movement", new Description(@"Sinking", @"no place to go!")),
                        SinkingMovement.CoreObject.ID);
                    new SinkingStopStep(this, Locator, SinkingMovement);
                }
            }
            return true;
        }

        protected override StepPrerequisite OnNextPrerequisite()
            => null;

        public override bool IsDispensingPrerequisites
            => false;
    }
}
