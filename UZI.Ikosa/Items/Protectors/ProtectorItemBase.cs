using System;
using Uzi.Core;
using Uzi.Ikosa.Deltas;
using Uzi.Ikosa.Contracts;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Objects;

namespace Uzi.Ikosa.Items
{
    /// <summary>
    /// Base class for Shields and Armors
    /// </summary>
    [Serializable]
    public abstract class ProtectorItemBase : SlottedItemBase, IProtectorItem
    {
        #region construction
        protected ProtectorItemBase(string name, string slotType, int bonus, int checkPenalty, int spellFailure)
            : base(name, slotType)
        {
            _ProtectBonus = new Deltable(bonus);
            _ProtectBonus.AddChangeMonitor(this);
            _TotalEnh = new ConstDeltable(0);
            _TotalEnh.AddChangeMonitor(this);
            _CheckPenalty = new ConstDeltable(checkPenalty);
            _ArcSpellFail = new ConstDeltable(spellFailure);
            _Terminator = new TerminateController(this);
            _ValueCtrlr = new ChangeController<DeltaValue>(this, new DeltaValue(bonus));
            _LastPrice = 0;
        }
        #endregion

        #region private data
        private Deltable _ProtectBonus;
        private ConstDeltable _TotalEnh;
        private ConstDeltable _CheckPenalty;
        private ConstDeltable _ArcSpellFail;
        private Delta _ArcDelta;
        private TerminateController _Terminator;
        private decimal _LastPrice;
        protected Delta _Penalty = null;
        #endregion

        #region IMonitorChange<DeltaValue> Members
        public override void ValueChanged(object sender, ChangeValueEventArgs<DeltaValue> args)
        {
            // NOTE: ItemBase is already setup to monitor DeltaValues, so must override
            if (sender == _ProtectBonus)
            {
                DoValueChanged();
            }
            else if (sender == _TotalEnh)
            {
                // cost of enhancements changed
                var _square = TotalEnhancement.EffectiveValue;
                _square *= _square;
                var _newPrice = 1000m * _square;

                // adjust base price cost
                Price.BaseItemExtraPrice += (_newPrice - _LastPrice);
                _LastPrice = _newPrice;
            }
            else
            {
                // call base so any other changes are tracked
                base.ValueChanged(sender, args);
            }
        }
        #endregion

        public virtual Deltable ProtectionBonus => _ProtectBonus;
        public ConstDeltable CheckPenalty => _CheckPenalty;
        public ConstDeltable ArcaneSpellFailureChance => _ArcSpellFail;
        public ConstDeltable TotalEnhancement => _TotalEnh;

        public decimal EnhancementCost
            => TotalEnhancement.EffectiveValue * TotalEnhancement.EffectiveValue * 1000m;

        public int ListedEnhancement
            => TotalEnhancement.Deltas[typeof(Enhancement)]?.Value ?? 0;

        #region IDelta Members
        public int Value
            => Enabled ? ProtectionBonus.EffectiveValue : 0;
        public abstract object Source { get; }
        public virtual bool Enabled { get { return true; } set { } }
        #endregion

        #region ValueChanged Event
        protected void DoValueChanged() { _ValueCtrlr.DoValueChanged(new DeltaValue(Value)); }

        #region IControlChange<DeltaValue> Members
        private ChangeController<DeltaValue> _ValueCtrlr;
        public void AddChangeMonitor(IMonitorChange<DeltaValue> subscriber)
        {
            _ValueCtrlr.AddChangeMonitor(subscriber);
        }

        public void RemoveChangeMonitor(IMonitorChange<DeltaValue> subscriber)
        {
            _ValueCtrlr.RemoveChangeMonitor(subscriber);
        }
        #endregion
        #endregion

        #region IControlTerminate Members
        /// <summary>Tells all modifiable values using this modifier to release it.  Note: this does not destroy the modifier and it can be re-used.</summary>
        public void DoTerminate() { _Terminator.DoTerminate(); }

        public void AddTerminateDependent(IDependOnTerminate subscriber)
        {
            _Terminator.AddTerminateDependent(subscriber);
        }

        public void RemoveTerminateDependent(IDependOnTerminate subscriber)
        {
            _Terminator.RemoveTerminateDependent(subscriber);
        }

        public int TerminateSubscriberCount => _Terminator?.TerminateSubscriberCount ?? 0;
        #endregion

        public override bool IsTransferrable
            => true;

        #region Slots
        protected override void OnClearSlots(ItemSlot slotA, ItemSlot slotB)
        {
            base.OnClearSlots(slotA, slotB);
            if (slotA != null)
            {
                DoTerminate();
            }
            _Penalty?.DoTerminate();
            _ArcDelta?.DoTerminate();
        }

        protected override void OnSetItemSlot()
        {
            base.OnSetItemSlot();
            if (MainSlot != null)
            {
                // modify the creature's AR
                Creature _creature = CreaturePossessor;
                _creature.NormalArmorRating.Deltas.Add(this);
                if (ArcaneSpellFailureChance.EffectiveValue > 0)
                {
                    _ArcDelta = new Delta(ArcaneSpellFailureChance.EffectiveValue, this);
                    _creature.ArcaneSpellFailureChance.Deltas.Add(_ArcDelta);
                }
            }
        }
        #endregion

        #region protected PInfo ToProtectorInfo<PInfo>(CoreActor actor, bool baseValues)
        protected PInfo ToProtectorInfo<PInfo>(CoreActor actor, bool baseValues)
            where PInfo : ProtectorInfo, new()
        {
            var _info = ObjectInfoFactory.CreateInfo<PInfo>(actor, this, baseValues);
            if (baseValues)
            {
                _info.ProtectionBonus = new DeltableInfo(ProtectionBonus.BaseValue);
                _info.CheckPenalty = new DeltableInfo(CheckPenalty.BaseValue);
                _info.ArcaneSpellFailureChance = new DeltableInfo(ArcaneSpellFailureChance.BaseValue);
            }
            else
            {
                _info.ProtectionBonus = ProtectionBonus.ToDeltableInfo();
                _info.CheckPenalty = CheckPenalty.ToDeltableInfo();
                _info.ArcaneSpellFailureChance = ArcaneSpellFailureChance.ToDeltableInfo();
            }
            return _info;
        }
        #endregion

        protected override bool FinalSlotCheck(ItemSlot slot)
            => slot.Creature.Sizer.Size.Order == ItemSizer.EffectiveCreatureSize.Order;
    }
}
