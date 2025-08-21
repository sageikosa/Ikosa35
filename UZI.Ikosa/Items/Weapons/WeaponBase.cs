using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons
{
    /// <summary>Summary description for BaseWeapon.</summary>
    [Serializable]
    public abstract class WeaponBase : SlottedItemBase, IWeapon
    {
        #region ctor()
        protected WeaponBase(string name, Size itemSize, string itemSlot)
            : base(name, itemSlot)
        {
            ItemSizer.NaturalSize = itemSize;
        }

        protected WeaponBase(string name, Size itemSize)
            : base(name, ItemSlot.HoldingSlot)
        {
            Sizer.NaturalSize = itemSize;
        }
        #endregion

        #region data
        protected WieldTemplate _WieldTemplate = WieldTemplate.OneHanded;
        #endregion

        /// <summary>Indicates that the weapon is actively wieldable</summary>
        public abstract bool IsActive { get; }

        public virtual bool IsSunderable => true;

        /// <summary>Weapon proficiency type currently needed.  Value may depend on how the weapon is wielded.</summary>
        public virtual WeaponProficiencyType ProficiencyType { get; protected set; }

        public virtual bool ProvokesMelee => false;
        public virtual bool ProvokesTarget => false;

        public abstract bool IsProficiencySuitable(Interaction interact);

        /// <summary>Can be used by derived classes to generate weapon heads bound to the natural attack type</summary>
        protected WeaponBoundHead<WpnType> GetWeaponHead<WpnType>() where WpnType : WeaponBase
            => GetWeaponHead<WpnType>(true);

        #region protected override void OnPreSetItemSlot(ref ItemSlot slotA, ref ItemSlot slotB)
        /// <summary>Ensure that if main slot is preferred for main hands</summary>
        protected override void OnPreSetItemSlot(ref ItemSlot slotA, ref ItemSlot slotB)
        {
            if ((slotB != null) && (slotB.SubType != null)
                && slotB.SubType.Equals(@"Main", StringComparison.OrdinalIgnoreCase))
            {
                if ((slotA.SubType != null) && !slotA.SubType.Equals(@"Main", StringComparison.OrdinalIgnoreCase))
                {
                    // ensure that slotA is the "Main" slot if either are "Main"
                    var _swap = slotB;
                    slotB = slotA;
                    slotA = _swap;
                }
            }
            base.OnPreSetItemSlot(ref slotA, ref slotB);
        }
        #endregion

        #region protected WeaponBoundHead<WpnType> GetWeaponHead<WpnType>(bool main) where WpnType : WeaponBase
        /// <summary>Can be used by derived classes to generate weapon heads bound to the natural attack type</summary>
        protected WeaponBoundHead<WpnType> GetWeaponHead<WpnType>(bool main) where WpnType : WeaponBase
        {
            var _attrList = (WeaponHeadAttribute[])typeof(WpnType).GetCustomAttributes(typeof(WeaponHeadAttribute), true);
            foreach (var _attr in _attrList)
            {
                if (main == _attr.Main)
                {
                    return new WeaponBoundHead<WpnType>(this, _attr.MediumDamage, _attr.DamageTypes, _attr.CriticalLow,
                        _attr.CriticalMultiplier, _attr.HeadMaterial, _attr.Lethality);
                }
            }
            return null;
        }
        #endregion

        /// <summary>Returns the allowable wield mode of the weapon</summary>
        public virtual WieldTemplate WieldTemplate => _WieldTemplate;

        #region public virtual WieldTemplate GetWieldTemplate()
        /// <summary>WieldTemplate needed for wield for the creature</summary>
        public virtual WieldTemplate GetWieldTemplate()
        {
            switch (WieldTemplate)
            {
                case WieldTemplate.Unarmed:
                    return WieldTemplate.Unarmed;
            }

            var _critterSize = CreaturePossessor?.Sizer.Size.Order ?? ItemSizer.EffectiveCreatureSize.Order;
            var _itemSize = ItemSizer.EffectiveCreatureSize.Order;
            var _diff = _itemSize - _critterSize;
            switch (WieldTemplate)
            {
                case WieldTemplate.Light:
                    if (_diff < 0)
                    {
                        return WieldTemplate.TooSmall;
                    }

                    switch (_diff)
                    {
                        case 0:
                            return WieldTemplate.Light;
                        case 1:
                            return WieldTemplate.OneHanded;
                        case 2:
                            return WieldTemplate.TwoHanded;
                        default:
                            return WieldTemplate.TooBig;
                    }

                case WieldTemplate.OneHanded:
                    if (_diff < -1)
                    {
                        return WieldTemplate.TooSmall;
                    }

                    switch (_diff)
                    {
                        case -1:
                            return WieldTemplate.Light;
                        case 0:
                            return WieldTemplate.OneHanded;
                        case 1:
                            return WieldTemplate.TwoHanded;
                        default:
                            return WieldTemplate.TooBig;
                    }

                case WieldTemplate.TwoHanded:
                    if (_diff < -2)
                    {
                        return WieldTemplate.TooSmall;
                    }

                    switch (_diff)
                    {
                        case -2:
                            return WieldTemplate.Light;
                        case -1:
                            return WieldTemplate.OneHanded;
                        case 0:
                            return WieldTemplate.TwoHanded;
                        default:
                            return WieldTemplate.TooBig;
                    }

                case WieldTemplate.Double:
                default:
                    if (_diff < -1)
                    {
                        return WieldTemplate.TooSmall;
                    }

                    switch (_diff)
                    {
                        case -1:
                        case 0:
                            return WieldTemplate.Double;
                        default:
                            return WieldTemplate.TooBig;
                    }

            }
        }
        #endregion

        public abstract IEnumerable<AttackActionBase> WeaponStrikes();

        /// <summary>True if currently treatable as a light weapon</summary>
        public abstract bool IsLightWeapon { get; }
    };
}
