using System;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Universal;
using Uzi.Ikosa.Contracts;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class FallingContinueStep : PreReqListStepBase
    {
        #region construction
        public FallingContinueStep(Locator locator, IGeometricRegion targetRegion, BaseFallMovement movement)
            : base((CoreProcess)null)
        {
            _Locator = locator;
            _Region = targetRegion;
            _FallMove = movement;

            if (FallMovement is FallMovement _fullFall)
            {
                if (_fullFall.ContinueSpeed != FallMovement.EffectiveValue)
                {
                    FallMovement.BaseValue = _fullFall.ContinueSpeed;
                    if (_fullFall.FlightStall)
                    {
                        // must make a difficulty 20 reflex save to cease falling
                        var _qualifier = new Qualifier(null, this, movement.CoreObject);
                        var _delta = new DeltaCalcInfo(FallMovement.CoreObject.ID, @"Save to Stop Falling") { Result = 20, BaseValue = 20 };
                        _PendingPreRequisites.Enqueue(new SavePrerequisite(this, _qualifier, @"Save.Reflex", @"Save to Stop Falling",
                            new SaveMode(SaveType.Reflex, SaveEffect.Negates, _delta)));
                    }
                }
            }
        }
        #endregion

        #region state
        private IGeometricRegion _Region;
        private Locator _Locator;
        private BaseFallMovement _FallMove;
        #endregion

        public override string Name => @"Continue falling";
        public Locator Locator => _Locator;
        public IGeometricRegion Region => _Region;
        public BaseFallMovement FallMovement { get { return _FallMove; } set { _FallMove = value; } }

        private LocalActionBudget GetBudget()
            => Locator.IkosaProcessManager?.LocalTurnTracker.GetBudget(Locator.Chief.ID);

        protected override bool OnDoStep()
        {
            var _budget = GetBudget();
            if (_budget != null)
            {
                _budget.ConsumeBudget(TimeType.Total);
            }

            var _savePre = AllPrerequisites<SavePrerequisite>(@"Save.Reflex").FirstOrDefault();
            if ((_savePre != null) && _savePre.Success)
            {
                // if recovered, set flight budget to non-stalling
                if (_budget != null)
                {
                    if (_budget.BudgetItems[FallMovement] is FlightBudget _fBudg)
                    {
                        _fBudg.HasRecovered = true;
                    }
                }
            }
            else
            {
                // only if the fall is not a stall, or the save to recover failed
                new FallingStep(this, Locator, Region, FallMovement, 0d, AnchorFaceListHelper.Create(Locator.GetGravityFace()), Locator.GetGravityFace());
            }
            return true;
        }
    }
}
