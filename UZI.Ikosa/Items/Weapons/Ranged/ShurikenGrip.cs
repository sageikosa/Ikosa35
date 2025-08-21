using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Core.Dice;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Interactions.Action;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Items.Weapons.Ranged
{
    [Serializable]
    [WeaponHead(@"1d2", DamageType.Piercing, 20, 2, typeof(Materials.SteelMaterial), Lethality.NormallyLethal)]
    [ItemInfo(@"Shuriken", @"Exotic Ranged 1d2 (x2) Piercing (Throw 10')", @"shuriken")]
    public class ShurikenGrip : ProjectileWeaponBase, IActionProvider, IMonitorChange<Size>, IExoticWeapon
    {
        #region ctor(Creature)
        public ShurikenGrip(Creature critter, Shuriken shuriken)
            : base(@"Shuriken Grip", 10, Size.Fine)
        {
            _Ammo = new Shuriken[] { shuriken.Clone() as Shuriken };
            Possessor = critter;
            Setup();
        }

        private void Setup()
        {
            ItemMaterial = Materials.HideMaterial.Static;
            ProficiencyType = WeaponProficiencyType.Exotic;
            Price.CorePrice = 0m;
            MaxStructurePoints.BaseValue = 1;
            BaseWeight = 0d;
        }
        #endregion

        private Shuriken[] _Ammo;

        public Shuriken[] Ammunition { get => _Ammo; set => _Ammo = value; }

        public override bool UsesStrengthDamage => true;
        public override bool TakesStrengthDamagePenalty => true;
        public override bool IsActive => (MainSlot != null);
        public override bool IsTransferrable => false;
        public bool IsLoaded => (_Ammo?.Length ?? 0) > 0;
        public bool IsOverloaded => (_Ammo?.Length ?? 0) > 1;

        /// <summary>When in use, uses two hands, even if only 1 is slotted (FALSE)</summary>
        public override bool UsesTwoHands => false;

        #region public override IEnumerable<DamageRollPrerequisite> BaseDamageRollers(Interaction workSet, string keyFix, int minGroup)
        public override IEnumerable<DamageRollPrerequisite> BaseDamageRollers(Interaction workSet, string keyFix, int minGroup)
        {
            var _atk = (workSet.InteractData as AttackData);
            var _nonLethal = (_atk == null ? false : _atk.IsNonLethal);

            // weapon base damage
            yield return new DamageRollPrerequisite(this, workSet, $@"{keyFix}Weapon", Name,
                WeaponDamageRollers.DiceLookup[MediumDamageRollString][ItemSizer.EffectiveCreatureSize.Order],
                false, _nonLethal, @"Weapon", minGroup);

            // creature-based damage bonuses
            if (CreaturePossessor != null)
            {
                yield return new DamageRollPrerequisite(typeof(Creature), workSet, $@"{keyFix}Creature", @"Creature",
                    new ConstantRoller(CreaturePossessor.ExtraWeaponDamage.QualifiedValue(workSet)),
                    false, _nonLethal, @"Creature", minGroup);
            }

            yield break;
        }
        #endregion

        // IActionProvider
        #region public override IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        public override IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            if (IsActive)
            {
                var _budget = budget as LocalActionBudget;

                // find strikes that can be performed based on available ammunition
                if (_budget.CanPerformRegular)
                {
                    foreach (var _strk in WeaponStrikes())
                    {
                        yield return new RegularAttack(_strk);
                    }
                }

                yield return new DropShuriken(this, @"201");
                yield return new StoreShuriken(this, @"202");
                yield return new SwapShuriken(this, @"203");
            }
            yield break;
        }
        #endregion

        public override bool IsProficiencySuitable(Interaction interact)
            => (interact.InteractData is RangedAttackData);

        public override IEnumerable<AttackActionBase> WeaponStrikes()
        {
            if (IsLoaded && !IsOverloaded)
            {
                var _rAmmo = new RangedAmmunition(this, Ammunition[0]);
                yield return new ShurikenStrike(_rAmmo, this, @"101");
            }
            yield break;
        }

        protected override IWeaponHead GetProxyHead()
            => GetWeaponHead<ShurikenGrip>();

        protected override string ClassIconKey => @"shuriken";

        // item slots

        protected override void OnSetItemSlot()
        {
            // hook size monitor
            CreaturePossessor.Sizer.AddChangeMonitor(this);
            base.OnSetItemSlot();
        }

        protected override void OnClearSlots(ItemSlot slotA, ItemSlot slotB)
        {
            // un-hook size monitor
            CreaturePossessor.Sizer.RemoveChangeMonitor(this);

            if (Ammunition?.Any() ?? false)
            {
                // if cleared with a shuriken still in hand, drop shuriken
                foreach (var _ammo in Ammunition)
                {
                    Drop.DoDrop(CreaturePossessor, _ammo.ToAmmunitionBundle(@"Shuriken"), this, true);
                }
            }
            base.OnClearSlots(slotA, slotB);
        }

        #region IMonitorChange<Size> Members

        protected virtual void OnSizeChanged(Size oldValue, Size newValue)
        {
            // weapon always sized for the creature
            ItemSizer.ExpectedCreatureSize = newValue;
        }

        public void PreTestChange(object sender, AbortableChangeEventArgs<Size> args)
        {
        }

        public void PreValueChanged(object sender, ChangeValueEventArgs<Size> args)
        {
        }

        public void ValueChanged(object sender, ChangeValueEventArgs<Size> args)
        {
            OnSizeChanged(args.OldValue, args.NewValue);
        }

        #endregion

        public override Info GetInfo(CoreActor actor, bool baseValues)
        {
            var _info = ToInfo<LoadableProjectileWeaponInfo>(actor, baseValues);
            return _info;
        }

        public override Info MergeConnectedInfos(Info fetchedInfo, CoreActor actor)
        {
            if (fetchedInfo is LoadableProjectileWeaponInfo _loadable)
            {
                _loadable.LoadedAmmunition = _Ammo?.Select(_a => GetInfoData.GetInfoFeedback(_a, actor)).ToList() ?? [];
            }
            return fetchedInfo;
        }
    }
}
