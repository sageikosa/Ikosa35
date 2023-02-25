using System;
using Uzi.Core;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Movement;
using Uzi.Visualize;
using System.Windows.Media.Media3D;
using System.Linq;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Skills;
using System.Collections.Generic;

namespace Uzi.Ikosa.Actions.Steps
{
    /// <summary>Used to adjust position...</summary>
    [Serializable]
    public class RelocationStep : CoreStep
    {
        #region ctor(...)
        /// <summary>Reactions (falling/sinking) used to adjust position...</summary>
        public RelocationStep(CoreStep predecessor, Locator locator, IGeometricRegion region,
            MovementBase activeMovement, Vector3D offset, AnchorFaceList crossings, AnchorFace baseFace)
            : base(predecessor)
        {
            _Locator = locator;
            _Region = region;
            _Movement = activeMovement;
            _Offset = offset;
            _Crossings = crossings;
            _BaseFace = baseFace;
        }

        /// <summary>Actions used to adjust position...</summary>
        public RelocationStep(CoreActivity activity, Locator locator, IGeometricRegion region,
            MovementBase activeMovement, Vector3D offset, AnchorFaceList crossings, AnchorFace baseFace)
            : base(activity)
        {
            _Locator = locator;
            _Region = region;
            _Movement = activeMovement;
            _Offset = offset;
            _Crossings = crossings;
            _BaseFace = baseFace;
        }
        #endregion

        #region Data
        private Locator _Locator;
        private IGeometricRegion _Region;
        private MovementBase _Movement;
        private Vector3D _Offset;
        private AnchorFaceList _Crossings;
        private AnchorFace _BaseFace;
        #endregion

        // TODO: consider ethereal relocation

        public AnchorFace BaseFace => _BaseFace;
        public AnchorFaceList Direction => _Crossings;
        public Locator Locator => _Locator;
        public IGeometricRegion Region => _Region;
        public MovementBase Movement => _Movement;
        public Vector3D OffsetVector => _Offset;

        protected override StepPrerequisite OnNextPrerequisite()
            => null;

        public override bool IsDispensingPrerequisites
            => false;

        protected override bool OnDoStep()
        {
            // even though there isn't a tracked time-tick per movestep, clear visualizations
            Locator.Map.ClearTransientVisualizers();

            // make sure no holding back...
            // legal region tracking and decisioning
            if (Movement.IsLegalPosition(Locator))
            {
                Locator.LastLegalRegion = Locator.GeometricRegion;
                Locator.LastModelOffset = Locator.IntraModelOffset;
            }
            else
            {
                Locator.LastLegalRegion = null;
            }

            // track some useful things about the locator
            if (Movement != null)
                Locator.ActiveMovement = Movement;      // useful when taking damage while climbing/balancing
            Locator.MovementCrossings = Direction;      // used in climbing cell snap determination
            Locator.IntraModelOffset = OffsetVector;    // terrain hugging

            // locator orientation
            var _lastBase = Locator.BaseFace;
            Locator.BaseFace = BaseFace;
            if ((_lastBase != BaseFace) && (Locator?.Chief is ISensorHost))
            {
                // movement crossings includes reverse heading?
                var _sensors = (Locator?.Chief as ISensorHost);
                var _lastHeadingFaces = _lastBase.GetHeadingFaces(_sensors.Heading);
                if (_lastHeadingFaces.Contains(BaseFace))
                {
                    // old-forward is now our base face, so look old-up
                    _sensors.Heading = BaseFace.GetHeadingValue(_lastBase.ReverseFace().ToAnchorFaceList());
                }
                else if (_lastHeadingFaces.Contains(BaseFace.ReverseFace()))
                {
                    // old-back is now our base, so look old-down
                    _sensors.Heading = BaseFace.GetHeadingValue(_lastBase.ToAnchorFaceList());
                }
                else
                {
                    // continue looking at same remote directionality
                    _sensors.Heading = BaseFace.GetHeadingValue(_lastHeadingFaces);
                }
            }

            // locate and notify
            Movement?.OnPreRelocated(Process, Locator);
            Locator.Relocate(Region, Locator.PlanarPresence);
            Movement?.OnRelocated(Process, Locator);

            // done
            return true;
        }
    }
}
