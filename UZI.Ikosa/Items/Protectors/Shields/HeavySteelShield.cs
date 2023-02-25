using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Items.Shields
{
    [
        WeaponHead(@"1d4", DamageType.Bludgeoning, 20, 2, typeof(Materials.SteelMaterial), Lethality.NormallyLethal),
        ItemInfo(@"Shield, Heavy Steel", @"AR:+2 Check:-2 SpellFail:15%", @"heavy_steel_shield"),
        Serializable
    ]
    public class HeavySteelShield : ShieldBase, IMeleeWeapon
    {
        public HeavySteelShield()
            : base("Shield, Heavy Steel", false, 2, -2, 15, false)
        {
            Init(20m, 15d, Materials.SteelMaterial.Static, 20);
            GetHead();
            GetDisabler();
        }

        private IWeaponHead GetHead()
            => _MainHead ??= new WeaponBoundHead<LightWoodenShield>(this, @"1d3", DamageType.Bludgeoning, 20, 2,
                Materials.WoodMaterial.Static, Lethality.NormallyLethal);

        private ShieldDisabler GetDisabler()
            => _Disabler ??= new ShieldDisabler(this);

        #region data
        protected IWeaponHead _MainHead;
        protected ShieldDisabler _Disabler;
        #endregion

        #region protected override void OnSetItemSlot()
        protected override void OnSetItemSlot()
        {
            // add item slot to creature
            if (IsActive)
            {
                // deltas are safe to attempt to add multiple times
                foreach (var _head in AllHeads)
                {
                    _head.CriticalRangeFactor.Deltas.Add(CreaturePossessor.CriticalRangeFactor);
                    _head.CriticalDamageFactor.Deltas.Add(CreaturePossessor.CriticalDamageFactor);
                }
                CreaturePossessor.AddAdjunct(GetDisabler());
            }

            base.OnSetItemSlot();

            if (IsActive)
            {
                this.CreateStrikeZone();
            }
        }
        #endregion

        #region protected override void OnClearSlots(ItemSlot slotA, ItemSlot slotB)
        protected override void OnClearSlots(ItemSlot slotA, ItemSlot slotB)
        {
            foreach (var _head in AllHeads)
            {
                _head.CriticalRangeFactor.Deltas.Remove(CreaturePossessor.CriticalRangeFactor);
                _head.CriticalDamageFactor.Deltas.Remove(CreaturePossessor.CriticalDamageFactor);
            }
            _Disabler?.Eject();
            this.RemoveStrikeZone();

            base.OnClearSlots(slotA, slotB);
        }
        #endregion

        public override int OpposedDelta => 4;

        protected override string ClassIconKey => @"heavy_steel_shield";

        public Geometry GetStrikeZone(bool snapshot = false)
            => this.CreateStrikeGeometry(snapshot);

        public IEnumerable<AttackActionBase> WeaponStrikes()
        {
            yield return new MeleeStrike(MainHead, AttackImpact.Penetrating, @"101");
            yield return new Disarm(MainHead, @"102");
            yield return new SunderWieldedItem(MainHead, @"103");
            yield break;
        }

        #region public override IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        public override IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            foreach (var _act in base.GetActions(budget))
            {
                yield return _act;
            }
            if (IsActive)
            {
                var _budget = budget as LocalActionBudget;

                // TODO: special weapon abilities

                if (_budget.CanPerformRegular)
                {
                    foreach (var _strike in WeaponStrikes())
                        yield return new RegularAttack(_strike);

                    // probe
                    yield return new Probe(MainHead, new ActionTime(TimeType.Regular), @"901");
                }
            }
            yield break;
        }
        #endregion

        public bool IsProficiencySuitable(Interaction interact)
            => true;

        public WieldTemplate GetWieldTemplate()
            => WieldTemplate.OneHanded;

        public virtual IEnumerable<IWeaponHead> AllHeads
        {
            get
            {
                yield return GetHead();
                yield break;
            }
        }

        public bool OpportunisticAttacks => true;
        public bool IsFinessable => false;
        public IWeaponHead MainHead => GetHead();
        public int HeadCount => 1;
        public bool IsReachWeapon => false;
        public bool HasPenaltyUsingLethalChoice => true;
        public WeaponProficiencyType ProficiencyType => WeaponProficiencyType.Martial;
        public bool IsActive => (_MainSlot != null);
        public bool IsLightWeapon => false;
        public bool IsSunderable => true;
        public bool ProvokesMelee => false;
        public bool ProvokesTarget => false;
    }
}
