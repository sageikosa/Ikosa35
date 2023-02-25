using System;
using Uzi.Core;
using Uzi.Ikosa.Time;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class ContinueFall : Adjunct, ITrackTime
    {
        public ContinueFall(Locator locator, IGeometricRegion targetRegion, BaseFallMovement fallMovement, double continueTime)
            : base(fallMovement)
        {
            _Time = continueTime;
            _Locator = locator;
            _Region = targetRegion;
        }

        #region state
        private double _Time;
        private Locator _Locator;
        private IGeometricRegion _Region;
        #endregion

        public BaseFallMovement BaseFallMovement => Source as BaseFallMovement;
        public double ContinueTime => _Time;
        public Locator Locator => _Locator;
        public IGeometricRegion TargetRegion => _Region;

        public override object Clone()
            => new ContinueFall(Locator, TargetRegion, BaseFallMovement, ContinueTime);

        #region ITrackTime Members

        public void TrackTime(double timeVal, TimeValTransition direction)
        {
            if ((timeVal >= _Time) && (direction == TimeValTransition.Entering))
            {
                // if creature has gained a recoverable flight mode, they can cease falling
                var _recover = FlightSuMovement.FirstRecoverableFlight(BaseFallMovement.CoreObject as Creature);
                if (_recover == null)
                {
                    var _procMan = Locator.Map.ContextSet.ProcessManager;
                    if (_procMan != null)
                    {
                        var _process = new CoreProcess(new FallingContinueStep(Locator, TargetRegion, BaseFallMovement),
                            @"Continue Fall");
                        _procMan.StartProcess(_process);
                    }
                }
                else
                {
                    Locator.ActiveMovement = _recover;
                    // TODO: set flight budget
                }
                Eject();
            }
        }

        public double Resolution => Round.UnitFactor; 

        #endregion
    }
}
