using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items
{
    /// <summary>Allows resizing a slotted magic item</summary>
    [Serializable]
    public class ItemResizer : Adjunct, IActionProvider
    {
        /// <summary>Allows resizing a slotted magic item</summary>
        public ItemResizer(IActionSource source)
            : base(source)
        {
            _Offsetter = new Delta(0, typeof(ItemResizer));
        }

        #region data
        private Delta _Offsetter;
        #endregion

        #region hook/unhook size offset on activate/deactivate
        protected override void OnActivate(object source)
        {
            SlottedItem?.ItemSizer.SizeOffset.Deltas.Add(_Offsetter);
            base.OnActivate(source);
        }

        protected override void OnDeactivate(object source)
        {
            base.OnDeactivate(source);
            _Offsetter.DoTerminate();
        }
        #endregion

        public IActionSource ActionSource
            => Source as IActionSource;

        public ISlottedItem SlottedItem
            => Anchor as ISlottedItem;

        public Delta Offsetter => _Offsetter;

        public override object Clone()
            => new ItemResizer(ActionSource);

        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            // must be budget for the possessor
            if ((SlottedItem?.CreaturePossessor != null)
                && (budget?.Actor == SlottedItem?.CreaturePossessor))
            {
                // must be exposed to the creature possessing the item
                if (this.HasExposureTo(SlottedItem?.CreaturePossessor))
                {
                    // must be held by creature (not slotted)
                    if ((SlottedItem?.Adjuncts.OfType<Held>().FirstOrDefault())
                        ?.HoldingWrapper.CreaturePossessor == SlottedItem?.CreaturePossessor)
                    {
                        yield return new ItemResizeAction(this, @"202");
                    }
                }
            }
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
            => new AdjunctInfo(@"Item Resizer", ID);
    }

    [Serializable]
    public class ItemResizeAction : ActionBase
    {
        public ItemResizeAction(ItemResizer resizer, string orderKey)
            : base(resizer?.ActionSource, new ActionTime(TimeType.Free), false, false, orderKey)
        {
            _Resizer = resizer;
        }

        #region data
        private ItemResizer _Resizer;
        #endregion

        public override string DisplayName(CoreActor actor) => @"Command item to resize for possessor";
        public override string Key => @"ItemResizer.ItemResize";
        public ItemResizer ItemResizer => _Resizer;
        public ISlottedItem SlottedItem => ItemResizer.SlottedItem;
        public Creature Creature => SlottedItem.CreaturePossessor;

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            // none...implicit with item and possessor
            yield break;
        }

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => null;

        protected override ActivityResponse OnCanPerformActivity(CoreActivity activity)
            => new ActivityResponse((Creature != null)
                && (SlottedItem.ItemSizer.ExpectedCreatureSize.Order != Creature?.Sizer.Size.Order));

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;

        public override bool IsStackBase(CoreActivity activity)
            => false;

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            // magical offset resizing...
            ItemResizer.Offsetter.Value =
                (Creature?.Sizer.Size.Order ?? 0) - SlottedItem.ItemSizer.ExpectedCreatureSize.Order;
            return null;
        }
    }
}
