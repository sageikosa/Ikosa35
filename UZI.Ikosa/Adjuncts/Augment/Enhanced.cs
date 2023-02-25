using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Deltas;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Advancement.CharacterClasses;
using Uzi.Ikosa.Items.Weapons.Ranged;

namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>Item material enhancement</summary>
    [Serializable]
    public class Enhanced : Adjunct, IIdentification, IEnhancementCost
    {
        #region construction
        public Enhanced(object source, int bonusValue)
            : base(source)
        {
            _TDelta = new Delta(bonusValue, typeof(Enhancement));
            _Delta = new Delta(bonusValue, typeof(Enhancement));
            _HDelta = new Delta(bonusValue * 2, typeof(Enhancement));
            _SPDelta = new Delta(bonusValue * 10, typeof(Enhancement));
        }
        #endregion

        #region private data
        private Delta _TDelta;
        private Delta _Delta;
        private Delta _HDelta;
        private Delta _SPDelta;
        #endregion

        public Delta Delta => _Delta;
        public Delta HardnessDelta => _HDelta;
        public Delta StructureDelta => _SPDelta;

        #region public override bool CanUnAnchor()
        public override bool CanUnAnchor()
        {

            if ((from _aug in Anchor.Adjuncts.OfType<MagicAugment>()
                 where (_aug.Augmentation != this) && (_aug.Augmentation is Enhanced)
                 select _aug).Any())
            {
                // if there is any other pure enhancement augmentations, we can always afford to lose one
                return true;
            }
            if ((from _aug in Anchor.Adjuncts.OfType<MagicAugment>()
                 where (_aug.Augmentation is IRequiresEnhancement)
                 let _req = _aug.Augmentation as IRequiresEnhancement
                 where _req.RequiresEnhancement
                 select _req).Any())
            {
                // otherwise, we cannot lose one if there are other adjuncts that require enhancement
                return false;
            }

            return base.CanUnAnchor();
        }
        #endregion

        #region protected override void OnActivate(object source)
        protected override void OnActivate(object source)
        {
            // weapon/weaponhead enhancements
            if (Anchor is IAttackSource _atkSrc)
            {
                _atkSrc.AttackBonus.Deltas.Add(Delta);
            }
            if (Anchor is IDamageSource _dmgSrc)
            {
                _dmgSrc.DamageBonus.Deltas.Add(Delta);
            }

            if (Anchor is IAmmunitionBase _ammo)
            {
                _ammo.Hardness.Deltas.Add(HardnessDelta);
                _ammo.MaxStructurePoints.Deltas.Add(StructureDelta);
            }
            else
            {
                if (Anchor is IWeaponHead _head)
                {
                    _head.ContainingWeapon.Hardness.Deltas.Add(HardnessDelta);
                    _head.ContainingWeapon.MaxStructurePoints.Deltas.Add(StructureDelta);
                }
                else
                {
                    if (Anchor is WeaponBase _wpn)
                    {
                        _wpn.Hardness.Deltas.Add(HardnessDelta);
                        _wpn.MaxStructurePoints.Deltas.Add(StructureDelta);
                    }
                    else
                    {
                        if (Anchor is IProtectorItem _protector)
                        {
                            // protection enhancement
                            _protector.ProtectionBonus.Deltas.Add(Delta);
                            _protector.Hardness.Deltas.Add(HardnessDelta);
                            _protector.MaxStructurePoints.Deltas.Add(StructureDelta);
                        }
                    }
                }
            }
            base.OnActivate(source);
        }
        #endregion

        protected override void OnDeactivate(object source)
        {
            // NOTE: does not destroy the delta, can be reused
            Delta.DoTerminate();
            HardnessDelta.DoTerminate();
            StructureDelta.DoTerminate();
            base.OnDeactivate(source);
        }

        public override object Clone()
            => new Enhanced(Source, Delta.Value);

        #region IIdentification Members

        public IEnumerable<Info> IdentificationInfos
        {
            get
            {
                yield return new Info { Message = string.Format(@"Enhanced +{0}", Delta.Value) };
                yield break;
            }
        }

        #endregion

        public Delta TrackedDelta => _TDelta;

        public override bool Equals(Adjunct other)
        {
            if (other is Enhanced _enhanced)
            {
                return _enhanced.Delta.Value == Delta.Value;
            }
            return false;
        }
    }

    public static class EnhancedExtension
    {
        /// <summary>True if Enhanced augmentation is attached to object</summary>
        public static bool IsEnhanced(this IAdjunctSet self)
        {
            return self.Adjuncts.OfType<MagicAugment>().Any(_aug => _aug.Augmentation is Enhanced);
        }

        /// <summary>True if an active, Enhanced augmentation is attached to object</summary>
        public static bool IsEnhancedActive(this IAdjunctSet self)
            => self.Adjuncts.OfType<MagicAugment>()
            .Any(_aug => _aug.IsActive && (_aug.Augmentation is Enhanced) && _aug.Augmentation.IsActive);

        /// <summary>Get a MagicAugment for item enhancement</summary>
        /// <param name="bonus"></param>
        /// <param name="magicStyle">Evocation (weapons) or Abjuration (protectors)</param>
        public static MagicAugment GetEnhancedAugment(int bonus, MagicStyle magicStyle)
        {
            // the source for this particular augmentation instance (based on the power)
            var _source = MagicAugmentationPowerSource.CreateItemPowerSource(
                MagicType.Divine, Alignment.TrueNeutral, bonus * 3, typeof(Cleric),
                bonus, magicStyle, @"Enhanced", bonus.ToString(), typeof(Enhanced).FullName);

            // the effect of the augmentation enhancement
            var _enhanced = new Enhanced(typeof(Enhanced), bonus);

            // the augmentation (based on the source)
            return new MagicAugment(_source, _enhanced);
        }
    }

    public interface IEnhancementTracker
    {
        ConstDeltable TotalEnhancement { get; }
        int ListedEnhancement { get; }
    }

    public interface IEnhancementCost
    {
        /// <summary>Return 0 if there is no tracked delta (that is, this enhancement doesn't contribute to tracking</summary>
        Delta TrackedDelta { get; }
    }

    public interface IRequiresEnhancement
    {
        bool RequiresEnhancement { get; }
    }
}
