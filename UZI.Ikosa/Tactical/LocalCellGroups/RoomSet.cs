using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Uzi.Visualize;
using System.Collections.Specialized;
using Uzi.Packaging;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class RoomSet : Collection<Room>, INotifyCollectionChanged, ICorePart
    {
        // TODO: consider replacing this with a more controlled set for room manipulation cleanup
        public RoomSet(LocalMap map)
            : base()
        {
            _Map = map;
            _Index = new Dictionary<Guid, Room>();
        }

        #region state
        private LocalMap _Map;
        private Dictionary<Guid, Room> _Index;
        #endregion

        public LocalMap Map => _Map;

        #region collection overrides
        protected override void InsertItem(int index, Room item)
        {
            base.InsertItem(index, item);
            _Map.RoomIndex.Add(item);
            _Index[item.ID] = item;
            if (_Map.ContextSet.Count > 0)
                _Map.MapContext.CacheLocatorGroups();
            _Map.RelightMap();
            _Map.SignalMapChanged(this);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }

        protected override void RemoveItem(int index)
        {
            var _item = this[index];
            _Map.RoomIndex.Remove(_item);
            base.RemoveItem(index);
            _Index.Remove(_item.ID);
            if (_Map.ContextSet.Count > 0)
                _Map.MapContext.CacheLocatorGroups();
            _Map.RelightMap();
            _Map.SignalMapChanged(this);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, _item, index));
        }

        protected override void SetItem(int index, Room item)
        {
            var _old = this[index];
            _Map.RoomIndex.Remove(_old);
            base.SetItem(index, item);
            _Map.RoomIndex.Add(item);
            _Index[item.ID] = item;
            if (_Map.ContextSet.Count > 0)
                _Map.MapContext.CacheLocatorGroups();
            _Map.RelightMap();
            _Map.SignalMapChanged(this);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, _old, index));
        }
        #endregion

        public Room this[Guid id]
        {
            get
            {
                if (_Index == null)
                {
                    // fill if not defined
                    _Index = new Dictionary<Guid, Room>();
                    foreach (var _r in this)
                    {
                        _Index[_r.ID] = _r;
                    }
                }

                if (_Index.TryGetValue(id, out var _return))
                    return _return;
                return null;
            }
        }

        #region INotifyCollectionChanged Members
        [field:NonSerialized, JsonIgnore]
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        #endregion

        #region ICorePart Members

        public string Name { get { return @"Rooms"; } }

        public IEnumerable<ICorePart> Relationships
        {
            get { return this.AsEnumerable(); }
        }

        public string TypeName
        {
            get { return typeof(RoomSet).FullName; }
        }

        #endregion
    }
}
