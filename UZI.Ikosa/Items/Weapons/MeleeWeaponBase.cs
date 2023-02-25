using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Tactical;
using System.Linq;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Interactions;

namespace Uzi.Ikosa.Items.Weapons
{
    [Serializable]
    public abstract class MeleeWeaponBase : WeaponBase, IMeleeWeapon
    {
        #region Construction
        protected MeleeWeaponBase(string name, WieldTemplate wldTemplate, bool finessable)
            : base(name, ((wldTemplate == WieldTemplate.TwoHanded) || (wldTemplate == WieldTemplate.Double))
            ? Size.Medium : (wldTemplate == WieldTemplate.OneHanded) ? Size.Small : Size.Tiny)
        {
            _WieldTemplate = wldTemplate;
            _Finessable = finessable;
        }

        protected MeleeWeaponBase(string name, WieldTemplate wldTemplate, bool finessable, string itemSlot)
            : base(name, ((wldTemplate == WieldTemplate.TwoHanded) || (wldTemplate == WieldTemplate.Double))
            ? Size.Medium : (wldTemplate == WieldTemplate.OneHanded) ? Size.Small : Size.Tiny, itemSlot)
        {
            _WieldTemplate = wldTemplate;
            _Finessable = finessable;
        }
        #endregion

        #region private data
        protected bool _Finessable = false;
        protected IWeaponHead _MainHead;
        #endregion

        public virtual bool IsFinessable => _Finessable;
        public IWeaponHead MainHead => _MainHead;
        public int HeadCount => 1;
        public bool IsReachWeapon => this is IReachWeapon;
        public virtual bool HasPenaltyUsingLethalChoice => true;

        public virtual IEnumerable<IWeaponHead> AllHeads
        {
            get
            {
                yield return _MainHead;
                yield break;
            }
        }

        public override bool IsProficiencySuitable(Interaction interact)
            => (interact.InteractData is RangedAttackData)
            ? this.IsThrowable()
            : true;

        #region public override bool IsActive { get; }
        /// <summary>Active if slotted on both main and secondary slots, or if just the main slot if not a two-handed weapon.</summary>
        public override bool IsActive
        {
            get
            {
                var _template = GetWieldTemplate();
                switch (_template)
                {
                    case WieldTemplate.Unarmed:
                    case WieldTemplate.Light:
                    case WieldTemplate.OneHanded:
                        return (_MainSlot != null);

                    case WieldTemplate.TwoHanded:
                    case WieldTemplate.Double:
                        return (_MainSlot != null) && (_SecondarySlot != null);

                    case WieldTemplate.TooBig:
                    case WieldTemplate.TooSmall:
                    default:
                        return false;
                }
            }
        }
        #endregion

        #region protected override void OnSetItemSlot()
        /// <summary>possibly adds action provider and creates strike zone</summary>
        protected override void OnSetItemSlot()
        {
            if (IsActive)
            {
                // deltas are safe to attempt to add multiple times
                foreach (var _head in AllHeads)
                {
                    _head.CriticalRangeFactor.Deltas.Add(CreaturePossessor.CriticalRangeFactor);
                    _head.CriticalDamageFactor.Deltas.Add(CreaturePossessor.CriticalDamageFactor);
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
            foreach (var _head in AllHeads)
            {
                _head.CriticalRangeFactor.Deltas.Remove(CreaturePossessor.CriticalRangeFactor);
                _head.CriticalDamageFactor.Deltas.Remove(CreaturePossessor.CriticalDamageFactor);
            }
            this.RemoveStrikeZone();
            base.OnClearSlots(slotA, slotB);
        }
        #endregion

        public override bool IsTransferrable => true;

        public override IEnumerable<AttackActionBase> WeaponStrikes()
        {
            yield return new MeleeStrike(MainHead, Contracts.AttackImpact.Penetrating, @"101");
            if (this.IsThrowable())
                yield return new ThrowStrike(MainHead, this.GetThrowable(), AttackImpact.Penetrating, @"102");
            yield return new Disarm(MainHead, @"103");
            if (MainHead.DamageTypes.Any(_dt => _dt != DamageType.Piercing))
                yield return new SunderWieldedItem(MainHead, @"104");
            if (this.IsTrippingWeapon())
                yield return new Trip(MainHead, @"105");
            yield break;
        }

        #region IActionProvider Members
        public virtual IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
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

                // throwing (non-proficiency)
                if (!this.IsThrowable())
                {
                    // TODO: light/one-handed: standard action attack
                    // TODO: two-handed/double: full-round action attack
                }
            }
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
        {
            return GetInfoData.GetInfoFeedback(this, budget.Actor);
        }
        #endregion

        // IMeleeWeapon Members
        public Geometry GetStrikeZone(bool snapshot = false) => this.CreateStrikeGeometry(snapshot);
        public virtual bool OpportunisticAttacks => true;

        public override Info GetInfo(CoreActor actor, bool baseValues)
            => MeleeWeaponInfoFactory.CreateMeleeWeaponInfo(actor, this, baseValues);

        /// <summary>Wield template must be light or unarmed</summary>
        public override bool IsLightWeapon
        {
            get
            {
                switch (GetWieldTemplate())
                {
                    case WieldTemplate.Unarmed:
                    case WieldTemplate.Light:
                        return true;
                }
                return false;
            }
        }

        public override ActionTime SlottingTime => new ActionTime(Contracts.TimeType.Brief);
        public override bool SlottingProvokes => false;
        public override ActionTime UnslottingTime => new ActionTime(Contracts.TimeType.Brief);
        public override bool UnslottingProvokes => false;
    }
}
