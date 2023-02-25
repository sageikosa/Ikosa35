using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Objects;
using Uzi.Core.Dice;

namespace Uzi.Ikosa.Items.Weapons.Natural
{
    [Serializable]
    public abstract class NaturalWeapon : WeaponBase, IMeleeWeapon, IActionProvider, IExoticWeapon, IQualifyDelta
    {
        #region Construction
        protected NaturalWeapon(string name, string slotType, bool finessable, Size damageSize, bool primary, bool alwaysOn, bool treatAsSole)
            : this(name, slotType, string.Empty, finessable, damageSize, primary, alwaysOn, treatAsSole)
        {
        }

        protected NaturalWeapon(string name, string slotType, string slotsubType, bool finessable, Size damageSize,
            Dictionary<int, Roller> damageRollers, bool primary, bool alwaysOn, bool treatAsSole)
            : this(name, slotType, slotsubType, finessable, damageSize, primary, alwaysOn, treatAsSole)
        {
            // custom damage rollers
            _DamageRollers = damageRollers;
        }

        protected NaturalWeapon(string name, string slotType, string slotsubType, bool finessable, Size damageSize, bool primary, bool alwaysOn, bool treatAsSole)
            : base(name, damageSize, slotType)
        {
            _Finessable = finessable;
            ProficiencyType = WeaponProficiencyType.Natural;
            _WieldTemplate = WieldTemplate.Unarmed;
            ItemMaterial = Materials.HideMaterial.Static;
            Weight = 0d;
            if (!primary)
                AddAdjunct(new NaturalSecondaryAdjunct(typeof(NaturalWeapon)));
            _SlotSub = slotsubType;
            _AlwaysOn = alwaysOn;
            _Sole = treatAsSole;
            _Term = new TerminateController(this);
        }
        #endregion

        /// <summary>Can be used by derived classes to generate weapon heads bound to the natural attack type</summary>
        protected WeaponBoundHead<WpnType> GetWeaponHead<WpnType>(string mediumDamage, int criticalLow, int criticalMultiplier,
            DamageType damageType) where WpnType : NaturalWeapon
        {
            if (_DamageRollers != null)
                return new WeaponBoundHead<WpnType>(this, mediumDamage, damageType, _DamageRollers, criticalLow, criticalMultiplier, ItemMaterial);
            return new WeaponBoundHead<WpnType>(this, mediumDamage, damageType, criticalLow, criticalMultiplier, ItemMaterial);
        }

        #region protected override bool FinalSlotCheck(ItemSlot slot)
        protected override bool FinalSlotCheck(ItemSlot slot)
        {
            if (slot != null)
                return String.IsNullOrEmpty(SlotSubType) || SlotSubType.Equals(slot.SubType, StringComparison.OrdinalIgnoreCase);
            return false;
        }
        #endregion

        #region private data
        private Dictionary<int, Roller> _DamageRollers = null;
        private bool _Finessable;
        protected IWeaponHead _MainHead;
        private bool _AlwaysOn;
        private string _SlotSub;
        private bool _Sole;
        private TerminateController _Term;
        #endregion

        public bool AlwaysOn => _AlwaysOn;
        public bool IsPrimary => !this.HasActiveAdjunct<NaturalSecondaryAdjunct>();
        public string SlotSubType => _SlotSub;
        public bool TreatAsSoleWeapon => _Sole;
        public bool IsReachWeapon => this is IReachWeapon;
        public override bool IsSunderable => false;
        public override bool IsActive => MainSlot != null;
        public IEnumerable<KeyValuePair<int, Roller>> DamageRollers
        {
            get
            {
                if (_DamageRollers != null)
                {
                    foreach (var _kvp in _DamageRollers)
                        yield return _kvp;
                }
                yield break;
            }
        }
        public virtual bool HasPenaltyUsingLethalChoice => true;

        public override bool IsTransferrable => false;

