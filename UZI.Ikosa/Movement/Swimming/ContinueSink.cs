using System;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;
using Uzi.Visualize;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class ContinueSink : Adjunct, ITrackTime
    {
        public ContinueSink(Locator locator, IGeometricRegion targetRegion, SinkingMovement sinkMovement, double continueTime)
            : base(sinkMovement)
        {
            _Time = continueTime;
            _Locator = locator;
            _Region = targetRegion;
        }

        #region private data
        private double _Time;
        private Locator _Locator;
        private IGeometricRegion _Region;
        #endregion

        public SinkingMovement SinkingMovement { get { return Source as SinkingMovement; } }
        public double ContinueTime { get { return _Time; } }
        public Locator Locator { get { return _Locator; } }
        public IGeometricRegion TargetRegion { get { return _Region; } }

        public override object Clone()
        {
            return new ContinueSink(Locator, TargetRegion, SinkingMovement, ContinueTime);
        }

        #region ITrackTime Members

        public void TrackTime(double timeVal, TimeValTransition direction)
        {
            if ((timeVal >= _Time) && (direction == TimeValTransition.Leaving))
            {
                // must still be sinking
                var _sink = SinkingMovement.CoreObject.Adjuncts.OfType<Sinking>().FirstOrDefault(_s => _s.IsActive);
                if (_sink != null)
                {
                    SinkingMovement.Distance = 0;
                    var _procMan = Locator.Map.ContextSet.ProcessManager;
                    if (_procMan != null)
                    {
                        var _process = new CoreProcess(new SinkingStartStep(Locator, 5), @"Sinking");
                        _procMan.StartProcess(_process);
                    }
                }
                this.Eject();
            }
        }

        public double Resolution { get { return Round.UnitFactor; } }

        #endregion
    }
}
