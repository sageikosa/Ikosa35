using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Uzi.Core
{
    [Serializable]
    // TODO: replace this with a Controllable set, using the IControlChange<> mechanism?
    public class WatchableSet<ItemType> : IEnumerable<ItemType> where ItemType : ISourcedObject
    {
        protected Collection<ItemType> _Items;

        public WatchableSet()
        {
            _Items = new Collection<ItemType>();
        }

        public S Item<S>() where S : class, ItemType
        {
            foreach (ItemType _itm in this._Items)
            {
                if (_itm.GetType().Equals(typeof(S)))
                {
                    return (S)_itm;
                }
            }
            return null;
        }

        #region Event Args Class
        public class ItemChangedEventArgs : EventArgs
        {
            public enum Direction
            {
                Added,
                Removed
            }

            public ItemChangedEventArgs(ItemType item, Direction dir)
            {
                this.Item = item;
                this.ChangeDirection = dir;
            }

            public readonly ItemType Item;
            public readonly Direction ChangeDirection;
        }
        #endregion

        #region ItemChanged Event
        protected void DoItemChanged(ItemType item, ItemChangedEventArgs.Direction direction)
        {
            if (this.ItemChanged != null)
            {
                ItemChanged(this, new ItemChangedEventArgs(item, direction));
            }
        }

        [field:NonSerialized, JsonIgnore]
        public event EventHandler<ItemChangedEventArgs> ItemChanged;
        #endregion

        #region ICollection<ItemType> Members
        public void Add(ItemType item)
        {
            _Items.Add(item);
            DoItemChanged(item, ItemChangedEventArgs.Direction.Added);
        }

        public bool Contains(ItemType item)
        {
            return _Items.Contains(item);
        }

        public int Count
        {
            get { return _Items.Count; }
        }

        public bool Remove(ItemType item)
        {
            DoItemChanged(item, ItemChangedEventArgs.Direction.Removed);
            return _Items.Remove(item);
        }
        #endregion

        #region IEnumerable<ItemType> Members
        public IEnumerator<ItemType> GetEnumerator()
        {
            return _Items.GetEnumerator();
        }
        #endregion

        #region IEnumerable Members
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _Items.GetEnumerator();
        }
        #endregion
    }
}