        #region protected override void OnSetItemSlot()
        protected override void OnSetItemSlot()
        {
            if (IsActive)
            {
                if (!CreaturePossessor.Actions.Providers.ContainsKey(this))
                {
                    CreaturePossessor.Actions.Providers.Add(this, (IActionProvider)this);
                    foreach (var _head in AllHeads)
                    {
                        _head.CriticalRangeFactor.Deltas.Add(CreaturePossessor.CriticalRangeFactor);
                        _head.CriticalDamageFactor.Deltas.Add(CreaturePossessor.CriticalDamageFactor);
                    }
                }
                CreaturePossessor.MeleeDeltable.Deltas.Add(this);
                CreaturePossessor.RangedDeltable.Deltas.Add(this);
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
                foreach (var _head in AllHeads)
                {
                    _head.CriticalRangeFactor.Deltas.Remove(CreaturePossessor.CriticalRangeFactor);
                    _head.CriticalDamageFactor.Deltas.Remove(CreaturePossessor.CriticalDamageFactor);
                }
            }
            this.RemoveStrikeZone();

            // remove qualify delta
            DoTerminate();

            base.OnClearSlots(slotA, slotB);
        }
        #endregion

        #region IMeleeWeapon Members
        public bool IsFinessable => _Finessable;
        public IWeaponHead MainHead => _MainHead;
        public int HeadCount => 1;
        public Geometry GetStrikeZone(bool snapshot = false) => this.CreateStrikeGeometry(snapshot);
        public virtual bool OpportunisticAttacks => true;
        #endregion

        #region public virtual IEnumerable<WeaponHead> AllHeads { get; }
        public virtual IEnumerable<IWeaponHead> AllHeads
        {
            get
            {
                yield return _MainHead;
                yield break;
            }
        }
        #endregion

        public override IEnumerable<AttackActionBase> WeaponStrikes()
        {
            yield return new MeleeStrike(MainHead, Contracts.AttackImpact.Penetrating, @"101");
            yield break;
        }

        #region IActionProvider Members
        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            if (IsActive)
            {
                var _budget = budget as LocalActionBudget;
                if (_budget.CanPerformRegular)
                    foreach (var _strike in WeaponStrikes())
                        yield return new RegularAttack(_strike);
            }
        }

        public Info GetProviderInfo(CoreActionBudget budget)
            => GetInfoData.GetInfoFeedback(this, budget.Actor);
        #endregion

        #region IQualifyDelta Members

        public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
        {
            if (!(qualify is Interaction _iAct))
                yield break;
            var _head = qualify.Source as IWeaponHead;
            if ((_iAct.InteractData is AttackData) && (_head == MainHead) && !IsPrimary)
            {
                yield return new QualifyingDelta(-5, typeof(NaturalSecondaryAdjunct), @"Secondary Natural Weapon");
            }
            yield break;
        }

        #endregion

        #region IControlTerminate Members

        public void DoTerminate()
        {
            _Term.DoTerminate();
        }

        public void AddTerminateDependent(IDependOnTerminate subscriber)
        {
            _Term.AddTerminateDependent(subscriber);
        }

        public void RemoveTerminateDependent(IDependOnTerminate subscriber)
        {
            _Term.RemoveTerminateDependent(subscriber);
        }

        public int TerminateSubscriberCount => _Term.TerminateSubscriberCount;

        #endregion

        #region public override Info GetInfos(CoreActor actor, bool baseValues)
        public override Info GetInfo(CoreActor actor, bool baseValues)
        {
            var _info = ObjectInfoFactory.CreateInfo<NaturalWeaponInfo>(actor, this, baseValues);
            _info.IsAlwaysOn = AlwaysOn;
            _info.IsPrimary = IsPrimary;
            _info.TreatAsSoleWeapon = TreatAsSoleWeapon;
            _info.IsReachWeapon = IsReachWeapon;
            _info.SlotType = SlotType;
            _info.SlotSubType = SlotSubType;
            _info.IsFinessable = IsFinessable;
            _info.WeaponHead = MainHead.ToWeaponHeadInfo(actor, baseValues);
            _info.WieldTemplate = GetWieldTemplate();
            return _info;
        }
        #endregion

        public override bool IsLightWeapon => true;

        public abstract NaturalWeapon Clone();

        public override ActionTime SlottingTime => new ActionTime(Contracts.TimeType.Free);
        public override bool SlottingProvokes => false;
        public override ActionTime UnslottingTime => new ActionTime(Contracts.TimeType.Free);
        public override bool UnslottingProvokes => false;

        public override bool IsProficiencySuitable(Interaction interact)
            => (interact.InteractData is RangedAttackData)
            ? false
            : true;
    }
}
