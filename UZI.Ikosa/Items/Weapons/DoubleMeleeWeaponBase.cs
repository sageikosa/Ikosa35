using Uzi.Core.Contracts;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Interactions;

namespace Uzi.Ikosa.Items.Weapons
{
    [Serializable]
    public abstract class DoubleMeleeWeaponBase : WeaponBase, IMeleeWeapon, IActionProvider, IMultiWeaponHead
    {
        #region Construction
        protected DoubleMeleeWeaponBase(string name, bool finessable, Size itemSize)
            : base(name, itemSize)
        {
            _WeaponHeads = new Collection<IWeaponHead>();
            _WieldTemplate = WieldTemplate.Double;
            _Finessable = finessable;
        }
        #endregion

        #region private/protected data
        protected Collection<IWeaponHead> _WeaponHeads;
        protected int _DominantHead;
        protected bool _Finessable;
        private bool _UseAsTwoHanded;
        #endregion

        public bool IsFinessable => _Finessable;
        public IWeaponHead MainHead => _WeaponHeads[_DominantHead] as IWeaponHead;
        public IWeaponHead OffHandHead => _WeaponHeads[1 - _DominantHead] as IWeaponHead;
        public int HeadCount => 2;
        public bool IsReachWeapon => this is IReachWeapon;
        public IMultiWeaponHead Head => this as IMultiWeaponHead;
        public virtual bool HasPenaltyUsingLethalChoice => true;

        public override bool IsTransferrable => true;

        public override bool IsProficiencySuitable(Interaction interact)
            => (interact.InteractData is RangedAttackData)
            ? this.IsThrowable()
            : true;

        #region IMultiWeaponHead Members
        public IWeaponHead this[int index]
        {
            get
            {
                if ((index < 2) && (index >= 0))
                    return _WeaponHeads[index] as IWeaponHead;
                else
                    return null;
            }
        }
        #endregion

        public virtual IEnumerable<IWeaponHead> AllHeads
            => _WeaponHeads.AsEnumerable();

        #region public int MainHeadIndex { get; set; }
        public int MainHeadIndex
        {
            get { return _DominantHead; }
            set
            {
                if ((value >= 0) && (value < HeadCount))
                {
                    _DominantHead = value;
                }
                else
                {
                    _DominantHead = 0;
                }
            }
        }
        #endregion

        /// <summary>Indicates that the weapon is being used as a two-handed weapon, rather than a double weapon, and only the main head can used.</summary>
        public bool UseAsTwoHanded { get { return _UseAsTwoHanded; } set { _UseAsTwoHanded = value; } }

        /// <summary>True if both slots are in use, and not two hand wielding</summary>
        public bool IsDualWielding
            => (_MainSlot != null) && (_SecondarySlot != null) && !UseAsTwoHanded;

        /// <summary>Active if slotted at least on the MainSlot</summary>
        public override bool IsActive
            => MainSlot != null;

        #region protected override void OnSetItemSlot()
        protected override void OnSetItemSlot()
        {
            if (IsActive)
            {
                if (!CreaturePossessor.Actions.Providers.ContainsKey(this))
                {
                    CreaturePossessor.Actions.Providers.Add(this, (IActionProvider)this);
                }
            }

            // slotted adjunct must precede strike zone, or setting cannot be found
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
            if (CreaturePossessor.Actions.Providers.ContainsKey(this))
            {
                CreaturePossessor.Actions.Providers.Remove(this);
            }
            this.RemoveStrikeZone();
            base.OnClearSlots(slotA, slotB);
        }
        #endregion

        #region public override IEnumerable<AttackActionBase> WeaponStrikes()
        public override IEnumerable<AttackActionBase> WeaponStrikes()
        {
            // main head attacks
            yield return new MeleeStrike(MainHead, Contracts.AttackImpact.Penetrating, @"101");
            yield return new Disarm(MainHead,@"102");
            if (MainHead.DamageTypes.Any(_dt => _dt != DamageType.Piercing))
                yield return new SunderWieldedItem(MainHead, @"103");

            // off hand attacks
            if ((SecondarySlot != null) && (!_UseAsTwoHanded))
            {
                yield return new MeleeStrike(OffHandHead, Contracts.AttackImpact.Penetrating, @"201");
                yield return new Disarm(OffHandHead, @"202");
                if (OffHandHead.DamageTypes.Any(_dt => _dt != DamageType.Piercing))
                    yield return new SunderWieldedItem(OffHandHead, @"203");
            }
            yield break;
        }
        #endregion

        #region IActionProvider Members
        public virtual IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            if (IsActive)
            {
                var _budget = budget as LocalActionBudget;
                var _fAtkBudget = FullAttackBudget.GetBudget(budget);

                if (_budget.CanPerformRegular)
                {
                    foreach (var _strike in WeaponStrikes())
                        yield return new RegularAttack(_strike);

                    // probe
                    yield return new Probe(MainHead, new ActionTime(TimeType.Regular), @"901");
                    if ((SecondarySlot != null) && (!_UseAsTwoHanded))
                        yield return new Probe(OffHandHead, new ActionTime(TimeType.Regular), @"902");
                }

                // off hand heads
                if (SecondarySlot != null)
                {
                    if ((_fAtkBudget == null) || !_fAtkBudget.AttackStarted)
                    {
                        // haven't attacked, so still have time to switch wield modes
                        yield return new DoubleWeaponMainHeadChoice(this);
                        yield return new DoubleWeaponTwoHandedChoice(this);
                    }
                }
            }
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
            => GetInfoData.GetInfoFeedback(this, budget.Actor);
        #endregion

        #region IMeleeWeapon Members

        public Geometry GetStrikeZone(bool snapshot = false) => this.CreateStrikeGeometry(snapshot);
        public virtual bool OpportunisticAttacks => true;

        #endregion

        public override Info GetInfo(CoreActor actor, bool baseValues)
            => MeleeWeaponInfoFactory.CreateDoubleMeleeWeaponInfo(actor, this, baseValues);

        public override bool IsLightWeapon
        {
            get
            {
                // NOTE: dual-wielding with one of the heads in a main-hand
                return IsDualWielding && AllHeads.Any(_h => !_h.IsOffHand);
            }
        }

        public override ActionTime SlottingTime => new ActionTime(Contracts.TimeType.Brief);
        public override bool SlottingProvokes => false;
        public override ActionTime UnslottingTime => new ActionTime(Contracts.TimeType.Brief);
        public override bool UnslottingProvokes => false;
    }
}
