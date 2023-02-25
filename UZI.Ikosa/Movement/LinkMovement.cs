using Uzi.Ikosa.Tactical;
using Uzi.Core;
using Uzi.Visualize;
using System.Collections.Generic;
using System;
using Uzi.Ikosa.Senses;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Movement
{
    /// <summary>Used to find links between rooms and cells</summary>
    public class LinkMovement : MovementBase
    {
        /// <summary>Used to find links between rooms and cells</summary>
        public readonly static LinkMovement Static = new LinkMovement();

        /// <summary>Used to find links between rooms and cells</summary>
        private LinkMovement()
            : base(0, null, null)
        {
        }

        public override string Name => @"Cell Link Finder";
        public override bool CanShiftPosition => false;
        public override bool IsUsable => false;
        public override bool SurfacePressure => false;
        public override bool CanMoveThrough(CellMaterial material) => !(material is SolidCellMaterial);
        public override bool CanMoveThrough(Items.Materials.Material material) => false;
        protected override MovementLocatorTarget GetNextGeometry(CoreTargetingProcess process,
            Locator locator, CellLocation leadCell, Dictionary<Guid, ICore> exclusions)
            => null;

        public override bool IsNativeMovement 
            => false;

        public override MovementBase Clone(Creature forCreature, object source) => Static;
    }
}