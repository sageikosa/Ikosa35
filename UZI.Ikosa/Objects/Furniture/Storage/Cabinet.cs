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
    public class Cabinet : Furnishing, IAlterLocalLink
    {
        public Cabinet(Items.Materials.Material material)
            : base(nameof(Cabinet), material)
        {
            var _container = new ContainerObject($@"{Name} storage", material, true, true)
            {
                MaxStructurePoints = 10,
                MaximumLoadWeight = 100,
                TareWeight = 0
            };

            _Compartment = new CloseableContainerObject($@"{Name} door", material, _container, true, 1);
            var _ccha = new CloseableContainerBinder(this);
            _Compartment.AddAdjunct(_ccha);
            _Compartment.AddAdjunct(new ConnectedSides(SideIndex.Front));
            ObjectSizer.Containers.Add(_Compartment.Container);
        }

        #region data
        private CloseableContainerObject _Compartment;
        #endregion

        public override bool IsUprightAllowed => true;

        public override object Clone()
        {
            var _clone = new Cabinet(ObjectMaterial);
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
            => _Compartment.OpenState.IsClosed
            ? true
            : Orientation.GetAnchorFaceForSideIndex(SideIndex.Front) != face;

        protected override AnchorFaceList GetCoveredFaces()
            => _Compartment.OpenState.IsClosed
            ? AnchorFaceList.All
            : AnchorFaceList.All.Remove(Orientation.GetAnchorFaceForSideIndex(SideIndex.Front));

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
                {
                    yield return _iKey;
                }

                // material class combination
                yield return $@"{ObjectMaterial?.Name}_{ClassIconKey}_1";

                // ... and then the class key
                yield return ClassIconKey;
                yield break;
            }
        }

        protected override string ClassIconKey
            => nameof(Cabinet);
    }
}
