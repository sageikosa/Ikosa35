using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Uzi.Core
{
    /// <summary>Set of possessions for an actor (use CoreItem.Possessor{set;} to manage contents)</summary>
    [Serializable]
    public class PossessionSet : IEnumerable<CoreItem>, IControlChange<CoreItem>
    {
        #region construction
        public PossessionSet(CoreActor owner)
        {
            Owner = owner;
            _Items = [];
            _ChangeCtrlr = new ChangeController<CoreItem>(this, null);
        }
        #endregion

        #region state
        private readonly Dictionary<Guid, CoreItem> _Items;
        #endregion

        public CoreActor Owner { get; private set; }

        public CastType COwner<CastType>() where CastType : CoreActor
            => (CastType)Owner;

        public int Count
            => _Items.Count;

        /// <summary>Indicates whether the actor possesses the item</summary>
        /// <param name="id">Guid of the item</param>
        public bool Contains(Guid id)
            => _Items.ContainsKey(id);

        /// <summary>Indicates whether the actor possesses the item</summary>
        public bool Contains(ICoreItem item)
            => _Items.ContainsKey(item.ID);

        public IEnumerable<CoreItem> All
            => _Items.Select(_i => _i.Value);

        public CoreItem this[Guid id]
            => _Items.TryGetValue(id, out var _item) ? _item : null;

        protected virtual void OnAdd(CoreItem item) { DoPropertyChanged(nameof(All)); }
        protected virtual void OnRemove(CoreItem item) { DoPropertyChanged(nameof(All)); }

        protected void DoPropertyChanged(string propName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        #region internal void Add(CoreItem item)
        internal void Add(CoreItem item)
        {
            if (!_Items.ContainsValue(item))
            {
                _ChangeCtrlr.DoPreValueChanged(item, "Add");
                _Items.Add(item.ID, item);
                OnAdd(item);
                _ChangeCtrlr.DoValueChanged(item, "Add");
            }
        }
        #endregion

        #region internal void RemovedFrom(CoreItem item)
        internal void RemovedFrom(CoreItem item)
        {
            if (_Items.ContainsKey(item.ID))
            {
                _ChangeCtrlr.DoPreValueChanged(item, "Remove");
                _Items.Remove(item.ID);
                OnRemove(item);
                _ChangeCtrlr.DoValueChanged(item, "Remove");
            }
        }
        #endregion

        public IEnumerator<CoreItem> GetEnumerator()
        {
            foreach (KeyValuePair<Guid, CoreItem> _kvp in _Items)
            {
                yield return _kvp.Value;
            }

            yield break;
        }

        #region IEnumerable Members
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach (KeyValuePair<Guid, CoreItem> _kvp in _Items)
            {
                yield return _kvp.Value;
            }

            yield break;
        }
        #endregion

        #region IControlChange<BaseItem> Members
        private ChangeController<CoreItem> _ChangeCtrlr;
        public void AddChangeMonitor(IMonitorChange<CoreItem> monitor)
        {
            _ChangeCtrlr.AddChangeMonitor(monitor);
        }

        public void RemoveChangeMonitor(IMonitorChange<CoreItem> monitor)
        {
            _ChangeCtrlr.RemoveChangeMonitor(monitor);
        }
        #endregion

        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
