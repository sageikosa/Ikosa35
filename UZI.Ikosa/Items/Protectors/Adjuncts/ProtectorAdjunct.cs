using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using Uzi.Ikosa.Adjuncts;
using Uzi.Core;
using Uzi.Ikosa.Items.Armor;
using Uzi.Ikosa.Items.Shields;

namespace Uzi.Ikosa.Items
{
    [Serializable]
    public abstract class ProtectorAdjunct : SlotConnectedAugmentation, IRequiresEnhancement, IIdentification,
        IEnhancementCost, IAugmentationCost
    {
        #region construction
        protected ProtectorAdjunct(object source, int enhanceValue, decimal specialCost)
            : base(source, true)
        {
            _EnhanceVal = new Delta(enhanceValue, this);
            _SpecialCost = specialCost;
        }
        #endregion

        #region private data
        private Delta _EnhanceVal;
        private decimal _SpecialCost;
        #endregion

        public static decimal SkillBoostCost(int amount)
        {
            decimal _limit = (amount < 1 ? 1 : amount > 15 ? 15 : amount);
            return _limit * _limit * 150m;
        }

        protected IProtectorItem Protector
            => Anchor as IProtectorItem;

        public Delta TrackedDelta => _EnhanceVal;
        public decimal StandardCost => _SpecialCost;

        /// <summary>True if the item must be enhanced before an independent adjunct can be anchored</summary>
        public virtual bool RequiresEnhancement
            => true;

        /// <summary>True if the adjunct can be used on armor</summary>
        public virtual bool CanUseOnArmor
            => true;

        /// <summary>True if the adjunct can be used on a shield</summary>
        public virtual bool CanUseOnShield
            => true;

        #region public override bool CanAnchor(IAdjunctable newAnchor)
        public override bool CanAnchor(IAdjunctable newAnchor)
        {
            if (base.CanAnchor(newAnchor) && (newAnchor is IProtectorItem))
            {
                if (!CanUseOnArmor && newAnchor is IArmor)
                {
                    return false;
                }

                if (!CanUseOnShield && newAnchor is IShield)
                {
                    return false;
                }

                return true;
            }
            return false;
        }
        #endregion

        // FORTIFIED 25/50/75/100
        // DAMAGE REDUCER 5/Magic

        public abstract IEnumerable<Info> IdentificationInfos { get; }
    }
}
