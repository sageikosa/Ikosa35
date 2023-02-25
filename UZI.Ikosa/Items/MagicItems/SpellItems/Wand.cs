using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Contracts;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Items
{
    /// <summary>
    /// Wand item, actions and spells are controlled by the SpellTrigger adjunct.
    /// </summary>
    [Serializable]
    public class Wand : SlottedItemBase, IMonitorChange<int>, IWieldMountable, IActionProvider
    {
        #region Construction
        public Wand(string name, Material itemMaterial, int maxHealthPoints, Size size)
            : base(@"Stick", ItemSlot.HoldingSlot)
        {
            InitItem(itemMaterial, maxHealthPoints, size);
            Name = name;
        }

        private void InitItem(Material itemMaterial, int maxHP, Size size)
        {
            Sizer.NaturalSize = size;
            ItemMaterial = itemMaterial;
            MaxStructurePoints.BaseValue = maxHP;
            // TODO: ... break DC of 16.
            BaseWeight = 0.1d;
        }
        #endregion

        #region public override bool AddAdjunct(Adjunct adjunct)
        /// <summary>Hook spell trigger battery</summary>
        public override bool AddAdjunct(Adjunct adjunct)
        {
            var _return = base.AddAdjunct(adjunct);
            if (adjunct is SpellTrigger)
            {
                SpellTrigger.PowerBattery.AddChangeMonitor(this);
            }
            return _return;
        }
        #endregion

        #region public override bool RemoveAdjunct(Adjunct adjunct)
        /// <summary>Unhook spell trigger battery</summary>
        public override bool RemoveAdjunct(Adjunct adjunct)
        {
            if (adjunct == SpellTrigger)
            {
                SpellTrigger.PowerBattery.RemoveChangeMonitor(this);
                var _return = base.RemoveAdjunct(adjunct);
                return _return;
            }
            else
            {
                return base.RemoveAdjunct(adjunct);
            }
        }
        #endregion

        public SpellTrigger SpellTrigger
            => Adjuncts.OfType<SpellTrigger>().FirstOrDefault();

        #region public override string Name { get; set; }
        public override string Name
        {
            get
            {
                var _trigger = SpellTrigger;
                if ((_trigger?.PowerBattery?.AvailableCharges ?? 0) > 0)
                {
                    return $"Wand of {_trigger.SpellSource.SpellDef.DisplayName} ({_trigger.SpellSource.CasterLevel})";
                }
                return base.Name;
            }
            set { }
        }
        #endregion

        #region public void SetPrice()
        public void SetPrice()
        {
            Price.DoCalcAugmentationPrice();
        }
        #endregion

        #region IMonitorChange<int> Members

        public void PreTestChange(object sender, AbortableChangeEventArgs<int> args) { }
        public void PreValueChanged(object sender, ChangeValueEventArgs<int> args) { }

        public void ValueChanged(object sender, ChangeValueEventArgs<int> args)
        {
            if (sender == SpellTrigger.PowerBattery)
                SetPrice();
        }

        #endregion

        #region IWieldMountable Members

        public IEnumerable<string> SlotTypes
        {
            get
            {
                yield return ItemSlot.WieldMount;
                yield break;
            }
        }

        public WieldTemplate WieldTemplate
            => WieldTemplate.Light;

        #endregion

        protected override string ClassIconKey
            => @"wand";

        public override bool IsTransferrable
            => true;

        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
            => SpellTrigger?.GetActions(budget) ?? new CoreAction[] { };

        public Info GetProviderInfo(CoreActionBudget budget)
            => GetInfoData.GetInfoFeedback(this, budget.Actor);

        public override Info MergeConnectedInfos(Info fetchedInfo, CoreActor actor)
        {
            // if we know about the spell trigger, we know about the number of charges...
            if (fetchedInfo is ObjectInfo _info
                && _info.AdjunctInfos.OfType<SpellTriggerInfo>()
                .FirstOrDefault(_sti => _sti.SpellSource.ID == SpellTrigger.SpellSource.ID) is SpellTriggerInfo _trigger)
            {
                _trigger.ChargesRemaining = SpellTrigger.PowerBattery.AvailableCharges;
            }
            return fetchedInfo;
        }

        public override ActionTime SlottingTime => new ActionTime(Contracts.TimeType.Brief);
        public override bool SlottingProvokes => false;
        public override ActionTime UnslottingTime => new ActionTime(Contracts.TimeType.Brief);
        public override bool UnslottingProvokes => false;
    }
}
