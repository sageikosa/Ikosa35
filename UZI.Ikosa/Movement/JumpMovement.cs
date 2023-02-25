using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class JumpMovement : MovementBase
    {
        public JumpMovement(Creature creature, object source)
            : base(0, creature, source)
        {
        }

        public override string Name => @"Jump";
        public override bool CanShiftPosition => false;
        public override bool SurfacePressure => false;

        public override bool IsNativeMovement
            => false;

        protected override MovementLocatorTarget GetNextGeometry(CoreTargetingProcess process,
            Locator locator, CellLocation leadCell, Dictionary<Guid, ICore> exclusions)
        {
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

                    // TODO: generally true
                    return true;
                }
                return false;
            }
        }

        public override bool CanMoveThrough(CellMaterial material)
        {
            return (material is GasCellMaterial) || (material is VoidCellMaterial);
        }

        public override bool CanMoveThrough(Material material)
        {
            return false;
        }

        public override MovementBase Clone(Creature forCreature, object source)
        {
            return new JumpMovement(forCreature, source);
        }
    }
}
