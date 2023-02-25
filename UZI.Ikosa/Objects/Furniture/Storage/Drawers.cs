using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class Drawers : Furnishing, IAlterLocalLink
    {
        public Drawers(Items.Materials.Material material, int number)
            : base(nameof(Drawers), material)
        {
            _Number = number;
            for (var _dx = 0; _dx < number; _dx++)
            {
                var _name = number > 1 ? $@"Drawer {_dx + 1}" : @"Drawer";
                var _container = new ContainerObject($@"{_name} storage", material, true, true)
                {
                    MaxStructurePoints = 10,
                    MaximumLoadWeight = 100,
                    TareWeight = 0
                };

                var _drawer = new Drawer(_name, material, _container, true, _dx + 1);
                var _ccha = new CloseableContainerBinder(this);
                _drawer.AddAdjunct(_ccha);
                _drawer.AddAdjunct(new ConnectedSides(SideIndex.Front));
                ObjectSizer.Containers.Add(_drawer.Container);
            }
        }

        private int _Number;

        public override bool IsUprightAllowed => true;

        public override object Clone()
        {
            var _clone = new Drawers(ObjectMaterial, _Number);
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

        public override IEnumerable<string> IconKeys
        {
            get
            {
                // provide any overrides
                foreach (var _iKey in IconKeyAdjunct.GetIconKeys(this))
                    yield return _iKey;

                // material class combination
                yield return $@"{ObjectMaterial?.Name}_{ClassIconKey}_{_Number}";

                // ... and then the class key
                yield return ClassIconKey;
                yield break;
            }
        }

        protected override string ClassIconKey
            => nameof(Drawers);
    }
}
