using System;
using Uzi.Core;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Actions.Steps;
using System.Windows.Media.Media3D;
using Uzi.Visualize;
using Uzi.Ikosa.Time;
using System.Diagnostics;
using Uzi.Core.Contracts;
using System.Linq;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class FallingStep : RelocationStep
    {
        public FallingStep(CoreStep predecessor, Locator locator, IGeometricRegion targetRegion,
            BaseFallMovement baseFall, double distance, AnchorFaceList crossings, AnchorFace baseFace)
            : base(predecessor, locator, targetRegion, baseFall, new Vector3D(), crossings, baseFace)
        {
            _Distance = distance;
        }

        #region state
        private double _Distance;
        #endregion

        public override string Name => @"Falling";
        public BaseFallMovement BaseFallMovement => Movement as BaseFallMovement;
        public Creature Creature => Movement.CoreObject as Creature;

        protected override bool OnDoStep()
        {
            // move first (count distance against current movement)
            var _movement = BaseFallMovement;
            _movement.AddInterval(0.5d);
            _Distance += 5d / _movement.EffectiveValue;

            // must do the base before calling NextRegion, since it updates location
            var _return = base.OnDoStep();
            if (_return)
            {
                // swap movements if slow fall pops in or out
                if (_movement is FallMovement 
                    && _movement.CoreObject.HasAdjunct<SlowFallEffect>())
                {
                    // could happen midway (or could be a resumption after temporary suppression)
                    _movement = new SlowFallMovement(_movement.CoreObject, this, 60);
                    _movement.CoreObject.AddAdjunct(new SlowFalling(_movement as SlowFallMovement));
                }
                else if (_movement is SlowFallMovement
                    && !_movement.CoreObject.HasAdjunct<SlowFallEffect>())
                {
                    // could be suppressed or simply expired
                    // NOTE: this is a fresh movement, so damage is unaccumulated
                    _movement = new FallMovement(_movement.CoreObject, this, 500, 0, 0);
                    _movement.CoreObject.AddAdjunct(new Falling(_movement as FallMovement));
                }

                // see if next cube will block transit movement, 
                var _region = _movement.NextRegion();

                // if not, continue fall
                if (_region != null)
                {
                    // check max fall distance (per round)...
                    if (_Distance < 1d)
                    {
                        // if creature has gained a recoverable flight mode, they can cease falling
                        var _recover = FlightSuMovement.FirstRecoverableFlight(Creature);
                        if (_recover == null)
                        {
                            // immediate continue (possibly with movement swap)
                            var _grav = Locator.GetGravityFace();
                            new FallingStep(this, Locator, _region, _movement, _Distance,
                                AnchorFaceListHelper.Create(_grav), _grav);
                        }
                        else
                        {
                            Locator.ActiveMovement = _recover;
                            // TODO: set flight budget
                        }
                    }
                    else
                    {
                        // continue next round (possibly with movement swap)
                        var _next = (_movement.CoreObject.Setting as ITacticalMap).CurrentTime + Round.UnitFactor;
                        _movement.CoreObject.AddAdjunct(new ContinueFall(Locator, _region, _movement, _next));
                    }
                }
                else
                {
                    _movement.ProcessNoRegion(this, Locator);

                    // remove slow fall effects controlled by magic
                    foreach (var _sfe in BaseFallMovement.CoreObject.Adjuncts.OfType<SlowFallEffect>().ToList())
                    {
                        (_sfe.Source as MagicPowerEffect)?.Eject();
                    }
                }
            }
            return _return;
        }
    }
}
