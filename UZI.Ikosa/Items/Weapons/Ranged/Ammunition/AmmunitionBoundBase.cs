using System;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Adjuncts;
using System.Collections.Generic;
using Uzi.Ikosa.Interactions;

namespace Uzi.Ikosa.Items.Weapons.Ranged
{
    [Serializable]
    public abstract class AmmunitionBoundBase<RangedType> : AmmunitionBase
        where RangedType : WeaponBase, IProjectileWeapon
    {
        #region ctor
        protected AmmunitionBoundBase(string dmgRoll, DamageType dmgType, Material headMaterial)
            : base(dmgRoll, dmgType, headMaterial)
        {
        }

        protected AmmunitionBoundBase(string dmgRoll, DamageType dmgType, int criticalLow,
            DeltableQualifiedDelta criticalMultiplier, Material headMaterial)
            : base(dmgRoll, dmgType, criticalLow, criticalMultiplier, headMaterial)
        {
        }

        protected AmmunitionBoundBase(string dmgRoll, DamageType[] dmgTypes, int criticalLow,
            DeltableQualifiedDelta criticalMultiplier, Material headMaterial)
            : base(dmgRoll, dmgTypes, criticalLow, criticalMultiplier, headMaterial)
        {
        }
        #endregion

        public override Type GetProjectileWeaponType()
            => typeof(RangedType);

        protected override void InitInteractionHandlers()
        {
            AddIInteractHandler(new AmmoDropHandler());
            base.InitInteractionHandlers();
        }

        #region public bool IsSameAmmunition(AmmunitionBoundBase<RangedType> ammoType)
        /// <summary>Has same mergeable adjuncts</summary>
        public bool IsSameAmmunition(AmmunitionBoundBase<RangedType> ammoType)
        {
            // material must match
            if ((ammoType?.ItemMaterial.Equals(ItemMaterial) ?? false)
                && ammoType.HeadMaterial.Equals(HeadMaterial))
            {
                // same IDs (must be identified as the same)
                List<Adjunct> _getMerges(AmmunitionBoundBase<RangedType> ammo)
                    => ammo.Adjuncts
                    .Where(_i => _i.MergeID != null)
                    .Distinct()
                    .ToList();

                var _self = _getMerges(this);
                var _inbound = _getMerges(ammoType);
                if (_self.Count != _inbound.Count)
                {
                    // different counts cannot be the same
                    return false;
                }
                if (_self.Any() && !_self.All(_s => _inbound.Any(_i => _s.Equals(_i))))
                {
                    // if anything, then everything must have a match, or else not the same
                    return false;
                }

                // otherwise it's a match
                return true;
            }
            return false;
        }
        #endregion

        #region public void MergeIdentityCreatures(AmmunitionBoundBase<RangedType> ammoType)
        /// <summary>Merges identity creatures if ammoType IsSameAmmunition()</summary>
        public void MergeIdentityCreatures(AmmunitionBoundBase<RangedType> ammoType)
        {
            if (!IsSameAmmunition(ammoType))
            {
                return;
            }

            // same IDs (must be identified as the same)
            Dictionary<Guid, Identity> _getMerges(AmmunitionBoundBase<RangedType> ammo)
                => ammo.Adjuncts.OfType<Identity>()
                .Where(_i => _i.MergeID != null)
                .Distinct()
                .ToDictionary(_i => _i.MergeID ?? Guid.Empty);

            var _self = _getMerges(this);
            var _inbound = _getMerges(ammoType);
            foreach (var _id in _self)
            {
                if (_inbound.ContainsKey(_id.Key))
                {
                    var _from = _inbound[_id.Key];
                    _id.Value.MergeCreatureIDs(_from.CreatureIDs);
                }
            }
        }
        #endregion
    }
}
