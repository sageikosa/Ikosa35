using System;
using Uzi.Ikosa.Tactical;
using Uzi.Core;
using Uzi.Visualize;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Actions;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class BurrowMovement : MovementBase
    {
        public BurrowMovement(int speed, Creature creature, object source, bool shiftable)
            : base(speed, creature, source)
        {
            _Shift = shiftable;
        }

        public override string Name => @"Burrow";

        private bool _Shift;
        public override bool CanShiftPosition => _Shift;

        /// <summary>Burrowing doesn't need to visualize, it uses tremor sense</summary>
        protected override bool MustVisualizeMovement => false;

        public override bool SurfacePressure => true;

        public override bool IsNativeMovement
            => (Source == CoreObject) || ((CoreObject as Creature)?.Body == Source);

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
                if (locator.NormalSize.XLength == 1)
                {
                    var _next = leadCell.Move(_crossings.GetAnchorOffset()) as CellLocation;
                    if (!locator.Map.CanOccupy(_locCore, _next, this, exclusions, locator.PlanarPresence))
                        return null;
                    if (!TransitFitness(process as CoreActivity, _crossings, leadCell, locator.Map, locator.ActiveMovement,
                        locator.PlanarPresence, exclusions) ?? true)
                        return null;
                    // TODO: see if total region (including any extended) needs to squeeze...
                    return new MovementLocatorTarget
                    {
                        Locator = locator,
                        TargetRegion = _next,
                        BaseFace = _gravity
                    };
                }
                else if (locator.NormalSize.XLength == 2)
                {
                }
                else if (locator.NormalSize.XLength == 3)
                {
                }
            }
            return null;
        }

        public override bool IsUsable
        {
            get
            {
                var _locator = CoreObject.GetLocated()?.Locator;
                if (_locator != null)
                {
                    if (!_locator.PlanarPresence.HasMaterialPresence())
                        return false;

                    // TODO: creature's locator must be in, or adjacent to, ground
                    return true;
                }
                return false;
            }
        }

        public override bool CanMoveThrough(Material material)
        {
            if ((material is EarthMaterial) && !(material is StoneMaterial))
            {
                return true;
            }
            return false;
        }

        public override bool CanMoveThrough(CellMaterial material)
        {
            if (material is SolidCellMaterial)
            {
                var _solid = material as SolidCellMaterial;
                return CanMoveThrough(_solid.SolidMaterial);
            }
            else if (material is GasCellMaterial)
            {
                // NOTE: can burrow through cells with gas
                // NOTE: burrow can only be used if the target has some earth material
                return true;
            }
            return false;
        }

        public override void OnRelocated(CoreProcess process, Locator locator)
        {
            // ensure an inflight adjunct is adjuncted
            var _burrowing = CoreObject.Adjuncts.OfType<Burrowing>().FirstOrDefault();
            if (_burrowing == null)
            {
                _burrowing = new Burrowing();
                CoreObject.AddAdjunct(_burrowing);
            }
            base.OnRelocated(process, locator);
        }

        public override void OnEndActiveMovement()
        {
            CoreObject.Adjuncts.OfType<Burrowing>().FirstOrDefault()?.Eject();
            base.OnEndActiveMovement();
        }

        public override MovementBase Clone(Creature forCreature, object source)
            => new BurrowMovement(BaseValue, forCreature, source, CanShiftPosition);
    }
}
