using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public abstract class SolidFurnishing : Furnishing, IAlterLocalLink
    {
        protected SolidFurnishing(string name, Items.Materials.Material material)
            : base(name, material)
        {
        }

        protected abstract SolidFurnishing GetClone();

        public override bool IsUprightAllowed => true;

        public override object Clone()
        {
            var _clone = GetClone();
            _clone.CopyFrom(this);
            return _clone;
        }

        public override bool BlocksSpread(AnchorFace transitFace, ICellLocation sourceCell, ICellLocation targetCell)
        {
            if (BlocksEffect)
            {
                // haven't really dealt with spreads yet
            }
            return false;
        }

        public override bool CanBlockSpread => BlocksEffect;

        public override bool IsCostlyMovement(MovementBase movement, IGeometricRegion region)
        {
            // NOTE: hinders move and OpensTowards should take care of this...
            return false;
        }

        public override bool IsHardSnap(AnchorFace face)
            => true;

        protected override bool IsCoveredFace(AnchorFace face)
            => true;

        // IAlterLink

        #region public bool CanAlterLink(LocalLink link)
        /// <summary>Snapped and intersecting the plane defining the surface</summary>
        public bool CanAlterLink(LocalLink link)
        {
            var _region = this.GetLocated()?.Locator.GeometricRegion;
            var _face = link.GetAnchorFaceForRegion(_region);

            // most likely can alter...
            return Orientation.IsFaceSnapped(_face);
        }
        #endregion

        public double AllowLightFactor(LocalLink link)
            => 1 - GetCoverage(link);

        public int GetExtraSoundDifficulty(LocalLink link)
            => CanAlterLink(link)
            ? Convert.ToInt32(GetCoverage(link) * ExtraSoundDifficulty.EffectiveValue)
            : 0;

    }
}
