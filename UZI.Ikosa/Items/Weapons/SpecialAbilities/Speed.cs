using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Items.Weapons.Ranged;

namespace Uzi.Ikosa.Items.Weapons
{
    /// <summary>Extra attack with weapon</summary>
    [Serializable]
    [MagicAugmentRequirement(7, typeof(CraftMagicArmsAndArmorFeat))]
    public class Speed : WeaponSpecialAbility
    {
        /// <summary>Extra attack with weapon</summary>
        public Speed()
            : base(typeof(Speed), 3, 0)
        {
            _Activation = new SpeedWeaponSlotAugmentation(this);
        }

        #region state
        private SpeedWeaponSlotAugmentation _Activation;
        #endregion

        public override IEnumerable<Info> IdentificationInfos
        {
            get
            {
                yield return new Info { Message = @"Speed: 1 extra attack on full attack" };
                yield break;
            }
        }

        protected override void OnActivate(object source)
        {
            // main purpose is to add the SlotConnectedAugmentation
            if (AttackSource is IWeapon _wpn)
            {
                // add to weapon
                _wpn.AddAdjunct(_Activation);
            }
            else if (Anchor is IWeaponHead _head)
            {
                // add to weapon for weapon head
                _head.ContainingWeapon.AddAdjunct(_Activation);
            }
            base.OnActivate(source);
        }

        protected override void OnDeactivate(object source)
        {
            _Activation.Eject();
            base.OnDeactivate(source);
        }

        public override bool CanUseOnRanged => true;

        public override object Clone()
            => new Speed();
    }

    /// <summary>
    /// Always added to a weapon, even if speed is added to a weapon head.  
    /// Consequently, for double-weapons there may be two speed factories.
    /// </summary>
    [Serializable]
    public class SpeedWeaponSlotAugmentation : SlotConnectedAugmentation
    {
        public SpeedWeaponSlotAugmentation(Speed source)
            : base(source, true)
        {
            _Factory = new SpeedWeaponAttackBudgetFactory(this);
        }

        #region state
        private SpeedWeaponAttackBudgetFactory _Factory;
        #endregion

        /// <summary>True if SpeedSource and attack are same source channel</summary>
        public bool IsAttackCompatible(AttackActionBase attack)
        {
            if (SpeedSource.AttackSource.IsSourceChannel(attack.AttackSource)
                || attack.AttackSource.IsSourceChannel(SpeedSource.AttackSource))
            {
                // either way...
                return true;
            }
            return false;
        }

        public override object Clone()
            => new SpeedWeaponSlotAugmentation(SpeedSource);

        public Speed SpeedSource => Source as Speed;

        protected override void OnSlottedActivate()
        {
            // add speed weapon factory to actor when slotted
            SlottedItem.MainSlot.Creature.AddAdjunct(_Factory);
        }

        protected override void OnSlottedDeActivate()
        {
            // remove speed weapon factory from actor when unslotted
            _Factory?.Eject();
        }
    }

    [Serializable]
    public class SpeedWeaponAttackBudgetFactory : Adjunct, IAttackPotentialFactory
    {
        public SpeedWeaponAttackBudgetFactory(SpeedWeaponSlotAugmentation augment)
            : base(augment)
        {
        }

        public SpeedWeaponSlotAugmentation SpeedWeaponSlotAugmentation
            => Source as SpeedWeaponSlotAugmentation;

        public override object Clone()
            => new SpeedWeaponAttackBudgetFactory(SpeedWeaponSlotAugmentation);

        public IEnumerable<IAttackPotential> GetIAttackPotentials(FullAttackBudget budget)
        {
            if (!budget.Budget.BudgetItems.Items.OfType<ExtraAttackPotential>().Any())
            {
                // do not add if there is already one...only allowed one of these
                yield return new SpeedWeaponAttackPotential(SpeedWeaponSlotAugmentation);
            }
            yield break;
        }
    }

    [Serializable]
    public class SpeedWeaponAttackPotential : IAttackPotential
    {
        public SpeedWeaponAttackPotential(SpeedWeaponSlotAugmentation augmentation)
        {
            _Used = false;
            _Augment = augmentation;
        }

        #region state
        private bool _Used;     // the extra attack was used
        private SpeedWeaponSlotAugmentation _Augment;
        #endregion

        public bool IsUsed => _Used;
        public string Name => @"Extra attack";
        public string Description => @"One extra attack at full attack bonus";
        public object Source => _Augment;

        /// <summary>Extra attack doesn't have any use-specific Deltas</summary>
        public IDelta Delta { get => null; }

        /// <summary>Extra attack doesn't block any other attack</summary>
        public bool BlocksUse(AttackActionBase attack)
            => false;

        /// <summary>Extra attack doesn't block any other budget</summary>
        public bool BlocksUse(IAttackPotential potential)
            => false;

        public bool BlocksUse(AttackActionBase attack, IAttackPotential potential)
        {
            if ((potential is ExtraAttackPotential) 
                && _Augment.IsAttackCompatible(attack))
            {
                // block haste use of this weapon
                return true;
            }
            return false;
        }

        /// <summary>Can only use on own weapon</summary>
        public bool CanUse(AttackActionBase attack)
            => !_Used && _Augment.IsAttackCompatible(attack);

        public bool RegisterUse(AttackActionBase attack)
        {
            if (CanUse(attack))
            {
                _Used = true;
                return true;
            }
            return false;
        }

        /// <summary>True means remove budget after reset</summary>
        public bool Reset()
            => true;

        public void Added(CoreActionBudget budget)
        {
            // nothing other effects
        }

        public void Removed()
        {
            // nothing other effects
        }
    }
}