using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Items.Shields
{
    [
        WeaponHead(@"1d3", DamageType.Bludgeoning, 20, 2, typeof(Materials.SteelMaterial), Lethality.NormallyLethal),
        ItemInfo(@"Shield, Light Steel", @"AR:+1 Check:-1 SpellFail:5%", @"light_steel_shield"),
        Serializable
    ]
    /// <summary>
    /// Shield that lets you use the hand, but not for wielding.
    /// </summary>
    public class LightSteelShield : ShieldBase, IMeleeWeapon
    {
        public LightSteelShield()
            : base("Shield, Light Steel", false, 1, -1, 5, true)
        {
            Init(9m, 6d, Materials.SteelMaterial.Static, 10);
            GetHead();
            GetDisabler();
        }

        private IWeaponHead GetHead()
            => _MainHead ??= new WeaponBoundHead<LightWoodenShield>(this, @"1d3", DamageType.Bludgeoning, 20, 2,
                    Materials.WoodMaterial.Static, Lethality.NormallyLethal);

        private ShieldDisabler GetDisabler()
            => _Disabler ??= new ShieldDisabler(this);

        #region data
        private ItemSlot _LastMain;
        private HoldingSlot _FreeHand;
        protected IWeaponHead _MainHead;
        protected ShieldDisabler _Disabler;
        #endregion

        #region protected override void OnSetItemSlot()
        protected override void OnSetItemSlot()
        {
            // add item slot to creature
            if (IsActive)
            {
                if (_LastMain != MainSlot)
                {
                    ReleaseFreeHand();
                }
                if (_FreeHand == null)
                {
                    _LastMain = MainSlot;
                    _FreeHand = new HoldingSlot(this, MainSlot.SubType, true);
                    CreaturePossessor.Body.ItemSlots.Add(_FreeHand);
                }

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
            var _reslot = GetReslotInfo(_FreeHand, slotA);
            foreach (var _head in AllHeads)
            {
                _head.CriticalRangeFactor.Deltas.Remove(CreaturePossessor.CriticalRangeFactor);
                _head.CriticalDamageFactor.Deltas.Remove(CreaturePossessor.CriticalDamageFactor);
            }
            _Disabler?.Eject();
            this.RemoveStrikeZone();

            base.OnClearSlots(slotA, slotB);

            ReslotItem(_reslot);
            ReleaseFreeHand();
        }
        #endregion

        private void ReleaseFreeHand()
        {
            // remove freehand item slot
            if (_FreeHand != null)
            {
                CreaturePossessor.Actions.Filters.Remove(_FreeHand);
                CreaturePossessor.Body.ItemSlots.Remove(_FreeHand);
            }
            _FreeHand = null;
        }

        #region IActionFilter Members

        public override bool SuppressAction(object source, CoreActionBudget budget, CoreAction action)
        {
            if (action is ISupplyAttackAction _atk)
            {
                if (_atk.Attack.Weapon == _FreeHand.SlottedItem)
                    return true;

                // projectile, needing two hands
                if ((_atk.Attack.Weapon is IProjectileWeapon _projectile)
                    && _projectile.UsesTwoHands
                    && (_projectile is ISlottedItem _slotted))
                {
                    // one slot is not filled
                    if ((_slotted.MainSlot == null) || (_slotted.SecondarySlot == null))
                    {
                        // and there are no other empty slots besides this hand
                        if (CreaturePossessor.Body.ItemSlots.GetFreeHand(_FreeHand) == null)
                            return true;
                    }
                }
            }
            return base.SuppressAction(source, budget, action);
        }

        #endregion

        protected override string ClassIconKey => @"light_steel_shield";

        public Geometry GetStrikeZone(bool snapshot = false)
            => this.CreateStrikeGeometry(snapshot);

        public IEnumerable<AttackActionBase> WeaponStrikes()
        {
            yield return new MeleeStrike(MainHead, Contracts.AttackImpact.Penetrating, @"101");
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
            => WieldTemplate.Light;

        public virtual IEnumerable<IWeaponHead> AllHeads
        {
            get
            {
                yield return GetHead();
                yield break;
            }
        }

        public bool OpportunisticAttacks => true;
        public bool IsFinessable => true;
        public IWeaponHead MainHead => GetHead();
        public int HeadCount => 1;
        public bool IsReachWeapon => false;
        public bool HasPenaltyUsingLethalChoice => true;
        public WeaponProficiencyType ProficiencyType => WeaponProficiencyType.Martial;
        public bool IsActive => (_MainSlot != null);
        public bool IsLightWeapon => true;
        public bool IsSunderable => true;
        public bool ProvokesMelee => false;
        public bool ProvokesTarget => false;
    }
}
