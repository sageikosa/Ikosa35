using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;

namespace Uzi.Ikosa.Movement
{
    /// <summary>Used to test object occupancy</summary>
    public class ObjectStaticMovement : MovementBase
    {
        /// <summary>Used to test object occupancy</summary>
        private ObjectStaticMovement()
            : base(0, null, null)
        {
        }

        public override string Name => @"Furnishing Tester";
        public override bool CanShiftPosition => false;
        public override bool IsUsable => false;
        public override bool CanMoveThrough(CellMaterial material) => !(material is SolidCellMaterial);
        public override bool CanMoveThrough(Items.Materials.Material material) => false;
        public static ObjectStaticMovement Static => new ObjectStaticMovement();
        public override bool SurfacePressure => true;

        public override bool IsNativeMovement
            => false;

        protected override MovementLocatorTarget GetNextGeometry(CoreTargetingProcess process,
            Locator locator, CellLocation leadCell, Dictionary<Guid, ICore> exclusions)
        {
            return null;
        }

        public override MovementBase Clone(Creature forCreature, object source) => Static;
    }
}
