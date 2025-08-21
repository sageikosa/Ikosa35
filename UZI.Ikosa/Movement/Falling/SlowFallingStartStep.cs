using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class SlowFallingStartStep : PreReqListStepBase
    {
        public SlowFallingStartStep(CoreProcess process, Locator locator, int speed)
            : base(process)
        {
            Init(locator, speed);
        }

        #region private void Init(Locator locator, int speed)
        private void Init(Locator locator, int speed)
        {
            _Locator = locator;
            _SlowFall = new SlowFallMovement(locator.ICore as CoreObject, typeof(FallMovement), speed);
        }
        #endregion

        #region state
        private Locator _Locator;
        private SlowFallMovement _SlowFall;
        #endregion

        public static void StartSlowFall(Locator locator, int speed, string reason)
        {
            var _step = new SlowFallingStartStep((CoreProcess)null, locator, speed);
            var _process = new CoreProcess(_step, reason);
            locator.Map.ContextSet.ProcessManager.StartProcess(_process);
        }

        public override string Name => @"Started falling";
        public Locator Locator => _Locator;
        public SlowFallMovement SlowFallMovement => _SlowFall;
        public Creature Creature => _SlowFall.CoreObject as Creature;

        private MovementBudget GetBudget()
            => Locator.IkosaProcessManager.LocalTurnTracker?.GetBudget(Locator.Chief?.ID ?? Guid.Empty)
            ?.BudgetItems[typeof(MovementBudget)] as MovementBudget;

        protected override bool OnDoStep()
        {
            SlowFallMovement.CoreObject?.AddAdjunct(new SlowFalling(SlowFallMovement));
         
            // check directional movement
            var _region = SlowFallMovement.NextRegion();

            // once falling starts, no more movement
            var _budget = GetBudget();
            if (_budget != null)
            {
                _budget.CanStillMove = false;
            }

            if (_region != null)
            {
                if (SlowFallMovement.CoreObject != null)
                {
                    EnqueueNotify(new BadNewsNotify(SlowFallMovement.CoreObject.ID, @"Movement", new Description(@"Slow Falling", @"Started")), SlowFallMovement.CoreObject.ID);
                }

                new FallingStep(this, Locator, _region, SlowFallMovement, 0d, AnchorFaceListHelper.Create(Locator.GetGravityFace()), Locator.GetGravityFace());
            }
            else
            {
                SlowFallMovement.ProcessNoRegion(this, Locator);

                // remove slow fall effects controlled by magic
                foreach (var _sfe in SlowFallMovement.CoreObject.Adjuncts.OfType<SlowFallEffect>().ToList())
                {
                    (_sfe.Source as MagicPowerEffect)?.Eject();
                }
            }
            return true;
        }
    }
}
