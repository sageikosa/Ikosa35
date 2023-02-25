using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Items.Weapons.Ranged;

namespace Uzi.Ikosa.Items.Weapons
{
    [Serializable]
    public abstract class WeaponSpecialAbility : Adjunct, IRequiresEnhancement, IIdentification,
        IEnhancementCost, IAugmentationCost
    {
        #region construction
        protected WeaponSpecialAbility(object source, int enhancementVal, decimal specialCost)
            : base(source)
        {
            _Tracked = new Delta(enhancementVal, this);
            _SpecialCost = specialCost;
        }
        #endregion

        #region private data
        private Delta _Tracked;
        private decimal _SpecialCost;
        #endregion

        public virtual string Name
            => GetType().Name;

        public Delta TrackedDelta => _Tracked;
        public decimal StandardCost => _SpecialCost;

        /// <summary>True if the item must be enhanced before an independent adjunct can be anchored</summary>
        public virtual bool RequiresEnhancement
            => true;

        /// <summary>True if the ability can be added to ranged weapons (projectile weapons, throwable weapons and ammo).  Default=FALSE</summary>
        public virtual bool CanUseOnRanged
            => false;

        /// <summary>True if the ability can be added to melee weapons.  Default=TRUE</summary>
        public virtual bool CanUseOnMelee
            => true;

        // TODO: CanUseOnAmmo...CanUseOnLauncher

        public IAttackSource AttackSource
            => Anchor as IAttackSource;

        /// <summary>Weapon without a head is a projectile and a head without a weapon is ammunition</summary>
        protected virtual bool OnCanBind(IWeapon weapon, IWeaponHead head)
            => true;

        #region private bool CostAllowed(IItemBase item)
        private bool CostAllowed(IItemBase item, IEnhancementTracker tracker)
        {
            if (TrackedDelta.Value > 0)
            {
                if (tracker != null)
                {
                    if (tracker.TotalEnhancement.EffectiveValue + TrackedDelta.Value > 10)
                        return false;
                }
                else
                {
                    return false;
                }
            }
            if (StandardCost > 0)
            {
                if (item.Price.BasePrice + StandardCost > 200000)
                    return false;
            }
            return true;
        }
        #endregion

        #region public override bool CanAnchor(IEffectHolder newAnchor)
        public override bool CanAnchor(IAdjunctable newAnchor)
        {
            if (base.CanAnchor(newAnchor) && (newAnchor is IAttackSource))
            {
                var _atkSource = newAnchor as IAttackSource;

                // get weapon base
                var _head = _atkSource as IWeaponHead;
                IWeapon _wpn = (_head == null)
                    // attack source is a projectile weapon
                    ? _atkSource as WeaponBase
                    // weapon head's containing weapon (null for ammunitionBase)
                    : _head.ContainingWeapon;

                // weapon without a head is a projectile
                // head without a weapon is ammunition
                Type _wpnType = _wpn?.GetType();
                Type _headType = _head?.GetType();

                if (CanUseOnRanged)
                {
                    if (_wpnType != null)
                    {
                        if ((_wpn is IMeleeWeapon _melee) && _melee.IsThrowable())
                        {
                            // throwable melee weapon
                            if (!CostAllowed(_wpn, _head))
                                return false;
                            return OnCanBind(_wpn, _head);
                        }
                        if (typeof(IProjectileWeapon).IsAssignableFrom(_wpnType))
                        {
                            if (!CostAllowed(_wpn, _wpn as IProjectileWeapon))
                                return false;
                            return OnCanBind(_wpn, _head);
                        }
                    }
                    if (_headType != null)
                    {
                        if (typeof(AmmunitionBase).IsAssignableFrom(_headType))
                        {
                            if (!CostAllowed(_head as IAmmunitionBase, _head))
                                return false;
                            return OnCanBind(_wpn, _head);
                        }
                    }
                }
                if (CanUseOnMelee)
                {
                    if (_wpnType != null)
                    {
                        if (typeof(IMeleeWeapon).IsAssignableFrom(_wpnType))
                        {
                            if (!CostAllowed(_wpn, _head))
                                return false;
                            return OnCanBind(_wpn, _head);
                        }
                    }
                }
            }
            return false;
        }
        #endregion

        public abstract IEnumerable<Info> IdentificationInfos { get; }

        public bool Affinity => true;
        public string SimilarityKey => null;
    }
}
