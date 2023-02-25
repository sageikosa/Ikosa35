using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class NaturalItemTrait : TraitEffect, ITacticalActionProvider, IActionSource
    {
        public NaturalItemTrait(ITraitSource traitSource, ISlottedItem item, string slotType, bool twoSlots, ActionTime extractTime)
            : base(traitSource)
        {
            _SType = slotType;
            _Item = item;
            _TwoSlots = twoSlots;
            _Extract = extractTime;
        }

        #region state
        protected string _SType;
        protected ISlottedItem _Item;
        protected bool _TwoSlots;
        protected ActionTime _Extract;
        #endregion

        public string SlotType => _SType;

        public ISlottedItem Item => _Item;
        public ActionTime ExtractTime => _Extract;

        /// <summary>Gets item and deactivates the TraitEffect</summary>
        public ISlottedItem ExtractItem()
        {
            IsActive = false;
            var _item = _Item;
            _Item = null;
            return _item;
        }

        /// <summary>Probably only magic can do this</summary>
        public void ReplaceItem(ISlottedItem item)
        {
            if ((item?.SlotType.Equals(SlotType, StringComparison.OrdinalIgnoreCase) ?? false)
                && (_Item == null))
            {
                _Item = item;
            }
        }

        protected override bool OnPreActivate(object source)
            => Item != null;

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            var _slots = Item?.CreaturePossessor.Body.ItemSlots.AvailableSlots(Item).ToList();
            if (_slots?.Any() ?? false)
            {
                if (_TwoSlots && (_slots.Count > 1))
                {
                    Item.SetItemSlot(_slots[0], _slots[1]);
                }
                else
                {
                    Item.SetItemSlot(_slots.First());
                }
            }
        }

        protected override void OnDeactivate(object source)
        {
            Item?.ClearSlots();
            base.OnDeactivate(source);
        }

        public override object Clone()
            => new NaturalItemTrait(TraitSource, ((Item as ICloneable)?.Clone() as ISlottedItem),
                SlotType, _TwoSlots, ExtractTime);

        public override TraitEffect Clone(ITraitSource traitSource)
            => new NaturalItemTrait(traitSource, ((Item as ICloneable)?.Clone() as ISlottedItem),
                SlotType, _TwoSlots, ExtractTime);

        public bool IsContextMenuOnly => true;

        public IEnumerable<CoreAction> GetTacticalActions(CoreActionBudget budget)
        {
            // if creature is helpless, extraction is possible
            if (Creature.Conditions.Contains(Condition.Helpless)
                && (budget?.Actor != Creature))
            {
                // NOTE: when complete...trait will be non-functional...
                yield return new ExtractNaturalItem(this, ExtractTime, @"400");
            }
            yield break;
        }

        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
            => GetInfoData.GetInfoFeedback(Item, budget.Actor);

        public IVolatileValue ActionClassLevel => new Deltable(1);

    }
}
