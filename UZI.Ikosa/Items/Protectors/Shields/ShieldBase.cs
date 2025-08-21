using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Abilities;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Shields
{
    [Serializable]
    public abstract class ShieldBase : ProtectorItemBase, IShield, IActionProvider, IWieldMountable, IActionFilter
    {
        #region Construction
        protected ShieldBase(string name, bool tower, int bonus, int checkPenalty, int spellFailure, bool useHand)
            : base(name, ItemSlot.HoldingSlot, bonus, checkPenalty, spellFailure)
        {
            _Tower = tower;
            Sizer.NaturalSize = Size.Small;
            _UseHand = useHand;
        }
        #endregion

        /// <summary>Set up other shield qualities not accounted for in the constructor base</summary>
        protected virtual void Init(decimal basePrice, double weight, Materials.Material material, int baseStructure)
        {
            ItemMaterial = material;
            Price.CorePrice = basePrice;
            BaseWeight = weight;
            MaxStructurePoints.BaseValue = baseStructure;
            ProtectionBonus.BaseValue = (int)((decimal)ProtectionBonus.BaseValue * ItemSizer.ExpectedCreatureSize.ArmorBonusFactor);
            if (ProtectionBonus.BaseValue < 1)
            {
                ProtectionBonus.BaseValue = 1;
            }
        }

        #region private data
        protected bool _Tower;
        protected bool _UseHand;
        protected bool _Enabled = true;
        #endregion

        // shield properties
        public bool Tower => _Tower;
        public bool UseHandToCarry => _UseHand;
        public virtual int OpposedDelta => 0;

        #region protected (ICoreObject CoreObj, ItemSlot MainSlot, ItemSlot SecondSlot) GetReslotInfo(ItemSlot freeHand, ItemSlot trueSlot)
        protected (ICoreObject CoreObj, ItemSlot MainSlot, ItemSlot SecondSlot) GetReslotInfo(ItemSlot freeHand, ItemSlot trueSlot)
        {
            if (freeHand.SlottedItem != null)
            {
                // get slots for item in freehand after the freehand evaporates
                var _item = freeHand.SlottedItem;
                var _obj = _item.BaseObject;
                var _main = _item.MainSlot;
                _main = (_main == freeHand) ? trueSlot : _main;
                var _second = _item.SecondarySlot;
                _second = (_second == freeHand) ? trueSlot : _second;

                // unslot the item
                _item.ClearSlots();

                // move to true slots
                if (_second != null)
                {
                    return (_obj, _main, _second);
                }
                else
                {
                    return (_obj, _main, null);
                }
            }
            return (null, null, null);
        }
        #endregion

        #region protected void ReslotItem((ICoreObject CoreObj, ItemSlot MainSlot, ItemSlot SecondSlot) reslotInfo)
        protected void ReslotItem((ICoreObject CoreObj, ItemSlot MainSlot, ItemSlot SecondSlot) reslotInfo)
        {
            if (reslotInfo.CoreObj != null)
            {
                var _item = reslotInfo.CoreObj is ISlottedItem
                    ? reslotInfo.CoreObj as ISlottedItem
                    : new HoldingWrapper(CreaturePossessor, reslotInfo.CoreObj);
                if (reslotInfo.SecondSlot != null)
                {
                    _item.SetItemSlot(reslotInfo.MainSlot, reslotInfo.SecondSlot);
                }
                else
                {
                    _item.SetItemSlot(reslotInfo.MainSlot);
                }
            }
        }
        #endregion

        #region protected override void OnSetItemSlot()
        protected override void OnSetItemSlot()
        {
            base.OnSetItemSlot();
            if (MainSlot != null)
            {
                // modify the creature's AR
                Creature _creature = CreaturePossessor;
                if (!_creature.Proficiencies.IsProficientWith(this, _creature.AdvancementLog.NumberPowerDice))
                {
                    // DEX/STR skills not normally affected by armor check
                    _Penalty = new Delta(CheckPenalty.EffectiveValue, typeof(ShieldBase), @"Shield non-proficiency");
                    foreach (var _skill in from _s in _creature.Skills
                                           where (new string[] { MnemonicCode.Str, MnemonicCode.Dex }).Contains(_s.KeyAbilityMnemonic)
                                           && (_s.CheckFactor == 0)
                                           select _s)
                    {
                        _skill.Deltas.Add(_Penalty);
                    }
                }
                Enabled = true;
            }
        }
        #endregion

        // delta properties
        public override object Source
            => typeof(IShield);

        public override bool Enabled
        {
            get { return _Enabled; }
            set
            {
                _Enabled = value;
                DoValueChanged();
            }
        }

        #region IActionProvider Members
        public virtual IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            if (budget is LocalActionBudget _budget)
            {
                // TODO: shield bash

                // brief action to drop a shield
                if ((!(_budget.Actor is Creature _critter))
                    || (_critter.BaseAttack.EffectiveValue < 1)
                       || !(_budget.TopActivity?.Action is StartMove))
                {
                    yield return new DropHeldObject(MainSlot as HoldingSlot, TimeType.Brief, @"201");
                }
            }
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
            => GetInfoData.GetInfoFeedback(this, budget.Actor);
        #endregion

        public override Info GetInfo(CoreActor actor, bool baseValues)
        {
            var _info = ToProtectorInfo<ShieldInfo>(actor, baseValues);
            _info.IsTower = Tower;
            _info.UseHandToCarry = UseHandToCarry;
            return _info;
        }

        #region IWieldMountable Members

        public IEnumerable<string> SlotTypes
        {
            get
            {
                yield return ItemSlot.BackShieldMount;
                yield break;
            }
        }

        public WieldTemplate WieldTemplate => WieldTemplate.Light;

        #endregion

        #region IActionFilter Members

        public virtual bool SuppressAction(object source, CoreActionBudget budget, CoreAction action)
        {
            if (budget is LocalActionBudget _budget)
            {
                var _critter = _budget.Actor as Creature;
                // trying to drop this freely
                if ((action is DropHeldObject _drop)
                    && (_drop.ItemSlot == MainSlot)
                    && (_drop.TimeCost.ActionTimeType == TimeType.Free)
                    // but cannot confirm the creature is moving, or has a base attack bonus high enough to freely drop
                    && ((_critter == null) || (_critter.BaseAttack.EffectiveValue < 1)
                    || !(_budget.TopActivity?.Action is StartMove)))
                {
                    return false;
                }
            }
            return false;
        }

        #endregion

        public override ActionTime SlottingTime
            => new ActionTime(TimeType.Brief);
        public override ActionTime UnslottingTime
            => new ActionTime(TimeType.Brief);
        public override bool SlottingProvokes => false;
        public override bool UnslottingProvokes => false;
    }
}
