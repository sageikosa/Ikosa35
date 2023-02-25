using System;
using Uzi.Core;

namespace Uzi.Ikosa.Items
{
    /// <summary>
    /// Used for adjuncts that anchor to slotted items.   
    /// Activated when the item is placed in a slot, and deactivated when unslotted.
    /// </summary>
    [Serializable]
    public abstract class SlotConnectedAugmentation : Adjunct, IMonitorChange<SlotChange>
    {
        /// <summary>
        /// Used for adjuncts that anchor to slotted items.   
        /// Activated when the item is placed in a slot, and deactivated when unslotted.
        /// </summary>
        protected SlotConnectedAugmentation(object source, bool affinity)
            : base(source)
        {
            _Affinity = affinity;
        }

        private bool _Affinity;

        /// <summary>Every slot connected augmentation can have affinity</summary>
        public bool Affinity => _Affinity;

        /// <summary>slot connected augmentations do not need similarty keys</summary>
        public string SimilarityKey => null;

        /// <summary>Override to define more stringent conditions for anchorage</summary>
        /// <param name="newAnchor"></param>
        protected virtual bool IsSlottedAnchorSuitable(IAdjunctable newAnchor)
            => newAnchor is ISlottedItem;

        public override bool CanAnchor(IAdjunctable newAnchor)
            => IsSlottedAnchorSuitable(newAnchor) && base.CanAnchor(newAnchor);

        public override bool CanUnAnchor()
            => OnPreDeactivate(this);

        protected ISlottedItem SlottedItem
            => Anchor as ISlottedItem;

        #region protected override void OnAnchorSet(IAdjunctable oldAnchor)
        protected override void OnAnchorSet(IAdjunctable oldAnchor, CoreSetting oldSetting)
        {
            if (oldAnchor != null)
            {
                // stop watching old slot
                var _oldItem = oldAnchor as ISlottedItem;
                _oldItem.RemoveChangeMonitor(this);
            }

            // start watching new slot
            if (SlottedItem != null)
                SlottedItem.AddChangeMonitor(this);
        }
        #endregion

        #region protected override void OnActivate(object source)
        protected override void OnActivate(object source)
        {
            // check if slotted when activated
            if (SlottedItem.MainSlot != null)
            {
                OnSlottedActivate();
            }
            base.OnActivate(source);
        }
        #endregion

        #region protected override void OnDeactivate(object source)
        protected override void OnDeactivate(object source)
        {
            // check if slotted when de-activated
            if (SlottedItem.MainSlot != null)
            {
                OnSlottedDeActivate();
            }
            base.OnDeactivate(source);
        }
        #endregion

        /// <summary>Adjunct is activated and item is slotted</summary>
        protected abstract void OnSlottedActivate();

        /// <summary>Adjunct is deactivated and item is no longer slotted</summary>
        protected abstract void OnSlottedDeActivate();

        #region IMonitorChange<SlotChange> Members
        /// <summary>This may be overridden in derived classes that may want to stop unslotting of items</summary>
        public virtual void PreTestChange(object sender, AbortableChangeEventArgs<SlotChange> args) { }

        public virtual void PreValueChanged(object sender, ChangeValueEventArgs<SlotChange> args) { }

        public void ValueChanged(object sender, ChangeValueEventArgs<SlotChange> args)
        {
            if (args.NewValue == SlotChange.Set)
            {
                // check if active when slotted
                if (IsActive)
                    OnSlottedActivate();
            }
            else
            {
                // check if active when unslotted
                if (IsActive)
                    OnSlottedDeActivate();
            }
        }
        #endregion
    }
}
