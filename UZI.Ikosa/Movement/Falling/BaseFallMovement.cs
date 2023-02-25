using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public abstract class BaseFallMovement : MovementBase
    {
        protected BaseFallMovement(CoreObject coreObj, object source, int speed)
            : base(speed, coreObj, source)
        {
        }

        public override bool IsNativeMovement => false;
        public override bool CanShiftPosition => false;
        public override bool SurfacePressure => false;

        #region public override bool IsUsable { get; }
        /// <summary>True if gravity directional blockage is less than 100%</summary>
        public override bool IsUsable
        {
            get
            {
                var _locator = CoreObject.GetLocated()?.Locator;
                if (_locator?.IsGravityEffective ?? false)
                {
                    if (!_locator.PlanarPresence.HasMaterialPresence())
                        return false;

                    var _grav = _locator.GetGravityFace();
                    return _locator.Map.DirectionalBlockage(CoreObject, _locator.GeometricRegion, this, _grav, _grav, null, _locator.PlanarPresence) < 1;
                }
                return false;
            }
        }
        #endregion

        public override bool CanMoveThrough(Items.Materials.Material material)
            => false;

        public override bool CanMoveThrough(CellMaterial material)
            => (material is GasCellMaterial) || (material is VoidCellMaterial);

        #region protected override IGeometricRegion GetNextGeometry(CoreTargettingProcess activity)
        protected override MovementLocatorTarget GetNextGeometry(CoreTargetingProcess process,
            Locator locator, CellLocation leadCell, Dictionary<Guid, ICore> exclusions)
        {
            var _dest = process.GetFirstTarget<StepDestinationTarget>(MovementTargets.Direction);
            var _idx = process.GetFirstTarget<ValueTarget<int>>(MovementTargets.StepIndex);
            if ((locator != null) && (leadCell != null) && (_dest != null))
            {
                var _locCore = locator.ICore as ICoreObject;
                var _gravity = locator.GetGravityFace();
                var _crossings = _dest.CrossingFaces(_gravity, _idx?.Value ?? 0);
                var _planar = locator.PlanarPresence;

                // get next cell
                var _next = leadCell.Move(_crossings.GetAnchorOffset()) as CellLocation;

                // make sure we can fall into the next cell
                if (!locator.Map.CanPlummet(leadCell, _next, locator.ActiveMovement, this, _gravity, _planar))
                {
                    // couldn't fall into next cell, stops fall (by not providing a next cell)
                    return null;
                }

                // now see if the next cell is occupiable
                if (!locator.Map.CanOccupy(_locCore, _next, this, exclusions, _planar))
                {
                    // couldn't occupy it, so try deflection
                    var _deflect = locator.Map.PlummetDeflection(_next, this, _gravity);
                    if (_deflect.Any())
                    {
                        // try the deflection
                        _next = _next.Move(_deflect.GetAnchorOffset()) as CellLocation;
                        if (!locator.Map.CanOccupy(_locCore, _next, this, exclusions, _planar))
                        {
                            // couldn't occupy the deflected cell
                            return null;
                        }
                    }
                    else
                    {
                        // no deflection, must stop
                        return null;
                    }
                }
                if (!TransitFitness(process, _crossings, leadCell, locator.Map, locator.ActiveMovement, _planar, exclusions) ?? false)
                {
                    // unable to transit, so stop
                    return null;
                }

                if (locator.NormalSize.XLength == 1)
                {
                    var _cubic = new Cubic(_next, 1, 1, 1);

                    // provide a cell list
                    var _moveVol = new MovementVolume(_locCore, this, _crossings, _gravity, locator.Map, _planar, exclusions);
                    var _final = _moveVol.SqueezeCells(_cubic, locator.ZFit, locator.YFit, locator.XFit, 0.5d, _gravity.GetAxis());
                    return new MovementLocatorTarget
                    {
                        Locator = locator,
                        TargetRegion = _final,
                        BaseFace = _gravity
                    };
                }
                else
                {
                    // TODO: needs to be more robust for large locators...

                    // cubic first
                    var _offset = locator.GeometricRegion.GetExitCubicOffset(leadCell, locator.NormalSize);
                    var _cubeStart = _next.Add(_offset);
                    var _cubic = new Cubic(_cubeStart, locator.NormalSize);

                    // provide a cell list
                    var _moveVol = new MovementVolume(_locCore, this, _crossings, _gravity, locator.Map, _planar, exclusions);
                    var _final = _moveVol.SqueezeCells(_cubic, locator.ZFit, locator.YFit, locator.XFit, 0.5d, _gravity.GetAxis());
                    return new MovementLocatorTarget
                    {
                        Locator = locator,
                        TargetRegion = _final,
                        BaseFace = _gravity
                    };
                }
            }
            return null;
        }
        #endregion

        #region public IGeometricRegion NextRegion()
        /// <summary>Determine what the next region should be</summary>
        public IGeometricRegion NextRegion()
        {
            var _loc = Locator.FindFirstLocator(CoreObject);
            if (_loc != null)
            {
                var _activity = new CoreTargetingProcess(CoreObject, Name,
                    new List<AimTarget>
                    {
                        new HeadingTarget(MovementTargets.Direction, 8, -2),
                        new ValueTarget<int>(MovementTargets.StepIndex, 0)
                    });
                if (IsValidActivity(_activity, _activity.MainObject))
                {
                    return _activity
                        .GetFirstTarget<ValueTarget<MovementLocatorTarget>>(MovementTargets.MoveData)
                        ?.Value.TargetRegion;
                }
            }
            return null;
        }
        #endregion

        public virtual void AddInterval(double amount) { }

        public virtual void ProcessNoRegion(CoreStep step, Locator locator) { }

        public virtual void RemoveFalling() { }

        /// <summary>True if the movement represents an actual fall (rather than one that started, but couldn't continue)</summary>
        public virtual bool IsUncontrolled => false;
    }
}
