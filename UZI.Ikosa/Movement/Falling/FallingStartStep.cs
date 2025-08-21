using Uzi.Core.Contracts;
using System;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;
using Uzi.Ikosa.Skills;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Universal;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class FallingStartStep : PreReqListStepBase
    {
        #region construction
        public FallingStartStep(CoreProcess process, Locator locator, int speed, double damageStart, double maxNonLethal)
            : base(process)
        {
            Init(locator, speed, damageStart, maxNonLethal);
        }

        public FallingStartStep(CoreStep predecessor, Locator locator, int speed, double damageStart, double maxNonLethal)
            : base(predecessor)
        {
            Init(locator, speed, damageStart, maxNonLethal);
        }

        #region private void Init(Locator locator, int speed, double damageStart, double maxNonLethal)
        private void Init(Locator locator, int speed, double damageStart, double maxNonLethal)
        {
            _Locator = locator;
            _FallMove = new FallMovement(locator.ICore as CoreObject, typeof(FallMovement), speed, damageStart, maxNonLethal);
            var _tumble = Creature?.Skills.Skill<TumbleSkill>();
            if (_tumble?.IsUsable ?? false)
            {
                _PendingPreRequisites.Enqueue(
                    new SuccessCheckPrerequisite(this, new Qualifier(null, FallMovement, FallMovement.CoreObject),
                        @"Tumble", @"Tumble check to reduce fall", new SuccessCheck(_tumble, 15, FallMovement), false));
            }
        }
        #endregion

        #endregion

        #region data
        private Locator _Locator;
        private FallMovement _FallMove;
        #endregion

        public static void StartFall(Locator locator, int speed, int continueSpeed, string reason, double fallReduce = 0d)
        {
            var _step = new FallingStartStep((CoreProcess)null, locator, speed, 0, 0);
            _step.FallMovement.ContinueSpeed = continueSpeed;
            _step.FallMovement.FallReduce = fallReduce;
            var _process = new CoreProcess(_step, reason);
            locator.Map.ContextSet.ProcessManager.StartProcess(_process);
        }

        public override string Name => @"Started falling";
        public Locator Locator => _Locator;
        public FallMovement FallMovement => _FallMove;
        public Creature Creature => _FallMove.CoreObject as Creature;

        private MovementBudget GetBudget()
            => Locator.IkosaProcessManager.LocalTurnTracker?.GetBudget(Locator.Chief?.ID ?? Guid.Empty)
            ?.BudgetItems[typeof(MovementBudget)] as MovementBudget;

        protected override bool OnDoStep()
        {
            #region preload tumble distance reduction
            var _tumble = AllPrerequisites<SuccessCheckPrerequisite>().FirstOrDefault();
            if (_tumble != null)
            {
                // NOTE: adding to fall reduction, as other powers may also affect fall distance
                //       and we don't want to obliterate that
                if (_tumble.Success)
                {
                    if (_tumble.Result >= 100)
                    {
                        FallMovement.FallReduce += 20;
                    }
                    else if (_tumble.Result >= 60)
                    {
                        FallMovement.FallReduce += 4;
                    }
                    else if (_tumble.Result >= 45)
                    {
                        FallMovement.FallReduce += 3;
                    }
                    else if (_tumble.Result >= 30)
                    {
                        FallMovement.FallReduce += 2;
                    }
                    else
                    {
                        FallMovement.FallReduce += 1;
                    }
                }
            }
            #endregion

            FallMovement.CoreObject?.AddAdjunct(new Falling(FallMovement));

            // check directional movement
            var _region = FallMovement.NextRegion();

            // once falling starts, no more movement
            var _budget = GetBudget();
            if (_budget != null)
            {
                _budget.CanStillMove = false;
            }

            if (_region != null)
            {
                if (FallMovement.CoreObject != null)
                {
                    EnqueueNotify(new BadNewsNotify(FallMovement.CoreObject.ID, @"Movement", new Description(@"Falling", @"Started")), FallMovement.CoreObject.ID);
                }

                new FallingStep(this, Locator, _region, FallMovement, 0d, AnchorFaceListHelper.Create(Locator.GetGravityFace()), Locator.GetGravityFace());
            }
            else
            {
                // hypothetical sinking movement to check
                var _sink = new SinkingMovement(5, FallMovement.CoreObject, this);
                var _sinkRegion = _sink.NextRegion();
                if (_sinkRegion != null)
                {
                    // if we can sink, we should sink
                    if (FallMovement.CoreObject != null)
                    {
                        EnqueueNotify(new BadNewsNotify(FallMovement.CoreObject.ID, @"Movement", new Description(@"Falling", @"going to sink")), FallMovement.CoreObject.ID);
                    }

                    AppendFollowing(new SinkingStartStep(Process, Locator, 5));
                }
                else
                {
                    if (FallMovement.CoreObject != null)
                    {
                        EnqueueNotify(new GoodNewsNotify(FallMovement.CoreObject.ID, @"Movement", new Description(@"Falling", @"no fall to location")), FallMovement.CoreObject.ID);
                    }

                    new FallingStopStep(this, Locator, FallMovement);
                }
            }
            return true;
        }
    }
}
