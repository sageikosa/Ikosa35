using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Interactions.Action;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public abstract class HollowFurnishing : Furnishing, IAlterLocalLink, IObjectStateModels
    {
        protected HollowFurnishing(string name, Items.Materials.Material material)
            : base(name, material)
        {
            _LiddedModel = new ObjectStateModelKey(@"Lidded");

            // interior of furnishing...
            var _container = new ContainerObject(@"interior", material, true, true)
            {
                MaxStructurePoints = 1,
                MaximumLoadWeight = 100,
                TareWeight = 0
            };
            _container.BindToObject(this);
        }

        #region data
        private ObjectStateModelKey _LiddedModel;
        #endregion

        public override bool IsUprightAllowed => true;

        // Lid Control

        public bool IsLidded => Anchored.OfType<HollowFurnishingLid>().Any();

        public IEnumerable<ObjectStateModelKey> StateModelKeys 
            => _LiddedModel.ToEnumerable();

        public string LiddedModelKey
        {
            get => _LiddedModel.ModelKey;
            set => _LiddedModel.ModelKey = value;
        }

        #region public override IEnumerable<ICoreObject> Accessible(ICoreObject principal)
        public override IEnumerable<ICoreObject> Accessible(ICoreObject principal)
        {
            // hide any raw container object if lidded
            foreach (var _obj in base.Accessible(principal))
            {
                // no lid, not a container, or the container is not directly bound to the furnishing
                if (!IsLidded
                    || !(_obj is ContainerObject)
                    || !_obj.GetObjectBindings().Any(_b => _b == this))
                {
                    yield return _obj;
                }
            }
            yield break;
        }
        #endregion

        public abstract HollowFurnishingLid CreateLid(Items.Materials.Material objectMaterial);

        #region public override bool CanAnchor(IAdjunctable newAnchor)
        /// <summary>Special handling for lids</summary>
        public override bool CanAcceptAnchor(IAdjunctable newAnchor)
        {
            // mounting a lid?
            if (newAnchor is HollowFurnishingLid _lid)
            {
                // correct type of lid and not lidded?
                if (_lid.IsValidLid(this) && !IsLidded)
                {
                    // confirm via normal tests
                    return base.CanAcceptAnchor(newAnchor);
                }

                // cannot put this lid on
                return false;
            }

            // something else...
            return base.CanAcceptAnchor(newAnchor);
        }
        #endregion

        #region public override bool CanUnAnchor(IAdjunctable existingAnchor)
        public override bool CanEjectAnchor(IAdjunctable existingAnchor)
        {
            if (existingAnchor is ContainerObject)
            {
                // suppress removal of last native container ...
                // ... unless the furnishing is destroyed
                if (Anchored.OfType<ContainerObject>().Count() == 1)
                {
                    return StructurePoints <= 0;
                }
            }

            // default
            return base.CanEjectAnchor(existingAnchor);
        }
        #endregion

        #region public override void Anchor(IAdjunctable newAnchor)
        public override void AcceptAnchor(IAdjunctable newAnchor)
        {
            // this is a double-check, but confirms for this override that it will succeed
            if (CanAcceptAnchor(newAnchor))
            {
                base.AcceptAnchor(newAnchor);
                if (newAnchor is HollowFurnishingLid _lid)
                {
                    // since it was a lid, it must be our normal lid...
                    if (!this.HasAdjunct<OverrideModelKey>())
                    {
                        AddAdjunct(new OverrideModelKey(this, _LiddedModel.ModelKey));
                    }

                    // prevent it from being picked up when acting as a lid
                    _lid.AddAdjunct(new Interactor<BlockInteraction<PickUp>>(this));
                }
            }
        }
        #endregion

        #region public override void UnAnchor(IAdjunctable existingAnchor)
        public override void EjectAnchor(IAdjunctable existingAnchor)
        {
            if (CanEjectAnchor(existingAnchor))
            {
                base.EjectAnchor(existingAnchor);
                if (existingAnchor is HollowFurnishingLid _lid)
                {
                    // removed our lid...
                    Adjuncts.OfType<OverrideModelKey>().FirstOrDefault()?.Eject();

                    // allow it to be picked up when not acting as a lid
                    _lid.Adjuncts.OfType<Interactor<BlockInteraction<PickUp>>>()?
                        .FirstOrDefault(_s => _s.Source == this)?
                        .Eject();
                }
            }
        }
        #endregion

        protected abstract HollowFurnishing GetClone();

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

        protected override AnchorFaceList GetCoveredFaces()
            => AnchorFaceList.All.Remove(Orientation.GetAnchorFaceForSideIndex(SideIndex.Top));

        public override bool IsCostlyMovement(MovementBase movement, IGeometricRegion region)
        {
            // NOTE: hinders move and OpensTowards should take care of this...
            return false;
        }

        // IAlterLink

        #region public bool CanAlterLink(LocalLink link)
        /// <summary>Snapped and intersecting the plane defining the surface</summary>
        public bool CanAlterLink(LocalLink link)
        {
            var _region = this.GetLocated()?.Locator.GeometricRegion;
            var _face = link.GetAnchorFaceForRegion(_region);

            // most likely can alter...
            // TODO: see if lidded...
            return (Orientation.IsFaceSnapped(_face)
                && Orientation.GetAnchorFaceForSideIndex(SideIndex.Top) == _face);
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
