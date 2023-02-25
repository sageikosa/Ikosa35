using System;
using System.Windows.Media.Media3D;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;
using Uzi.Visualize;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class SinkingStep : RelocationStep
    {
        public SinkingStep(CoreStep predecessor, Locator locator, IGeometricRegion target,
            SinkingMovement sinkMove, AnchorFaceList crossings, AnchorFace baseFace)
            : base(predecessor, locator, target, sinkMove, new Vector3D(), crossings, baseFace)
        {
        }

        public override string Name => @"Sinking";
        public SinkingMovement SinkingMovement => Movement as SinkingMovement;

        #region protected override bool OnDoStep()
        protected override bool OnDoStep()
        {
            // must do the base before calling NextRegion, since it updates location
            var _last = Locator.CenterPoint;
            var _return = base.OnDoStep();
            if (_return)
            {
                // log the distance sunk
                SinkingMovement.Distance += (int)(Locator.CenterPoint - _last).Length;

                // see if next cube will block transit movement, 
                var _map = Locator.Map;
                var _region = SinkingMovement.NextRegion();

                // if not, continue fail
                if (_region != null)
                {
                    // check max fall distance (per round)...
                    if (SinkingMovement.Distance < SinkingMovement.EffectiveValue)
                    {
                        // immediate continue
                        var _grav = Locator.GetGravityFace();
                        new SinkingStep(this, Locator, _region, SinkingMovement,
                            AnchorFaceListHelper.Create(_grav), _grav);
                    }
                    else
                    {
                        // continue next round (if haven't started swimming...)
                        var _next = (SinkingMovement.CoreObject.Setting as ITacticalMap).CurrentTime + Round.UnitFactor;
                        SinkingMovement.CoreObject.AddAdjunct(new ContinueSink(Locator, _region, SinkingMovement, _next));
                    }
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
            }
            return _return;
        }
        #endregion
    }
}